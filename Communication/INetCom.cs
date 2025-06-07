/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/INetCom.cs
 * PURPOSE:     Entry Point for File Downloads, defined as Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    ///     The ICom interface.
    /// </summary>
    public interface INetCom
    {
        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     Success Status
        /// </returns>
        Task<bool> SaveFile(string filePath, string url, IProgress<int> progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="urls">The urls.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>If task is finished</returns>
        Task SaveFile(string filePath, IEnumerable<string> urls, IProgress<int> progress = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Sends an HTTP request to the specified URL with the given method and body.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
        /// <param name="body">The request body (optional).</param>
        /// <param name="contentType">The content type (default: application/json).</param>
        /// <returns>An HttpResponseMessage containing the response details.</returns>
        Task<string> SendMessageAsync(string url, string method, string body = null,
            string contentType = "application/json");

        /// <summary>
        ///     Listeners the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>Reference to the listener</returns>
        Listener Listener(int port);
    }
}
