/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/Com.cs
 * PURPOSE:     Entry Point for File Downloads
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <inheritdoc />
    /// <summary>
    ///     The com class.
    /// </summary>
    public sealed class Com : ICom
    {
        /// <inheritdoc />
        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Success Status
        /// </returns>
        public Task<bool> SaveFile(string filePath, string url, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            return FileTransfer.SaveFileAsync(filePath, url, progress, cancellationToken);
        }

        /// <inheritdoc />
        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="urls"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        public async Task SaveFile(string filePath, IEnumerable<string> urls, IProgress<int> progress = null, CancellationToken cancellationToken = default)
        {
            await FileTransfer.SaveFilesAsync(filePath, urls, progress, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends an HTTP request using the HttpClientManager.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
        /// <param name="body">The request body (optional).</param>
        /// <param name="contentType">The content type (default: application/json).</param>
        /// <returns>An HttpResponseMessage containing the response details.</returns>
        public async Task<string> SendMessageAsync(string url, string method, string body = null, string contentType = "application/json")
        {
            try
            {
                // Call the static SendMessageAsync from HttpClientManager
                // You can handle specific processing here if needed (e.g., logging, deserialization)
                return await HttpClientManager.SendMessageAsync(url, method, body, contentType);
            }
            catch (Exception ex)
            {
                // Handle or log the error as needed
                throw new Exception("An error occurred while sending the HTTP request.", ex);
            }
        }
    }
}

