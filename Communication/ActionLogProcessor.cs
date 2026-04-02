/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        ActionLogProcessor.cs
 * PURPOSE:     Sample implementation of ILogProcessor that simply wraps an Action<string> delegate, 
 *              allowing users to easily provide a lambda or method group for log processing without needing to create a full class that implements ILogProcessor.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Threading.Tasks;
using Communication.Interfaces;

namespace Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Internal Helper Class to adapt the Action to the Interface
    /// </summary>
    /// <seealso cref="ILogProcessor" />
    internal sealed class ActionLogProcessor : ILogProcessor
    {
        /// <summary>
        /// The action
        /// </summary>
        private readonly Action<string> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionLogProcessor"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public ActionLogProcessor(Action<string> action)
        {
            _action = action;
        }

        /// <inheritdoc />
        /// <summary>
        /// Processes the received message (e.g., save to DB, log to file).
        /// </summary>
        /// <param name="message">The message received from the client.</param>
        /// <returns>Message task.</returns>
        public Task ProcessMessageAsync(string message)
        {
            // Run the user's lambda
            _action(message);
            return Task.CompletedTask;
        }
    }
}
