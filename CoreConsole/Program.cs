/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CoreConsole
* FILE:        CoreConsole/Program.cs
* PURPOSE:     Basic Console app, to get my own tools running
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreBuilder;
using CoreBuilder.Interface;
using Interpreter;
using Interpreter.Resources;

namespace CoreConsole;

/// <summary>
///     Entry Point for my tools and future utilities.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     The prompt
    /// </summary>
    private static Prompt? _prompt;

    /// <summary>
    ///     The console lock
    /// </summary>
    private static readonly Lock ConsoleLock = new();

    /// <summary>
    ///     The is event triggered
    /// </summary>
    private static bool _isEventTriggered;

    /// <summary>
    ///     The extension
    /// </summary>
    private static ExtensionCommands? _ext;

    /// <summary>
    ///     Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    private static void Main(string?[] args)
    {
        if (args.Length < 2)
        {
            Initiate();
        }
        else
        {
            var operation = args[0];

            switch (operation)
            {
                case ConResources.ResourceHeader when args.Length == 2:
                    {
                        var directoryPath = args[1];
                        IHeaderExtractor headerExtractor = new HeaderExtractor();
                        var message = headerExtractor.ProcessFiles(directoryPath, true);
                        Console.WriteLine(message);
                        break;
                    }
                case ConResources.ResourceResxtract when args.Length >= 3:
                    {
                        var projectPath = args[1];
                        var outputResourceFile = args[2];
                        var ignoreList = new List<string>();
                        var ignorePatterns = new List<string>();
                        if (args.Length > 3 && File.Exists(args[3]))
                        {
                            ignoreList = new List<string>(File.ReadAllLines(args[3]));
                            Console.WriteLine(ConResources.MessageFilesIgnored, ignoreList.Count);
                        }

                        if (args.Length > 4 && File.Exists(args[4]))
                        {
                            foreach (var pattern in File.ReadAllLines(args[4]))
                            {
                                try
                                {
                                    ignorePatterns.Add(pattern);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ConResources.ErrorRegexpattern, pattern, ex.Message);
                                }
                            }

                            Console.WriteLine(ConResources.MessageOutputIgnore, ignorePatterns.Count);
                        }

                        IResourceExtractor resXtractExtractor = new ResXtract(ignoreList, ignorePatterns);
                        resXtractExtractor.ProcessProject(projectPath, outputResourceFile);
                        break;
                    }
                default:
                    Console.WriteLine(ConResources.InformationInvalidArgument);
                    break;
            }
        }

        Console.WriteLine(ConResources.ResourceInput);
        Console.ReadKey();
    }

    /// <summary>
    ///     Initiates this instance.
    /// </summary>
    private static void Initiate()
    {
        _prompt = new Prompt();
        _prompt.SendLogs += SendLogs!;
        _prompt.SendCommands += SendCommands!;
        _prompt.HandleFeedback += PromptHandleFeedback;
        _prompt.Callback(ConResources.MessageInfo);
        _prompt.Initiate(ConResources.CodeCommands, ConResources.UserSpaceCode, ConResources.ExtensionCommands);
        _prompt.AddCommands(ConResources.CodeCommands, ConResources.UserSpaceCode);
        _prompt.ConsoleInput(ConResources.ResourceUsingCmd);
        _prompt.Callback(Environment.NewLine);
        _prompt.ConsoleInput(ConResources.ResourceListCmd);

        while (true)
        {
            lock (ConsoleLock)
            {
                if (!_isEventTriggered)
                {
                    _prompt.Callback(ConResources.ResourceInput);
                    var input = Console.ReadLine();
                    _prompt.ConsoleInput(input!);
                }
                else
                {
                    _prompt.Callback(ConResources.ResourceEventWait);
                }
            }

            Thread.Sleep(500); // Small delay to prevent tight loop
        }
    }

    /// <summary>
    ///     Sends the commands.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The e.</param>
    private static void SendCommands(object sender, OutCommand e)
    {
        lock (ConsoleLock)
        {
            _isEventTriggered = true;
            // Simulate event processing
            _prompt?.Callback(ConResources.ResourceEventWait);
            _ = HandleCommandsAsync(e);
            _prompt?.Callback(ConResources.ResourceEventProcessing);
            _isEventTriggered = false;
        }
    }

    /// <summary>
    ///     Handles the commands.
    /// </summary>
    /// <param name="outCommand">The out command.</param>
    private static async Task HandleCommandsAsync(OutCommand outCommand)
    {
        switch (outCommand.Command)
        {
            case -1:
                _prompt?.Callback(outCommand.ErrorMessage);
                break;
            case 99:
                // Simulate some work
                _prompt?.Callback(ConResources.MessageClose);
                _prompt?.Dispose();
                // Introduce a small delay before closing
                await Task.Delay(3000); // Delay for 3000 milliseconds (3 seconds)
                // Close the console application
                Environment.Exit(0);
                break;
        }

        if (outCommand.ExtensionUsed)
        {
            await CheckExtension(outCommand);
        }

        string result;

        switch (outCommand.Command)
        {
            //Just show some stuff
            case ConResources.Header:
                result = ConsoleHelper.HandleHeader(outCommand);
                _prompt?.Callback(result);
                break;
            case ConResources.Resxtract:
                result = ConsoleHelper.HandleResxtract(outCommand);
                _prompt?.Callback(result);
                break;
            case ConResources.ResXtractOverload:
                result = ConsoleHelper.HandleResxtract(outCommand);
                _prompt?.Callback(result);
                break;
            case ConResources.Analyzer:
                var (_, output) = ConsoleHelper.RunAnalyzers(outCommand);
                _prompt?.Callback(output);
                break;
            case ConResources.DirAnalyzer:
                {
                    result = ConsoleHelper.HandleDirAnalyzer(outCommand);
                    _prompt?.Callback(result);
                    break;
                }
            default:
                _prompt?.Callback(ConResources.ErrorCommandNotFound);
                break;
        }
    }

    /// <summary>
    ///     Checks the extension.
    /// </summary>
    /// <param name="outCommand">The out command.</param>
    private static async Task CheckExtension(OutCommand outCommand)
    {
        string result;

        //check if the Command is contained.
        if (!ConResources.CodeCommands.ContainsKey(outCommand.Command) &&
            outCommand.Command != ConResources.Analyzer)
        {
            _prompt?.Callback("Error: Extension, for this command not supported.");
        }

        _ext = outCommand.ExtensionCommand;

        switch (outCommand.Command)
        {
            //Just show some stuff
            case ConResources.Header:
                result = ConsoleHelper.HandleHeaderTryrun(outCommand);
                _prompt?.Callback(result);
                break;
            case ConResources.Resxtract:
                result = ConsoleHelper.HandleResxtractTryrun(outCommand);
                _prompt?.Callback(result);
                break;
            case ConResources.ResXtractOverload:
                result = ConsoleHelper.HandleResxtractTryrun(outCommand);
                _prompt?.Callback(result);
                break;
        }

        // Wait a bit to let async logs finish before prompt
        await Task.Delay(200);

        _prompt?.ConsoleInput("confirm()");
    }


    /// <summary>
    ///     Prompts the handle feedback.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="IrtFeedbackInputEventArgs" /> instance containing the event data.</param>
    private static void PromptHandleFeedback(object? sender, IrtFeedbackInputEventArgs e)
    {
        switch (e.Answer)
        {
            case AvailableFeedback.Yes:
                {
                    var reconstructed = _ext.ExtensionParameter?.Count > 0
                        ? $"{_ext.BaseCommand}({string.Join(",", _ext.ExtensionParameter)})"
                        : $"{_ext.BaseCommand}";

                    _prompt?.ConsoleInput(reconstructed);
                    break;
                }
            case AvailableFeedback.No:
            case AvailableFeedback.Cancel:
                _prompt?.Callback("Operation canceled");
                break;
            default:
                _prompt?.Callback(ConResources.InformationInvalidArgument);
                break;
        }
    }

    /// <summary>
    ///     Listen to Messages
    /// </summary>
    /// <param name="sender">Object</param>
    /// <param name="e">Type</param>
    private static void SendLogs(object sender, string e)
    {
        lock (ConsoleLock)
        {
            _isEventTriggered = true;
            //_prompt?.Callback(e);
            Console.WriteLine(e);
            _isEventTriggered = false;
        }
    }
}
