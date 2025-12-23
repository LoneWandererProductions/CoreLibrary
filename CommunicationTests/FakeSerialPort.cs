/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommunicationTests
 * FILE:        FakeSerialPort.cs
 * PURPOSE:     Serial port mock for testing
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;
using Communication.Interfaces;

namespace CommunicationTests
{
    /// <inheritdoc />
    /// <summary>
    /// Mock serial port for testing purposes
    /// </summary>
    /// <seealso cref="Communication.Interfaces.ISerialPort" />
    internal sealed class FakeSerialPort : ISerialPort
    {
        /// <summary>
        /// The rx
        /// </summary>
        private readonly Queue<byte> _rx = new();

        /// <inheritdoc />
        public bool IsOpen { get; private set; }

        /// <inheritdoc />
        public event EventHandler? DataReceived;

        /// <inheritdoc />
        public void Open() => IsOpen = true;

        /// <inheritdoc />
        public void Close() => IsOpen = false;

        /// <inheritdoc />
        public void Write(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            foreach (var b in bytes)
                _rx.Enqueue(b);

            DataReceived?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public int Read(byte[] buffer, int offset, int count)
        {
            int i = 0;
            while (i < count && _rx.Count > 0)
                buffer[offset + i++] = _rx.Dequeue();
            return i;
        }

        /// <inheritdoc />
        public int Read(char[] buffer, int offset, int count)
        {
            return -1;
        }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            return;
        }
    }
}
