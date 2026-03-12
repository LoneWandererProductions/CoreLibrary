/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Interfaces
 * FILE:        IState.cs
 * PURPOSE:     State Interface for the State Executive Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Core.StateExecutive.Interfaces
{
    /// <summary>
    /// Represents a distinct node in the executive engine.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets the transitions.
        /// </summary>
        /// <value>
        /// The transitions.
        /// </value>
        IEnumerable<ITransition> Transitions { get; }
    }
}
