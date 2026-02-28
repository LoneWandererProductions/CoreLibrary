/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        MessageMediator.cs
 * PURPOSE:     Mediates messages and feedback requests between commands and the script executor.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Concurrent;
using Weaver.Interfaces;

namespace Weaver.Messages
{
    /// <summary>
    /// Mediator between User and Script Executor for commands and feedback requests.
    /// </summary>
    public sealed class MessageMediator
    {
        /// <summary>
        /// The pending
        /// </summary>
        private readonly ConcurrentDictionary<string, ICommand> _pending = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registers the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="feedback">The feedback.</param>
        /// <exception cref="System.ArgumentNullException">feedback</exception>
        public void Register(ICommand command, FeedbackRequest feedback)
        {
            if (feedback == null)
                throw new ArgumentNullException(nameof(feedback));

            _pending[feedback.RequestId] = command;
        }

        /// <summary>
        /// Resolves the specified request identifier.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        /// <returns></returns>
        public ICommand? Resolve(string requestId)
        {
            return _pending.TryGetValue(requestId, out var cmd) ? cmd : null;
        }

        /// <summary>
        /// Clears the specified request identifier.
        /// </summary>
        /// <param name="requestId">The request identifier.</param>
        public void Clear(string requestId)
        {
            _pending.TryRemove(requestId, out _);
        }

        /// <summary>
        /// Clears all.
        /// </summary>
        public void ClearAll()
        {
            _pending.Clear();
        }
    }
}