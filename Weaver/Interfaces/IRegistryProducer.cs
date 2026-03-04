/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        IRegistryProducer.cs
 * PURPOSE:     Interface for Registry storage producers. Commands that implement this interface can produce data that is automatically stored in the Weaver registry under a specified key and type, 
 *              allowing other commands to access it without needing explicit variable assignment.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

namespace Weaver.Interfaces
{
    /// <summary>
    /// Interface declaration for commands that produce data to be stored in the Weaver registry. Implementing this interface allows a command's output to be automatically registered under a specified key and type, 
    /// enabling other commands to access it without explicit variable assignment.
    /// </summary>
    public interface IRegistryProducer
    {
        /// <summary>
        /// Gets the current registry key.
        /// </summary>
        /// <value>
        /// The current registry key.
        /// </value>
        string CurrentRegistryKey { get; }

        /// <summary>
        /// The expected type of the data being stored.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        EnumTypes DataType { get; }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        IVariableRegistry Variables { get; }
    }
}