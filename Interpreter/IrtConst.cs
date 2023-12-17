/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtConst.cs
 * PURPOSE:     The Command Constants that are delivered with the dll
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace Interpreter
{
    /// <summary>
    ///     IrtConst contains all strings class.
    /// </summary>
    internal static class IrtConst
    {
        /// <summary>
        ///     The internal command container (const). Value: "CONTAINER".
        /// </summary>
        internal const string InternalCommandContainer = "CONTAINER";

        /// <summary>
        ///     The internal command batch execute (const). Value: "BATCHEXECUTE".
        /// </summary>
        internal const string InternalCommandBatchExecute = "BATCHEXECUTE";

        /// <summary>
        ///     The internal command help (const). Value: "HELP".
        /// </summary>
        internal const string InternalCommandHelp = "HELP";

        /// <summary>
        ///     The internal command list (const). Value: "LIST".
        /// </summary>
        internal const string InternalCommandList = "LIST";

        /// <summary>
        ///     The internal command using (const). Value: "USING".
        /// </summary>
        internal const string InternalUsing = "USING";

        /// <summary>
        ///     The internal command use (const). Value: "USE".
        /// </summary>
        internal const string InternalUse = "USE";

        /// <summary>
        ///     The error no commands provided (const). Value: "No Commands were provided".
        /// </summary>
        internal const string ErrorNoCommandsProvided = "No Commands were provided";

        /// <summary>
        ///     The error UserSpace not Found (const). Value: "Error UserSpace not found".
        /// </summary>
        internal const string ErrorUserSpaceNotFound = "Error UserSpace not found";

        /// <summary>
        ///     The error not initialized (const). Value: "Error please initiate the Prompt first.".
        /// </summary>
        internal const string ErrorNotInitialized = "Error please initiate the Prompt first.";

        /// <summary>
        ///     The error File not found (const). Value: "Error please initiate the Prompt first.".
        /// </summary>
        internal const string ErrorFileNotFound = "Error please initiate the Prompt first.";

        /// <summary>
        ///     The parenthesis error (const). Value: "Wrong parenthesis".
        /// </summary>
        internal const string ParenthesisError = "Wrong parenthesis";

        /// <summary>
        ///     The key word not found error (const). Value: "error KeyWord not Found: ".
        /// </summary>
        internal const string KeyWordNotFoundError = "error KeyWord not Found: ";

        /// <summary>
        ///     The syntax error (const). Value: "Error in the Syntax".
        /// </summary>
        internal const string SyntaxError = "Error in the Syntax";

        /// <summary>
        ///     The message info (const). Value: "Information: ".
        /// </summary>
        internal const string MessageInfo = "Information: ";

        /// <summary>
        ///     The message error (const). Value: "Error: ".
        /// </summary>
        internal const string MessageError = "Error: ";

        /// <summary>
        ///     The message warning (const). Value: "Warning: ".
        /// </summary>
        internal const string MessageWarning = "Warning: ";

        /// <summary>
        ///     The end (const). Value: ";".
        /// </summary>
        internal const string End = ";";

        /// <summary>
        ///     The Active (const). Value: "Active Using: ".
        /// </summary>
        internal const string Active = "Active Using: ";

        /// <summary>
        ///     The information startup (const). Value: "Interpreter started up".
        /// </summary>
        internal const string InformationStartup = "Interpreter started up";

        /// <summary>
        ///     The format description (const). Value: " Description: ".
        /// </summary>
        internal const string FormatDescription = " Description: ";

        /// <summary>
        ///     The format count (const). Value: " Parameter Count: ".
        /// </summary>
        internal const string FormatCount = " Parameter Count: ";

        /// <summary>
        ///     The error Parameter (const). Value: -1.
        /// </summary>
        internal const int ErrorParam = -1;

        /// <summary>
        ///     The open Clause, Standard is'('
        /// </summary>
        internal const char BaseOpen = '(';

        /// <summary>
        ///     The advanced open Clause, Standard is'{'
        /// </summary>
        internal const char AdvancedOpen = '{';

        /// <summary>
        ///     The close Clause, Standard is')'
        /// </summary>
        internal const char BaseClose = ')';

        /// <summary>
        ///     The advanced close Clause, Standard is'}'
        /// </summary>
        internal const char AdvancedClose = '}';

        /// <summary>
        ///     The Splitter Clause, Standard is','
        /// </summary>
        internal const char Splitter = ',';

        /// <summary>
        ///     The Splitter for a new Command, Standard is';'
        /// </summary>
        internal const char NewCommand = ';';

        /// <summary>
        ///     The internal commands
        /// </summary>
        internal static readonly List<string> InternalCommands = new()
        {
            InternalCommandHelp,
            InternalCommandList,
            InternalUsing,
            InternalUse,
            InternalCommandContainer,
            InternalCommandBatchExecute
        };

        /// <summary>
        ///     Basic internal Help
        /// </summary>
        internal static readonly string HelpGeneric = string.Concat(
            "Basic prompt, Version : 0.3. Author: Peter Geinitz (Wayfarer), not context sensitive",
            Environment.NewLine,
            "Type Help or Help(Keyword) for a specific help",
            Environment.NewLine,
            "Basic Syntax: Verb (Parameter, ...)",
            Environment.NewLine,
            "Basic Syntax: Verb (Parameter, ...)",
            Environment.NewLine,
            "System Commands:",
            Environment.NewLine,
            InternalCommandHelp, " : Basic help and specific help for provided Commands", Environment.NewLine,
            InternalCommandList, " : List all external Commands", Environment.NewLine,
            InternalUsing, " : Current Commands available and the one currently in use", Environment.NewLine,
            InternalUse, " : Type use(namespace) to switch to command namespace", Environment.NewLine,
            InternalCommandContainer,
            " : Holds a set of commands and executes them sequential, use Container{Command1; Command2; .... } and ; is the Separator that states that a new command follows."
        );
    }
}
