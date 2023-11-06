/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/FileTransfer.cs
 * PURPOSE:     Does the heavy lifting for File Transfers
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    ///     The file transfer class.
    /// </summary>
    internal static class FileTransfer
    {
        /// <summary>
        ///     Gets or sets the progress.
        /// </summary>
        /// <value>
        ///     The progress.
        /// </value>
        public static int Progress { get; private set; }

        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Success Status</returns>
        internal static bool SaveFile(string filePath, string url)
        {
            _ = Directory.CreateDirectory(filePath);

            try
            {
                using var webC = new WebClient {Credentials = CredentialCache.DefaultNetworkCredentials};
                webC.DownloadProgressChanged += DownloadProgressChanged;

                var link = new Uri(url);

                var path = GetPath(link, filePath);

                if (path == null)
                {
                    return false;
                }

                //webC.DownloadFileAsync(new Uri(url), path);
                webC.DownloadFile(link, path);
            }
            catch (ExternalException ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
            catch (ArgumentNullException ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
            catch (WebException ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }


        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        internal static async Task SaveFile(string filePath, IEnumerable<string> url)
        {
            await DownloadFileAsync(filePath, url).ConfigureAwait(false);
        }


        /// <summary>
        ///     Downloads the file asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URLs.</param>
        private static async Task DownloadFileAsync(string filePath, IEnumerable<string> url)
        {
            _ = Directory.CreateDirectory(filePath);

            foreach (var link in url)
            {
                try
                {
                    using var webC = new WebClient {Credentials = CredentialCache.DefaultNetworkCredentials};

                    var urls = new Uri(link);

                    var path = GetPath(urls, filePath);

                    if (path == null)
                    {
                        continue;
                    }

                    webC.DownloadProgressChanged += DownloadProgressChanged;
                    await webC.DownloadFileTaskAsync(urls, path).ConfigureAwait(false);
                }
                catch (ExternalException ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
                catch (ArgumentNullException ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
                catch (UnauthorizedAccessException ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
        }


        /// <summary>
        ///     Gets the path.
        /// </summary>
        /// <param name="link">The link.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>Save Path</returns>
        private static string GetPath(Uri link, string filePath)
        {
            if (string.IsNullOrEmpty(link.AbsoluteUri))
            {
                return string.Empty;
            }

            var target = Path.GetFileName(link.AbsolutePath);

            if (!string.IsNullOrEmpty(target))
            {
                return Path.Combine(filePath, target);
            }

            target = link.AbsoluteUri[(link.AbsoluteUri.LastIndexOf("//", StringComparison.Ordinal) + 2)..];
            target = Regex.Replace(target, "/", "");

            return Path.Combine(filePath, target);
        }

        /// <summary>
        ///     The download progress changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The download progress changed event arguments.</param>
        private static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }
    }
}
