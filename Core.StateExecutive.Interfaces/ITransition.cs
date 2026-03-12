/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Interfaces
 * FILE:        ITransition.cs
 * PURPOSE:     Transition Interface for the State Executive Engine. Represents a pathway from one state to another, with guards and effects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Core.StateExecutive.Interfaces
{
    /// <summary>
    /// Represents a pathway from one state to another.
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// Gets the target state identifier.
        /// </summary>
        /// <value>
        /// The target state identifier.
        /// </value>
        string TargetStateId { get; }

        /// <summary>
        /// Evaluates if the transition's guards and resource requirements are met.
        /// </summary>
        bool CanTransition(IStateContext context);

        /// <summary>
        /// Executes the atomic transaction (claims resources, runs effects).
        /// </summary>
        void Execute(IStateContext context);
    }
}
