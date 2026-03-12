/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Builder
 * FILE:        StateBuilder.cs
 * PURPOSE:     Generates states for the State Executive Engine. Each state has a unique ID and a list of transitions to other states.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive.Interfaces;

namespace Core.StateExecutive.Builder
{
    /// <summary>
    /// StateBuilder generates states for the State Executive Engine. Each state has a unique ID and a list of transitions to other states.
    /// </summary>
    public sealed class StateBuilder
    {
        /// <summary>
        /// The identifier
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// The transitions
        /// </summary>
        private readonly List<ITransition> _transitions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StateBuilder"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="System.ArgumentNullException">id</exception>
        private StateBuilder(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>
        /// Creates the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>New Transition Builder</returns>
        public static StateBuilder Create(string id)
        {
            return new StateBuilder(id);
        }

        /// <summary>
        /// Transitions to.
        /// </summary>
        /// <param name="targetStateId">The target state identifier.</param>
        /// <returns>New Transition Builder</returns>
        public TransitionBuilder TransitionTo(string targetStateId)
        {
            return new TransitionBuilder(targetStateId, this);
        }

        /// <summary>
        /// Adds the transition.
        /// </summary>
        /// <param name="transition">The transition.</param>
        internal void AddTransition(ITransition transition)
        {
            _transitions.Add(transition);
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>Satus object of build.</returns>
        public IState Build()
        {
            return new ExecutiveState(_id, _transitions.ToList());
        }
    }
}
