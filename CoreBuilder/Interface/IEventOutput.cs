/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Interface
 * FILE:        IEventOutput.cs
 * PURPOSE:     Simple interface for side channel event output.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global

namespace CoreBuilder.Interface
{
    /// <summary>
    /// Define an Interface for Event Output, can be implemented by Console, File, etc.
    /// </summary>
    public interface IEventOutput
    {
        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Write(string message);
    }
}
