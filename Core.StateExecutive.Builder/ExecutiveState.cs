/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Builder
 * FILE:        ExecutiveState.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive.Interfaces;

namespace Core.StateExecutive.Builder
{
    /// <inheritdoc />
    /// <summary>
    /// Stat eof the State Executive Engine. Each state has a unique ID and a list of transitions to other states.
    /// </summary>
    /// <seealso cref="Core.StateExecutive.Interfaces.IState" />
    internal sealed class ExecutiveState : IState
    {
        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public IEnumerable<ITransition> Transitions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutiveState"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="transitions">The transitions.</param>
        public ExecutiveState(string id, IEnumerable<ITransition> transitions)
        {
            Id = id;
            Transitions = transitions;
        }
    }
}
