/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver
 * FILE:        Weave.cs
 * PURPOSE:     Command execution and extension management engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Core;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.ParseEngine;
using Weaver.ScriptEngine;

namespace Weaver
{
    /// <summary>
    /// Core command execution engine.
    /// Manages registration, lookup, execution, and feedback for commands with optional extensions.
    /// </summary>
    public sealed class Weave
    {
        /// <summary>
        /// Stores registered commands, keyed by (namespace, name, parameter count)
        /// </summary>
        private readonly Dictionary<(string ns, string name, int paramCount), ICommand> _commands
            = new();

        /// <summary>
        /// Stores per-command extension definitions
        /// </summary>
        private readonly Dictionary<(string ns, string name, int paramCount), Dictionary<string, int>>
            _commandExtensions
                = new();

        /// <summary>
        /// Built-in extensions injected into every command
        /// </summary>
        private static readonly Dictionary<string, CommandExtension> GlobalExtensions
            = new(StringComparer.OrdinalIgnoreCase)
            {
                [WeaverResources.GlobalExtensionHelp] = new CommandExtension
                    { Name = WeaverResources.GlobalExtensionHelp, ParameterCount = 0, IsInternal = true },
                [WeaverResources.GlobalExtensionTryRun] = new CommandExtension
                {
                    Name = WeaverResources.GlobalExtensionTryRun, ParameterCount = 0, IsInternal = true,
                    IsPreview = true
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
        /// The evaluator
        /// </summary>
        private IEvaluator _evaluator;

        /// <summary>
        /// The extensions
        /// </summary>
        private readonly List<ICommandExtension> _extensions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Weave"/> class.
        /// </summary>
        public Weave()
        {
            // Register internal commands
            var list = new ListCommand(() => GetCommands());
            Register(list);

            var help = new HelpCommand(() => GetCommands());
            Register(help);

            var print = new PrintCommand();
            Register(print);

            //our custom calculator command, needs the evaluator.
            _evaluator = new ExpressionEvaluator();
            var evaluate = new EvaluateCommand(_evaluator);
            Register(evaluate);

            _extensions.Add(new HelpExtension());
            _extensions.Add(new TryRunExtension());
        }

        /// <summary>
        /// Registers a command in the engine.
        /// Combines command-defined extensions with global extensions for internal use.
        /// </summary>
        /// <param name="command">The command to register.</param>
        public void Register(ICommand command)
        {
            var mergedExtensions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Include command-defined extensions
            if (command.Extensions != null)
            {
                foreach (var kv in command.Extensions)
                    mergedExtensions[kv.Key] = kv.Value;
            }

            // Include global extensions, skipping duplicates
            foreach (var kv in GlobalExtensions)
                if (!mergedExtensions.ContainsKey(kv.Key))
                    mergedExtensions[kv.Key] = kv.Value.ParameterCount;

            // Store extensions for internal lookup
            _commandExtensions[
                    (command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)] =
                mergedExtensions;

            // Store the command itself
            _commands[(command.Namespace.ToLowerInvariant(), command.Name.ToLowerInvariant(), command.ParameterCount)] =
                command;
        }

        /// <summary>
        /// Registers an external command extension with the runtime.
        /// These extensions are applied globally to all commands.
        /// </summary>
        /// <param name="extension">The extension to register.</param>
        public void RegisterExtension(ICommandExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            // Avoid duplicate registration
            if (_extensions.Any(e => e.Name.Equals(extension.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Extension '{extension.Name}' is already registered.");

            _extensions.Add(extension);
        }

        /// <summary>
        /// Retrieves all registered commands, optionally filtered by namespace.
        /// </summary>
        private IEnumerable<ICommand> GetCommands(string? ns = null)
        {
            return ns == null
                ? _commands.Values
                : _commands.Values.Where(c => c.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase));
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
        ///       <c>namespace:command()</c> or <c>namespace:command(arg1, ...)</c> – A namespaced command.
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
        ///       <c>namespace:command(...).Extension(...)</c> – Full namespaced command with extension and optional arguments.
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
            if (string.IsNullOrEmpty(raw))
                return CommandResult.Fail("Empty input.");

            // 1️⃣ Pending feedback
            if (_pendingFeedback?.IsPending == true)
            {
                // Verify the command using mediator
                var associatedCommand = Mediator.Resolve(_pendingFeedback.RequestId);
                if (associatedCommand == null)
                {
                    // Unexpected state
                    _pendingFeedback = null;
                    return CommandResult.Fail("Pending feedback has no associated command.");
                }

                var result = _pendingFeedback.Respond(raw);

                if (result.RequiresConfirmation) return result;

                Mediator.Clear(_pendingFeedback.RequestId);
                _pendingFeedback = null;

                return result;
            }

            // 2️⃣ Parse command
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
            if (cmdError != null)
                return cmdError;

            // 3️⃣ Handle extensions
            if (!string.IsNullOrEmpty(parsed.Extension))
            {
                // Find the extension associated with this command
                var (ext, error) = FindExtension(cmd, parsed.Extension, parsed.ExtensionArgs.Length);
                if (error != null)
                    return error; // Namespace mismatch or not found

                // Delegate execution completely to the extension
                // The extension may choose whether and how to invoke the command via 'executor'
                var result = ext?.Invoke(cmd, parsed.ExtensionArgs.Length > 0 ? parsed.ExtensionArgs : parsed.Args,
                                 cmd.Execute)
                             ?? cmd.InvokeExtension(parsed.Extension, parsed.Args);

                // Centralized feedback registration
                return HandleFeedback(result, cmd);
            }

            // 4️⃣ Normal execution
            var execResult = cmd.Execute(parsed.Args);
            if (execResult.RequiresConfirmation && execResult.Feedback != null)
            {
                _pendingFeedback = execResult.Feedback;

                // Register feedback with mediator
                Mediator.Register(cmd, _pendingFeedback);
            }

            return execResult;
        }

        /// <summary>
        /// Finds the best matching command for the given name, argument count, and optional namespace.
        /// Supports exact and variable parameter matches.
        /// </summary>
        /// <param name="name">The command name.</param>
        /// <param name="argCount">The number of arguments provided in the invocation.</param>
        /// <param name="ns">The optional namespace for namespaced commands.</param>
        /// <returns>
        /// The best matching <see cref="ICommand"/> if found, otherwise <c>null</c>.
        /// Returns a failed <see cref="CommandResult"/> for namespace mismatches.
        /// </returns>
        private (ICommand? Command, CommandResult? Error) FindCommand(string name, int argCount, string? ns = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (null, CommandResult.Fail("Command name cannot be empty."));

            name = name.ToLowerInvariant();

            // 1️⃣ If a namespace is specified, prefer a direct lookup first
            if (!string.IsNullOrWhiteSpace(ns))
            {
                ns = ns.ToLowerInvariant();

                // Try exact match first
                if (_commands.TryGetValue((ns, name, argCount), out var exact))
                    return (exact, null);

                // Try variable-parameter match
                if (_commands.TryGetValue((ns, name, 0), out var varargs))
                    return (varargs, null);

                return (null, CommandResult.Fail($"Command '{ns}:{name}' not found with {argCount} parameters."));
            }

            // 2️⃣ Otherwise, search all namespaces for a match
            var candidates = _commands.Values
                .Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (candidates.Count == 0)
                return (null, CommandResult.Fail($"Command '{name}' not found."));

            // Prefer exact arg count first
            var exactMatches = candidates
                .Where(c => c.ParameterCount == argCount)
                .ToList();

            if (exactMatches.Count == 1)
                return (exactMatches[0], null);

            if (exactMatches.Count > 1)
            {
                // TODO: Handle ambiguous overloads (e.g., different namespaces or ambiguous signatures)
                return (exactMatches[0], null);
            }

            // Then fallback to variable-parameter match
            var variableMatches = candidates
                .Where(c => c.ParameterCount == 0)
                .ToList();

            if (variableMatches.Count == 1)
                return (variableMatches[0], null);

            if (variableMatches.Count > 1)
            {
                // TODO: Handle ambiguous wildcard matches
                return (variableMatches[0], null);
            }

            return (null,
                CommandResult.Fail($"No suitable overload found for command '{name}' with {argCount} parameters."));
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
        /// A tuple containing the matching <see cref="ICommandExtension"/>, or a failure <see cref="CommandResult"/> if not found or invalid.
        /// </returns>
        private (ICommandExtension? Extension, CommandResult? Error) FindExtension(ICommand command,
            string extensionName, int argCount)
        {
            if (string.IsNullOrWhiteSpace(extensionName))
                return (null, CommandResult.Fail("No extension name provided."));

            // Normalize casing
            extensionName = extensionName.ToLowerInvariant();

            // 1️⃣ Check Global Extensions first
            if (GlobalExtensions.TryGetValue(extensionName, out var globalExt))
            {
                // Parameter check: 0 = variable
                if (globalExt.ParameterCount != 0 && globalExt.ParameterCount != argCount)
                    return (null, CommandResult.Fail(
                        $"Global extension '{extensionName}' expects {globalExt.ParameterCount} parameters, but got {argCount}."));

                // Find corresponding runtime extension implementation if available
                var wrapper = _extensions.FirstOrDefault(e =>
                    e.Name.Equals(extensionName, StringComparison.OrdinalIgnoreCase));

                // If none registered, allow global extension to be handled internally by the command
                return (wrapper, null);
            }

            // 2️⃣ Check registered runtime extensions
            var found = _extensions.FirstOrDefault(e =>
                e.Name.Equals(extensionName, StringComparison.OrdinalIgnoreCase));

            if (found == null)
                return (null, CommandResult.Fail($"Extension '{extensionName}' not found."));

            // 3️⃣ Namespace validation — must match if both define namespaces
            if (!string.IsNullOrEmpty(found.Namespace) &&
                !string.IsNullOrEmpty(command.Namespace) &&
                !found.Namespace.Equals(command.Namespace, StringComparison.OrdinalIgnoreCase))
            {
                return (null, CommandResult.Fail(
                    $"Namespace mismatch: Command '{command.Namespace}' cannot use extension '{found.Name}' from '{found.Namespace}'."));
            }

            // 4️⃣ Parameter count check (0 = variable)
            if (found.ExtensionParameterCount != 0 && found.ExtensionParameterCount != argCount)
            {
                return (null, CommandResult.Fail(
                    $"Extension '{found.Name}' expects {found.ExtensionParameterCount} parameters, but got {argCount}."));
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
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _pendingFeedback = null;
            Mediator.ClearAll();
        }
    }
}
