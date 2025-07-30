/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/FileTransfer.cs
 * PURPOSE:     Does the heavy lifting for File Transfers
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */
// ReSharper disable MemberCanBePrivate.Global
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Communication;
/// <summary>
///     Handles file transfers for the Communication project.
/// </summary>
internal static class FileTransfer
{
    /// <summary>
    ///     The HTTP client
    /// </summary>
    private static readonly Lazy<HttpClient> HttpClient = new(() => new HttpClient());
    /// <summary>
    ///     Saves a file from a URL to the specified file path.
    /// </summary>
    /// <param name = "filePath">The destination folder path.</param>
    /// <param name = "url">The file URL.</param>
    /// <param name = "progress">Optional progress reporter.</param>
    /// <param name = "cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that resolves to true if the file was saved successfully; otherwise, false.</returns>
    internal static async Task<bool> SaveFileAsync(string filePath, string url, IProgress<int> progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Directory.CreateDirectory(filePath);
            var uri = new Uri(url);
            var path = GetPath(uri, filePath);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            using var response = await HttpClient.Value.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;
                if (progress != null && totalBytes > 0)
                {
                    var percentage = (int)(totalRead * 100 / totalBytes);
                    progress.Report(percentage);
                }
            }

            return true;
        }
        catch (Exception ex)when (ex is HttpRequestException or IOException)
        {
            Trace.WriteLine(string.Format(Resource.Resource4, ex.Message));
            return false;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(string.Format(Resource.Resource5, ex));
            throw; // Re-throw unexpected exceptions to let the caller handle them.
        }
    }

    /// <summary>
    ///     Saves multiple files from URLs to the specified file path.
    /// </summary>
    /// <param name = "filePath">The destination folder path.</param>
    /// <param name = "urls">The file URLs.</param>
    /// <param name = "progress">Optional progress reporter.</param>
    /// <param name = "cancellationToken">Optional cancellation token.</param>
    internal static async Task SaveFilesAsync(string filePath, IEnumerable<string> urls, IProgress<int> progress = null, CancellationToken cancellationToken = default)
    {
        foreach (var url in urls)
        {
            await SaveFileAsync(filePath, url, progress, cancellationToken);
        }
    }

    /// <summary>
    ///     Generates a valid file path based on the URL and destination folder.
    /// </summary>
    /// <param name = "link">The URL of the file.</param>
    /// <param name = "filePath">The destination folder path.</param>
    /// <returns>The full file path for saving the file.</returns>
    private static string GetPath(Uri link, string filePath)
    {
        if (string.IsNullOrEmpty(link.AbsoluteUri))
        {
            return string.Empty;
        }

        var target = Path.GetFileName(link.AbsolutePath);
        if (!string.IsNullOrWhiteSpace(target))
        {
            return Path.Combine(filePath, target);
        }

        target = Regex.Replace(link.AbsoluteUri[(link.AbsoluteUri.LastIndexOf(Resource.Resource1, StringComparison.Ordinal) + 2)..], Resource.Resource2, Resource.Resource3);
        return Path.Combine(filePath, target);
    }
}