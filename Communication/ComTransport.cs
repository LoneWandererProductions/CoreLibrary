/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        ComTransport.cs
 * PURPOSE:     Wrapper around a serial port implementing IComTransport, with async read support.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Interfaces;

namespace Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Wrapper around a serial port implementing IComTransport
    /// </summary>
    /// <seealso cref="Communication.Interfaces.IComTransport" />
    public sealed class ComTransport : IComTransport
    {
        /// <summary>
        /// The port
        /// </summary>
        private readonly ISerialPort _port;

        /// <summary>
        /// The buffer
        /// </summary>
        private readonly byte[] _buffer = new byte[4096];

        /// <summary>
        /// The encoding
        /// </summary>
        private readonly Encoding _encoding = Encoding.ASCII;

        /// <inheritdoc />
        public event Action<string>? DataReceived;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComTransport"/> class.
        /// </summary>
        /// <param name="port">The port.</param>
        public ComTransport(ISerialPort port)
        {
            _port = port;
            _port.DataReceived += OnPortData;
        }

        /// <summary>
        /// Called when [port data].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnPortData(object? sender, EventArgs e)
        {
            int bytes = _port.Read(_buffer, 0, _buffer.Length);
            if (bytes <= 0)
                return;

            string text = _encoding.GetString(_buffer, 0, bytes);
            DataReceived?.Invoke(text);
        }

        /// <inheritdoc />
        public void Write(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _port.Write(text);
        }

        /// <summary>
        /// Reads the once asynchronous.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>Message from Port.</returns>
        public Task<string> ReadOnceAsync(TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void Handler(string data)
                => tcs.TrySetResult(data);

            DataReceived += Handler;

            var timer = new Timer(_ =>
                    tcs.TrySetException(new TimeoutException()),
                null,
                timeout,
                Timeout.InfiniteTimeSpan);

            return tcs.Task.ContinueWith(t =>
            {
                DataReceived -= Handler;
                timer.Dispose();
                return t.Result;
            });
        }
    }
}
