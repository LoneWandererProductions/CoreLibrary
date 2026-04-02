/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Tests
 * FILE:        DroneBlackboard.cs
 * PURPOSE:     A sample implementation of IStateContext for testing the State Executive Engine with a drone scenario.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.StateExecutive.Interfaces;

namespace Core.Tests
{
    /// <inheritdoc />
    /// <summary>
    /// A simulated drone. The blackboard holds the state of the drone, such as whether the engine is on and the battery level.
    /// </summary>
    /// <seealso cref="Core.StateExecutive.Interfaces.IStateContext" />
    public sealed class DroneBlackboard : IStateContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is engine on.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is engine on; otherwise, <c>false</c>.
        /// </value>
        public bool IsEngineOn { get; set; } = false;

        /// <summary>
        /// Gets or sets the battery level.
        /// </summary>
        /// <value>
        /// The battery level.
        /// </value>
        public int BatteryLevel { get; set; } = 100;

        /// <summary>
        /// Gets the event log.
        /// </summary>
        /// <value>
        /// The event log.
        /// </value>
        public List<string> EventLog { get; } = new();

        /// <inheritdoc />
        public bool HasResource(string key, int amount = 1)
        {
            if (key == "Battery") return BatteryLevel >= amount;

            return false;
        }

        /// <inheritdoc />
        public bool TryClaimResource(string key, int amount = 1)
        {
            if (HasResource(key, amount))
            {
                if (key == "Battery") BatteryLevel -= amount;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Log(string message)
        {
            EventLog.Add(message);
        }
    }
}
