/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Interfaces;
 * FILE:        IRasterCodec.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects.Interfaces;

/// <summary>
///     Reads and writes the pixels of a single layer.
/// </summary>
public interface IRasterCodec
{
    /// <summary>Gets the file extension including the dot, for example ".png". Used to pick the codec when loading.</summary>
    string Extension { get; }

    /// <summary>Gets a value indicating whether the output is already compressed (the zip container then stores it as is).</summary>
    bool IsCompressed { get; }

    /// <summary>Writes the buffer.</summary>
    void Write(UnmanagedImageBuffer buffer, Stream output);

    /// <summary>Reads a buffer of exactly the expected size, or throws <see cref="DocumentFormatException" />.</summary>
    UnmanagedImageBuffer Read(Stream input, int expectedWidth, int expectedHeight);
}