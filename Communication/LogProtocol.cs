/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        LogProtocol.cs
 * PURPOSE:     List of Supported Log Protocols.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Communication
{
    /// <summary>
    /// Enum of supported Protocols
    /// </summary>
    public enum LogProtocol
    {
        /// <summary>
        /// The TCP Protocol
        /// </summary>
        Tcp = 0,

        /// <summary>
        /// The UDP Protocol
        /// </summary>
        Udp = 1,

        /// <summary>
        /// Both Protocols
        /// </summary>
        Both = 2
    }
}
