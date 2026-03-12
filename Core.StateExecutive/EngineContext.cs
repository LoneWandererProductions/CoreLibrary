/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive
 * FILE:        EngineContext.cs
 * PURPOSE:     The EngineContext class manages the current state and transitions of the State Executive Engine. 
 *              It allows registering states, setting the initial state, and evaluating transitions based on external events and a shared blackboard context.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive.Interfaces;

namespace Core.StateExecutive
{
    /// <summary>
    /// EngineContext manages the current state and transitions of the State Executive Engine.
    /// </summary>
    public class EngineContext
    {
        /// <summary>
        /// The states
        /// </summary>
        private readonly Dictionary<string, IState> _states = new();

        /// <summary>
        /// Gets the state of the current.
        /// </summary>
        /// <value>
        /// The state of the current.
        /// </value>
        public IState CurrentState { get; private set; }

        /// <summary>
        /// Registers the state.
        /// </summary>
        /// <param name="state">The state.</param>
        public void RegisterState(IState state)
        {
            _states[state.Id] = state;
        }

        /// <summary>
        /// Sets the initial state.
        /// </summary>
        /// <param name="stateId">The state identifier.</param>
        public void SetInitialState(string stateId)
        {
            CurrentState = _states[stateId];
        }

        /// <summary>
        /// Call this when external events happen to see if the state should change.
        /// </summary>
        /// <param name="blackboard">The blackboard.</param>
        /// <returns>Evaluation Status.</returns>
        public bool Evaluate(IStateContext blackboard)
        {
            if (CurrentState == null) return false;

            foreach (var transition in CurrentState.Transitions)
            {
                if (transition.CanTransition(blackboard))
                {
                    // Execute atomic flip
                    transition.Execute(blackboard);

                    // Move to new state
                    CurrentState = _states[transition.TargetStateId];
                    blackboard.Log($"Transitioned to {CurrentState.Id}");
                    return true;
                }
            }

            return false;
        }
    }
}
