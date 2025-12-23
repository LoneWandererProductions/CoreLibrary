/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication.Interfaces
 * FILE:        ISerialPort.cs
 * PURPOSE:     Basic serial port interface
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace Communication.Interfaces
{
    /// <summary>
    /// The ISerialPort interface.
    /// </summary>
    public interface ISerialPort
    {
        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>Actual count of bytes, the actual value is in the buffer</returns>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Reads the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>Actual count of bytes, the actual value is in the buffer</returns>
        int Read(char[] buffer, int offset, int count);

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        void Write(string data);


        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Occurs when [data received].
        /// </summary>
        event EventHandler DataReceived;
    }
}
