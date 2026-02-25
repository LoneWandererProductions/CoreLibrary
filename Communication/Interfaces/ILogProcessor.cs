/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication.Interfaces
 * FILE:        ILogProcessor.cs
 * PURPOSE:     Interface for processing incoming log messages.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using System.Threading.Tasks;

namespace Communication.Interfaces
{
    /// <summary>
    ///     Interface for processing incoming log messages.
    /// </summary>
    public interface ILogProcessor
    {
        /// <summary>
        ///     Processes the received message (e.g., save to DB, log to file).
        /// </summary>
        /// <param name="message">The message received from the client.</param>
        Task ProcessMessageAsync(string message);
    }
}
