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
    /// <inheritdoc />
    internal sealed class SystemSerialPort : ISerialPort
    {
        private readonly SerialPort _port;

        /// <inheritdoc />
        public event EventHandler? DataReceived;

        /// <inheritdoc />
        public bool IsOpen => _port.IsOpen;

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

            _port.DataReceived += (_, __) =>
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
    }
}
