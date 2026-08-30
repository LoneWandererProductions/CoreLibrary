/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugins
 * FILE:        PpmDecoderPlugin.cs
 * PURPOSE:     Example IImageDecoderPlugin - decodes binary PPM ("P6") images.
 *              Exists to prove the plugin pipeline end-to-end with a format
 *              that needs zero external dependencies to decode, not because
 *              PPM is a format SlimViewer users need day to day. Use this as
 *              the template for a real one (WebP, HEIC, RAW, ...).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using Imaging.Interfaces;

namespace Imaging.Plugins
{
    /// <summary>
    ///     Decodes binary PPM (Netpbm "P6") images.
    /// </summary>
    /// <remarks>
    ///     To try it: build this project, copy Plugins.Ppm.dll into a "Plugins"
    ///     folder next to SlimViewer.exe (Imaging.dll does not need to go with
    ///     it - the host already has it loaded), and open a .ppm file. No change
    ///     to SlimViewer itself is needed for that to work.
    /// </remarks>
    public sealed class PpmDecoderPlugin : IImageDecoderPlugin
    {
        /// <inheritdoc />
        public string Name => "PPM (Netpbm P6) decoder";

        /// <inheritdoc />
        public IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".ppm" };

        /// <inheritdoc />
        public Bitmap Decode(string path)
        {
            using var stream = File.OpenRead(path);

            if (ReadToken(stream) != "P6")
            {
                throw new InvalidDataException($"'{path}' is not a binary (P6) PPM file.");
            }

            var width = int.Parse(ReadToken(stream));
            var height = int.Parse(ReadToken(stream));
            var maxValue = int.Parse(ReadToken(stream));

            if (maxValue is <= 0 or > 255)
            {
                throw new NotSupportedException(
                    $"'{path}' uses max value {maxValue}; only 8-bit-per-channel PPM (1-255) is supported.");
            }

            var rgb = new byte[width * height * 3];
            var read = 0;
            while (read < rgb.Length)
            {
                var n = stream.Read(rgb, read, rgb.Length - read);
                if (n == 0)
                {
                    throw new EndOfStreamException(
                        $"'{path}' is truncated: expected {rgb.Length} bytes of pixel data, got {read}.");
                }

                read += n;
            }

            return ToBitmap(rgb, width, height);
        }

        /// <summary>
        /// Builds a GDI+ Bitmap from raw RGB bytes. No unsafe/pointer code -
        /// Marshal.Copy does the managed-to-native transfer, which keeps this
        /// usable as a template without needing AllowUnsafeBlocks.
        /// </summary>
        /// <param name="rgb">The RGB.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Normal Bitmap</returns>
        private static Bitmap ToBitmap(byte[] rgb, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                var rowBytes = width * 3;

                if (bits.Stride == rowBytes)
                {
                    // No row padding - swap R/B (PPM is RGB, 24bppRgb is stored BGR)
                    // in one pass and copy the whole buffer at once.
                    var bgr = new byte[rgb.Length];
                    for (var i = 0; i < rgb.Length; i += 3)
                    {
                        bgr[i] = rgb[i + 2];
                        bgr[i + 1] = rgb[i + 1];
                        bgr[i + 2] = rgb[i];
                    }

                    Marshal.Copy(bgr, 0, bits.Scan0, bgr.Length);
                }
                else
                {
                    // Stride is padded to a 4-byte boundary - copy row by row.
                    var padded = new byte[bits.Stride * height];
                    for (var y = 0; y < height; y++)
                    {
                        var srcRow = y * rowBytes;
                        var destRow = y * bits.Stride;
                        for (var x = 0; x < width; x++)
                        {
                            padded[destRow + x * 3] = rgb[srcRow + x * 3 + 2];
                            padded[destRow + x * 3 + 1] = rgb[srcRow + x * 3 + 1];
                            padded[destRow + x * 3 + 2] = rgb[srcRow + x * 3];
                        }
                    }

                    Marshal.Copy(padded, 0, bits.Scan0, padded.Length);
                }
            }
            finally
            {
                bitmap.UnlockBits(bits);
            }

            return bitmap;
        }

        /// <summary>
        /// Reads one whitespace-delimited token from the PPM header, skipping
        /// '#' comments (which run to end of line), per the Netpbm spec.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.InvalidDataException">Unexpected end of file while reading PPM header.</exception>
        private static string ReadToken(Stream stream)
        {
            var token = new StringBuilder();
            var inComment = false;
            int b;

            while ((b = stream.ReadByte()) != -1)
            {
                var c = (char)b;
                if (inComment)
                {
                    if (c == '\n') inComment = false;
                    continue;
                }

                if (c == '#')
                {
                    inComment = true;
                    continue;
                }

                if (char.IsWhiteSpace(c)) continue;

                token.Append(c);
                break;
            }

            while ((b = stream.ReadByte()) != -1)
            {
                var c = (char)b;
                if (char.IsWhiteSpace(c)) break;

                token.Append(c);
            }

            if (token.Length == 0)
            {
                throw new InvalidDataException("Unexpected end of file while reading PPM header.");
            }

            return token.ToString();
        }
    }
}