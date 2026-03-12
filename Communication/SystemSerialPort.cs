/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        SystemSerialPort.cs
 * PURPOSE:     Simple wrapper around System.IO.Ports.SerialPort, with ISerialPort interface and configurable settings.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.IO.Ports;
using Communication.Interfaces;

namespace Communication
{
    /// <inheritdoc cref="ISerialPort" />
    internal sealed class SystemSerialPort : ISerialPort, IDisposable
    {
        /// <summary>
        /// The port
        /// </summary>
        private readonly SerialPort _port;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <inheritdoc />
        public event EventHandler? DataReceived;

        /// <inheritdoc />
        public bool IsOpen => !_disposed && _port.IsOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemSerialPort"/> class.
        /// </summary>
        /// <param name="portName">Name of the port.</param>
        /// <param name="cfg">The CFG.</param>
        public SystemSerialPort(string portName, SerialPortConfig cfg)
        {
            _port = new SerialPort
            {
                PortName = portName,
                BaudRate = cfg.BaudRate,
                Parity = cfg.Parity,
                DataBits = cfg.DataBits,
                StopBits = cfg.StopBits,
                Handshake = cfg.Handshake,
                ReadTimeout = -1,
                WriteTimeout = -1
            };

            _port.DataReceived += OnDataReceived;
        }

        /// <summary>
        /// Called when [data received].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void Open() => _port.Open();

        /// <inheritdoc />
        public void Close() => _port.Close();

        /// <inheritdoc />
        public int Read(byte[] buffer, int offset, int count)
            => _port.Read(buffer, offset, count);

        /// <inheritdoc />
        public int Read(char[] buffer, int offset, int count)
            => _port.Read(buffer, offset, count);

        /// <inheritdoc />
        public void Write(string data)
            => _port.Write(data);

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
            => _port.Write(buffer, offset, count);

        /// <summary>
        /// Cleanup the hardware port.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Unhook event to avoid memory leaks
            _port.DataReceived -= OnDataReceived;

            // Close and Dispose the actual hardware port
            if (_port.IsOpen)
            {
                _port.Close();
            }

            _port.Dispose();
            _disposed = true;
        }
    }
}
