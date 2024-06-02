/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/Prompt.cs
 * PURPOSE:     Handle the Input, Entry point for all Commands
 *              Don't use in a multi thread Environment, not tested.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;
using ExtendedSystemObjects;

namespace Interpreter
{
    /// <inheritdoc cref="IDisposable" />
    /// <inheritdoc cref="IPrompt" />
    /// <summary>
    ///     Bucket List:
    ///     - Overloads
    ///     - nested Commands
    /// </summary>
    public sealed class Prompt : IPrompt, IDisposable
    {
        /// <summary>
        ///     Used to interpret Commands
        /// </summary>
        private static IrtPrompt _interpret;

        /// <summary>
        ///     The count
        /// </summary>
        private static int _count = -1;

        /// <summary>
        ///     User Input Windows
        /// </summary>
        private WindowPrompt _prompt;

        /// <summary>
        ///     The collected Namespaces
        /// </summary>
        public static Dictionary<string, UserSpace> CollectedSpaces { get; private set; }

        /// <summary>
        ///     Send selected Command to the Subscriber
        /// </summary>
        public EventHandler<OutCommand> SendCommands { get; set; }

        /// <summary>
        ///     Send Message to the Subscriber
        /// </summary>
        public EventHandler<string> SendLogs { get; set; }

        /// <summary>
        ///     Gets the log.
        /// </summary>
        /// <value>
        ///     The log.
        /// </value>
        public Dictionary<int, string> Log { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="Prompt" /> is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        public bool Disposed { get; set; }

        /// <summary>
        ///     Logs all activities
        /// </summary>
        private static int MaxLines => 1000;

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Start the Sender and Interpreter
        ///     UserSpace will never overwrite the System Commands.
        ///     Parenthesis are not defined by the UserSpace but by the definition of the User, so both ( or [ can be valid
        /// </summary>
        /// <param name="com">Command Register</param>
        /// <param name="userSpace">UserSpace of the register</param>
        /// <param name="extension">Optional Extension Methods</param>
        public void Initiate(Dictionary<int, InCommand> com, string userSpace,
            Dictionary<int, InCommand> extension = null)
        {
            ResetState();

            var use = new UserSpace { UserSpaceName = userSpace, Commands = com };

            //Upper is needed because of the way we compare commands in the Interpreter
            CollectedSpaces.AddDistinct(userSpace.ToUpper(), use);

            _interpret = new IrtPrompt(this);
            _interpret.Initiate(use);
            _interpret.SendLog += SendLog;
            _interpret.sendCommand += SendCommand;
        }

        /// <inheritdoc />
        /// <summary>Start the Sender and Interpreter</summary>
        /// <param name="com">Command Register</param>
        /// <param name="userSpace">UserSpace of the register</param>
        /// <param name="extension">Optional Extension Methods</param>
        public void AddCommands(Dictionary<int, InCommand> com, string userSpace,
            Dictionary<int, InCommand> extension = null)
        {
            if (CollectedSpaces.IsNullOrEmpty())
            {
                SendLogs?.Invoke(this, IrtConst.ErrorNotInitialized);
                return;
            }

            var use = CreateUserSpace(userSpace, com);
            CollectedSpaces.AddDistinct(userSpace, use);
            _interpret.SendLog += SendLog;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Start a Window for the Input.
        ///     Included and optional
        /// </summary>
        public void StartWindow()
        {
            _prompt = new WindowPrompt(_interpret) { ShowInTaskbar = true };
            _prompt.Show();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Handle it within the code, no User Input
        /// </summary>
        /// <param name="input">Input string</param>
        public void StartConsole(string input)
        {
            _interpret?.HandleInput(input);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Callback from the Outside, here for window
        /// </summary>
        /// <param name="message">Feedback Message for Display</param>
        public void CallbacksWindow(string message)
        {
            _prompt?.FeedbackMessage(message);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generic Callback from Outside, will appear in the window as SendLog Event
        /// </summary>
        /// <param name="message">Feedback Message for Display</param>
        public void Callback(string message)
        {
            SendLogs?.Invoke(nameof(Callback), message);
        }


        /// <summary>Switches the name spaces.</summary>
        /// <param name="space">The Namespace we would like to use.</param>
        internal static void SwitchNameSpaces(string space)
        {
            var use = CollectedSpaces[space];
            _interpret.Initiate(use);
        }

        /// <summary>
        ///     Return the selected Command
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void SendCommand(object sender, OutCommand e)
        {
            AddToLog(e.Command.ToString());
            SendCommands?.Invoke(nameof(Prompt), e);
        }

        /// <summary>
        ///     Send the News Out
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void SendLog(object sender, string e)
        {
            AddToLog(e);
            SendLogs?.Invoke(nameof(Prompt), e);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                _interpret = null;
                CollectedSpaces = null;
                Log = null;
                _prompt?.Close();
            }

            Disposed = true;
        }

        /// <summary>
        ///     Adds to log.
        /// </summary>
        /// <param name="message">The message.</param>
        private void AddToLog(string message)
        {
            if (_count == MaxLines)
            {
                Log.Remove(Log.Keys.First());
            }

            _count++;
            Log.Add(_count, message);
        }

        /// <summary>
        ///     Resets the state.
        /// </summary>
        private void ResetState()
        {
            CollectedSpaces = new Dictionary<string, UserSpace>();
            Log = new Dictionary<int, string>();
            _count = -1;
        }

        /// <summary>
        ///     Creates the user space.
        /// </summary>
        /// <param name="userSpace">The user space.</param>
        /// <param name="com">The COM.</param>
        /// <returns>New UserSpace</returns>
        private static UserSpace CreateUserSpace(string userSpace, Dictionary<int, InCommand> com)
        {
            var use = new UserSpace { UserSpaceName = userSpace, Commands = com };
            CollectedSpaces.AddDistinct(userSpace.ToUpper(), use);
            return use;
        }


        /// <summary>
        ///     NOTE: Leave out the finalizer altogether if this class doesn't
        ///     own unmanaged resources, but leave the other methods
        ///     exactly as they are.
        ///     Finalizes an instance of the <see cref="Prompt" /> class.
        /// </summary>
        ~Prompt()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}
