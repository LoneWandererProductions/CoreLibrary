/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Interfaces;
 * FILE:        IRasterCodec.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects.Interfaces
{
    /// <summary>
    ///     Reads and writes the pixels of a single layer.
    /// </summary>
    public interface IRasterCodec
    {
        /// <summary>
        /// Gets the file extension including the dot, for example ".png". Used to pick the codec when loading.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        string Extension { get; }

        /// <summary>
        /// Gets a value indicating whether the output is already compressed (the zip container then stores it as is).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is compressed; otherwise, <c>false</c>.
        /// </value>
        bool IsCompressed { get; }

        /// <summary>
        /// Writes the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="output">The output.</param>
        void Write(UnmanagedImageBuffer buffer, Stream output);

        /// <summary>
        /// Reads a buffer of exactly the expected size, or throws <see cref="DocumentFormatException" />.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="expectedWidth">The expected width.</param>
        /// <param name="expectedHeight">The expected height.</param>
        /// <returns>Our Image data as <see cref="UnmanagedImageBuffer"/</returns>
        UnmanagedImageBuffer Read(Stream input, int expectedWidth, int expectedHeight);
    }
}
