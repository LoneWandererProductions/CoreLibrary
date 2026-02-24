/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        FileTransfer.cs
 * PURPOSE:     Does the heavy lifting for File Transfers
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    /// Helper class for file transfers, including downloading files from URLs and saving them to disk.
    /// </summary>
    internal static class FileTransfer
    {
        /// <summary>
        /// Singleton HttpClient is correct practice
        /// </summary>
        private static readonly Lazy<HttpClient> HttpClient = new(() => new HttpClient());

        /// <summary>
        /// Saves the file asynchronous.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="url">The URL.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        internal static async Task<bool> SaveFileAsync(string folderPath, string url, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate URL
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

                Directory.CreateDirectory(folderPath);

                // 1. PATH FIX: Robust name generation
                var fileName = GetFileNameFromUri(uri);
                var fullPath = Path.Combine(folderPath, fileName);

                using var response = await HttpClient.Value.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

                // Buffer size: 8192 is standard.
                await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalRead = 0;
                int bytesRead;
                int lastReportedPercent = -1;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    // 2. PROGRESS FIX: Only report if totalBytes is known AND percentage changed
                    if (progress != null && totalBytes > 0)
                    {
                        var currentPercent = (int)((totalRead * 100) / totalBytes);
                        if (currentPercent > lastReportedPercent)
                        {
                            progress.Report(currentPercent);
                            lastReportedPercent = currentPercent;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException)
            {
                // Log and swallow known I/O errors
                Trace.WriteLine($"Error saving file: {ex.Message}");
                return false;
            }
            // Let TaskCanceledException bubble up so the caller knows it was cancelled intentionally
        }

        /// <summary>
        /// Saves the files asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="urls">The url.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        internal static async Task SaveFilesAsync(string filePath, IEnumerable<string> urls, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            // Note: This downloads sequentially.
            // If you want parallel, you would need Task.WhenAll (but be careful of bandwidth).
            foreach (var url in urls)
            {
                // Check cancellation before starting next file
                cancellationToken.ThrowIfCancellationRequested();
                await SaveFileAsync(filePath, url, progress, cancellationToken);
            }
        }

        /// <summary>
        /// Gets a safe filename from the URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        private static string GetFileNameFromUri(Uri uri)
        {
            // Try to get the file name from the LocalPath (handles most cases)
            var fileName = Path.GetFileName(uri.LocalPath);

            // If empty (e.g. "http://site.com/"), generate a default name
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // You could also use a Guid here, or "index.html" depending on context
                return $"download_{DateTime.Now.Ticks}.dat";
            }

            return fileName;
        }
    }
}
