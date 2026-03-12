/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.StateExecutive.Interfaces
 * FILE:        IStateContext.cs
 * PURPOSE:     State Context Interface for the State Executive Engine. Holds all telemetry, variables, and resource locks.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Core.StateExecutive.Interfaces
{
    /// <summary>
    /// The Blackboard. Holds all telemetry, variables, and resource locks.
    /// Passed into every condition and effect.
    /// </summary>
    public interface IStateContext
    {
        /// <summary>
        /// Determines whether the specified key has resource.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="amount">The amount.</param>
        /// <returns>
        ///   <c>true</c> if the specified key has resource; otherwise, <c>false</c>.
        /// </returns>
        bool HasResource(string key, int amount = 1);

        /// <summary>
        /// Tries the claim resource.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        bool TryClaimResource(string key, int amount = 1);

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Log(string message);
    }
}
