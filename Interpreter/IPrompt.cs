/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IPrompt.cs
 * PURPOSE:     The Prompt Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global, not used yet
// ReSharper disable UnusedMemberInSuper.Global

using System.Collections.Generic;

namespace Interpreter
{
    /// <summary>
    ///     The IPrompt interface.
    /// </summary>
    internal interface IPrompt
    {
        /// <summary>Start the Sender and Interpreter</summary>
        /// <param name="com">Command Register</param>
        /// <param name="userSpace">UserSpace of the register</param>
        /// <param name="extension">Optional Extension Methods</param>
        void Initiate(Dictionary<int, InCommand> com, string userSpace, Dictionary<int, InCommand> extension = null);

        /// <summary>Add further command Namespaces</summary>
        /// <param name="com">Command Register</param>
        /// <param name="userSpace">UserSpace of the register</param>
        /// <param name="extension">Optional Extension Methods</param>
        void AddCommands(Dictionary<int, InCommand> com, string userSpace, Dictionary<int, InCommand> extension = null);

        /// <summary>
        ///     Start the window. If we want to use the included window.
        /// </summary>
        void StartWindow();

        /// <summary>
        ///     The callbacks for Window, method messages to the window.
        /// </summary>
        /// <param name="message">The messages.</param>
        void CallbacksWindow(string message);

        /// <summary>
        ///     Callbacks the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Callback(string message);

        /// <summary>
        ///     Start the console.
        /// </summary>
        /// <param name="input">The input.</param>
        void StartConsole(string input);
    }
}
