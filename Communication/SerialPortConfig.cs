/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        SerialPortConfig.cs
 * PURPOSE:     Serial Port configuration record
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.IO.Ports;

namespace Communication
{
    /// <summary>
    /// Simple record to hold Serial Port configuration settings.
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;Communication.SerialPortConfig&gt;" />
    public sealed record SerialPortConfig
    (
        int BaudRate,
        Parity Parity,
        int DataBits,
        StopBits StopBits,
        Handshake Handshake
    );
}
