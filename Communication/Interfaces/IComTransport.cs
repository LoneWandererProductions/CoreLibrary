/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication.Interfaces
 * FILE:        IComTransport.cs
 * PURPOSE:     Com Port Interface, wraps around my ISerialPort.
 *              After that the Protocol layer can be implemented on top.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace Communication.Interfaces
{
    /// <summary>
    /// Minimal implementation of a COM transport interface.
    /// </summary>
    public interface IComTransport
    {
        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        void Write(string text);

        /// <summary>
        /// Occurs when [data received].
        /// </summary>
        event Action<string> DataReceived;
    }
}
