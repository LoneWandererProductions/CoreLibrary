/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        Weave.cs
 * PURPOSE:     Command execution and extension management engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Core.Commands;
using Weaver.Core.Extensions;
using Weaver.Evaluate;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ParseEngine;

namespace Weaver
{
    /// <summary>
    /// Core command execution engine.
    /// Manages registration, lookup, execution, and feedback for commands with optional extensions.
    /// </summary>
    public sealed class Weave
    {
        /// <summary>
        /// The synchronization lock object.
        /// We use a private object to prevent deadlocks from external locking on 'this'.
        /// </summary>
        private readonly object _syncRoot = new();

        /// <summary>
        /// Stores registered commands, keyed by (namespace, name, parameter count)
        /// </summary>
        private readonly Dictionary<(string ns, string name, int paramCount), ICommand> _commands
            = new();

        /// <summary>
        /// Built-in extensions injected into every command
        /// </summary>
        private static readonly Dictionary<string, CommandExtension> GlobalExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [WeaverResources.GlobalExtensionHelp] = new CommandExtension
                    { Name = WeaverResources.GlobalExtensionHelp, ParameterCount = 0, IsInternal = true },
                [WeaverResources.GlobalExtensionTryRun] = new CommandExtension
                {
                    Name = WeaverResources.GlobalExtensionTryRun, ParameterCount = 0, IsInternal = true,
                    IsPreview = true
                },
                [WeaverResources.GlobalExtensionStore] = new CommandExtension
                {
                    Name = WeaverResources.GlobalExtensionStore, ParameterCount = -1, IsInternal = true,
                    IsPreview = false
                },
                [WeaverResources.GlobalExtensionClean] = new CommandExtension
                {
                    Name = WeaverResources.GlobalExtensionClean,
                    ParameterCount = -1,
                    IsInternal = true,
                    IsPreview = false
                }
            };

        /// <summary>
        /// The mediator
        /// </summary>
        public readonly MessageMediator Mediator = new();

        /// <summary>
        /// The pending feedback
        /// </summary>
        private FeedbackRequest? _pendingFeedback;

        /// <summary>
        /// The extensions
        /// </summary>
        private readonly Dictionary<string, ICommandExtension> _extensions = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the IVariableRegistry runtime.
        /// </summary>
        /// <value>
        /// The runtime.
        /// </value>
        public WeaveRuntime Runtime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Weave" /> class.
        /// </summary>
        /// <param name="runtime">Optional: The runtime environment to use. If null, a new one is created.</param>
        public Weave(WeaveRuntime? runtime = null)
        {
            // Use the injected runtime, or create a fresh default one
            Runtime = runtime ?? new WeaveRuntime();

            RegisterDefaults();
        }

        /// <summary>
        /// Registers a command in the engine.
        /// Combines command-defined extensions with global extensions for internal use.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void Register(ICommand command)
        {
            // Lock ensures we don't modify the dictionary while ProcessInput is reading it
            lock (_syncRoot)
            {
                var key = (command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(),
                    command.ParameterCount);
                _commands[key] = command;
            }
        }

        /// <summary>
        /// Registers an external command extension with the runtime.
        /// These extensions are applied globally to all commands.
        /// </summary>
        /// <param name="extension">The extension to register.</param>
        public void RegisterExtension(ICommandExtension extension)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));

            lock (_syncRoot)
            {
                if (_extensions.ContainsKey(extension.Name))
                    throw new InvalidOperationException($"Extension '{extension.Name}' is already registered.");

                _extensions[extension.Name] = extension;
            }
        }

        /// <summary>
        /// Retrieves all registered commands, optionally filtered by namespace.
        /// </summary>
        private IEnumerable<ICommand> GetCommands(string? ns = null)
        {
            // Used by ListCommand/HelpCommand.
            // Since those run inside ProcessInput -> Command.Execute, the _syncRoot is already held.
            // C# locks are re-entrant, so this is safe.
            lock (_syncRoot)
            {
                return ns == null
                    ? _commands.Values.ToList() // Return a copy to prevent "Collection Modified" errors
                    : _commands.Values.Where(c => c.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        /// <summary>
        /// Retrieves all registered command extensions, optionally filtered by namespace.
        /// </summary>
        /// <param name="ns">The optional namespace filter.</param>
        /// <returns>A list of registered extensions.</returns>
        public List<ICommandExtension> GetExtensions(string? ns = null)
        {
            lock (_syncRoot)
            {
                return ns == null
                    ? _extensions.Values.ToList()
                    : _extensions.Values.Where(e => e.Namespace != null && e.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        /// <summary>
        /// Processes a raw command input string and executes the corresponding command.
        /// </summary>
        /// <param name="raw">The raw command input.</param>
        /// <returns>
        /// A <see cref="CommandResult" /> containing the result of execution, or a failure message if the input is invalid.
        /// </returns>
        /// <remarks>
        /// <para>The supported command syntax is:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <c>command()</c> – A command with zero arguments.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>command(arg1, arg2, ...)</c> – A command with one or more arguments, separated by commas.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>namespace:command()</c> or <c>namespace:command(arg1, ...)</c> – A namespace command.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>command(...).Extension()</c> – An optional extension applied to a command, with optional extension arguments.
        /// Extensions are invoked after the main command executes.
        /// </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <c>namespace:command(...).Extension(...)</c> – Full namespace command with extension and optional arguments.
        /// </description>
        ///   </item>
        /// </list>
        /// <para>Notes:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>Parentheses are required for both commands and extensions, even if empty.</description>
        ///   </item>
        ///   <item>
        ///     <description>Arguments may be quoted with single or double quotes to allow commas inside a single argument.</description>
        ///   </item>
        ///   <item>
        ///     <description>Only one extension is supported per command in the current parser.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        public CommandResult ProcessInput(string raw)
        {
            raw = raw.Trim();
            if (string.IsNullOrEmpty(raw)) return CommandResult.Fail("Empty input.");

            // CRITICAL SECTION:
            // We lock the entire execution pipeline. This ensures that checking for 
            // _pendingFeedback and executing the command happens as one atomic unit.
            lock (_syncRoot)
            {
                // 1. Pending Feedback
                if (_pendingFeedback?.IsPending == true)
                {
                    var associatedCommand = Mediator.Resolve(_pendingFeedback.RequestId);
                    if (associatedCommand == null)
                    {
                        _pendingFeedback = null;
                        return CommandResult.Fail("Pending feedback has no associated command.");
                    }

                    var result = _pendingFeedback.Respond(raw);

                    // If still waiting for more input (multi-stage feedback), return immediately
                    if (result.RequiresConfirmation) return result;

                    Mediator.Clear(_pendingFeedback.RequestId);
                    _pendingFeedback = null;
                    return result;
                }

                // 2. Parse
                ParsedCommand parsed;
                try
                {
                    parsed = SimpleCommandParser.Parse(raw);
                }
                catch (Exception ex)
                {
                    return CommandResult.Fail($"Syntax error: {ex.Message}");
                }

                var (cmd, cmdError) = FindCommand(parsed.Name, parsed.Args.Length, parsed.Namespace);
                if (cmdError != null) return cmdError;

                // 3. Extensions
                if (!string.IsNullOrEmpty(parsed.Extension))
                {
                    var (ext, error) = FindExtension(cmd!, parsed.Extension, parsed.ExtensionArgs.Length);
                    if (error != null) return error;

                    if (ext != null)
                    {
                        var result = ext.Invoke(cmd!, parsed.ExtensionArgs, cmd!.Execute, parsed.Args);
                        return HandleFeedback(result, cmd!);
                    }

                    return CommandResult.Fail($"Unknown extension '{parsed.Extension}' for command '{cmd!.Name}'.");
                }

                // 4. Execution
                var execResult = cmd!.Execute(parsed.Args);
                return HandleFeedback(execResult, cmd);
            }
        }

        /// <summary>
        /// Finds the best matching command for the given name, argument count, and optional namespace.
        /// Supports exact and variable parameter matches.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="argCount">The number of arguments provided in the invocation.</param>
        /// <param name="ns">The optional namespace for namespace commands.</param>
        /// <returns>
        /// The best matching <see cref="ICommand"/> if found, otherwise <c>null</c>.
        /// Returns a failed <see cref="CommandResult"/> for namespace mismatches.
        /// </returns>
        private (ICommand? Command, CommandResult? Error) FindCommand(string name, int argCount, string? ns = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (null, CommandResult.Fail("Command name cannot be empty."));

            name = name.ToLowerInvariant();

            IEnumerable<ICommand> candidates;

            // 1️⃣ Namespace filtering
            if (!string.IsNullOrWhiteSpace(ns))
            {
                ns = ns.ToLowerInvariant();
                candidates = _commands.Values
                    .Where(c => c.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase) &&
                                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                candidates = _commands.Values
                    .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            var list = candidates.ToList();
            if (list.Count == 0)
                return (null, CommandResult.Fail(
                    ns == null ? $"Command '{name}' not found." : $"Command '{ns}:{name}' not found."));

            // 2️⃣ Exact match always wins
            var exact = list.FirstOrDefault(c => c.ParameterCount > 0 && c.ParameterCount == argCount);
            if (exact != null)
                return (exact, null);

            // 3️⃣ Optional (0 or 1)
            if (argCount <= 1)
            {
                var optional = list.FirstOrDefault(c => c.ParameterCount == -1);
                if (optional != null)
                    return (optional, null);
            }

            // 4️⃣ Fully variadic
            var variadic = list.FirstOrDefault(c => c.ParameterCount == 0);
            if (variadic != null)
                return (variadic, null);

            // 5️⃣ Error
            return (null, CommandResult.Fail(
                ns == null
                    ? $"No suitable overload found for command '{name}' with {argCount} parameters."
                    : $"No suitable overload found for command '{ns}:{name}' with {argCount} parameters."));
        }


        /// <summary>
        /// Finds a matching extension for a given command.
        /// Searches global extensions first, then runtime-registered extensions.
        /// Validates namespace and parameter compatibility.
        /// </summary>
        /// <param name="command">The command requesting the extension.</param>
        /// <param name="extensionName">The name of the extension.</param>
        /// <param name="argCount">The number of arguments passed to the extension.</param>
        /// <returns>
        /// A tuple containing the matching <see cref="ICommandExtension" />, or a failure <see cref="CommandResult" /> if not found or invalid.
        /// </returns>
        private (ICommandExtension? Extension, CommandResult? Error) FindExtension(ICommand command,
            string extensionName, int argCount)
        {
            if (string.IsNullOrWhiteSpace(extensionName))
                return (null, CommandResult.Fail("No extension name provided."));

            extensionName = extensionName.ToLowerInvariant();

            // 1. Check Global Definitions
            if (GlobalExtensions.TryGetValue(extensionName, out var globalExt))
            {
                if (globalExt.ParameterCount != 0 && globalExt.ParameterCount != -1)
                {
                    if (globalExt.ParameterCount != argCount)
                        return (null,
                            CommandResult.Fail(
                                $"Global extension '{extensionName}' expects {globalExt.ParameterCount} parameters, but got {argCount}."));
                }

                // O(1) Lookup from registry
                return _extensions.TryGetValue(extensionName, out var impl)
                    ? (impl, null)
                    : (null, null); // Global def exists, but no implementation registered
            }

            // 2. Check Registered Extensions
            if (!_extensions.TryGetValue(extensionName, out var found))
                return (null, CommandResult.Fail($"Extension '{extensionName}' not found."));

            // 3. Namespace Validation
            if (!string.IsNullOrEmpty(found.Namespace) &&
                !string.IsNullOrEmpty(command.Namespace) &&
                !found.Namespace.Equals(command.Namespace, StringComparison.OrdinalIgnoreCase))
            {
                return (null,
                    CommandResult.Fail(
                        $"Namespace mismatch: Command '{command.Namespace}' cannot use extension '{found.Name}' from '{found.Namespace}'."));
            }

            // 4. Parameter Validation
            switch (found.ExtensionParameterCount)
            {
                case 0: break;
                case -1:
                    if (argCount > 1)
                        return (null,
                            CommandResult.Fail(
                                $"Extension '{found.Name}' expects zero or one parameter, but got {argCount}."));
                    break;
                default:
                    if (found.ExtensionParameterCount != argCount)
                        return (null,
                            CommandResult.Fail(
                                $"Extension '{found.Name}' expects {found.ExtensionParameterCount} parameters, but got {argCount}."));
                    break;
            }

            return (found, null);
        }

        /// <summary>
        /// Registers pending feedback with the mediator if required, and returns the same result.
        /// </summary>
        private CommandResult HandleFeedback(CommandResult result, ICommand cmd)
        {
            if (result.RequiresConfirmation && result.Feedback != null)
            {
                _pendingFeedback = result.Feedback;
                Mediator.Register(cmd, _pendingFeedback);
            }

            return result;
        }

        /// <summary>
        /// Registers the defaults.
        /// </summary>
        private void RegisterDefaults()
        {
            // Core Commands
            Register(new ListCommand(() => GetCommands(), () => GetExtensions()));
            Register(new HelpCommand(() => GetCommands(), () => GetExtensions()));
            Register(new PrintCommand());
            Register(new LoadPluginCommand(this));
            Register(new EvaluateCommand(Runtime.Evaluator, Runtime.Variables));

            // Variable/Memory Commands
            Register(new SetValueCommand(Runtime.Variables, Runtime.Evaluator));
            Register(new GetValueCommand(Runtime.Variables));
            Register(new IncCommand(Runtime.Variables));
            Register(new DecCommand(Runtime.Variables));
            Register(new DeleteValueCommand(Runtime.Variables));
            Register(new MemoryCommand(Runtime.Variables));
            Register(new ScriptCommand(Runtime.Variables));
            Register(new MemClearCommand(Runtime.Variables));

            // Extensions
            RegisterExtension(new HelpExtension());
            RegisterExtension(new TryRunExtension());
            RegisterExtension(new StoreExtension(Runtime.Variables));
            RegisterExtension(new ScriptStepperExtension(Runtime.Variables));
            RegisterExtension(new CleanExtension());
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            lock (_syncRoot)
            {
                _pendingFeedback = null;
                Mediator.ClearAll();
            }
        }
    }
}