/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/DialogHelper.cs
 * PURPOSE:     Some Shared Helpers tha are needed but a bit redundant
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace CommonDialogs
{
    internal static class DialogHelper
    {
        /// <summary>
        ///     Loads File in a Stream
        ///     takes longer but can be changed on Runtime
        /// </summary>
        /// <param name="path">path to the File</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>
        ///     An Image as <see cref="BitmapImage" />.
        /// </returns>
        /// <exception cref="IOException">
        /// </exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="IOException">Error while we try to access the File</exception>
        /// <exception cref="ArgumentException">No Correct Argument were provided</exception>
        /// <exception cref="NotSupportedException">File Type provided was not supported</exception>
        /// <exception cref="InvalidOperationException">Could not get correct access to the Object</exception>
        internal static BitmapImage? GetBitmapImageFileStream(string path, int width = 0, int height = 0)
        {
            var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.DelayCreation };

            try
            {
                using var flStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;

                if (width > 0 && height > 0)
                {
                    bmp.DecodePixelWidth = width;
                    bmp.DecodePixelHeight = height;
                }

                bmp.StreamSource = flStream;
                bmp.EndInit();

                return bmp;
            }
            catch (FileFormatException ex)
            {
                Trace.Write(ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
