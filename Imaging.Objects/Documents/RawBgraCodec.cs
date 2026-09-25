/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        RasterCodecs.cs
 * PURPOSE:     How the pixels of one raster layer are stored inside a document file.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Raw straight-alpha BGRA with a tiny header; the zip container deflates it. No GDI+ needed, so it works
    ///     everywhere and is what the unit tests use.
    /// </summary>
    public sealed class RawBgraCodec : IRasterCodec
    {
        private static readonly byte[] Magic = { (byte)'S', (byte)'B', (byte)'G', (byte)'R' };

        /// <inheritdoc />
        public string Extension => ".bgra";

        /// <inheritdoc />
        public bool IsCompressed => false;

        /// <inheritdoc />
        public void Write(UnmanagedImageBuffer buffer, Stream output)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(output);

            using var writer = new BinaryWriter(output, System.Text.Encoding.UTF8, leaveOpen: true);
            writer.Write(Magic);
            writer.Write(buffer.Width);
            writer.Write(buffer.Height);
            writer.Flush();

            output.Write(buffer.BufferSpan);
        }

        /// <inheritdoc />
        public UnmanagedImageBuffer Read(Stream input, int expectedWidth, int expectedHeight)
        {
            ArgumentNullException.ThrowIfNull(input);

            var header = new byte[12];
            try
            {
                input.ReadExactly(header);
            }
            catch (EndOfStreamException ex)
            {
                throw new DocumentFormatException("Raster layer data is truncated.", ex);
            }

            if (!header.AsSpan(0, 4).SequenceEqual(Magic))
            {
                throw new DocumentFormatException("Raster layer data has a wrong header.");
            }

            var width = BitConverter.ToInt32(header, 4);
            var height = BitConverter.ToInt32(header, 8);

            if (width != expectedWidth || height != expectedHeight)
            {
                throw new DocumentFormatException(
                    $"Raster layer is {width}x{height} but the document is {expectedWidth}x{expectedHeight}.");
            }

            var buffer = new UnmanagedImageBuffer(width, height);
            try
            {
                input.ReadExactly(buffer.BufferSpan);
                return buffer;
            }
            catch (EndOfStreamException ex)
            {
                buffer.Dispose();
                throw new DocumentFormatException("Raster layer data is truncated.", ex);
            }
            catch
            {
                buffer.Dispose();
                throw;
            }
        }
    }
}
