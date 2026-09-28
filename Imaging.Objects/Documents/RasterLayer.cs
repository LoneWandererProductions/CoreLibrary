/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        RasterLayer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

/// <summary>
///     A layer of pixels, stored in an <see cref="UnmanagedImageBuffer" /> (straight-alpha BGRA, same layout
///     as GDI+ Format32bppArgb). Always the size of the document.
/// </summary>
public sealed class RasterLayer : Layer, IDisposable
{
    private UnmanagedImageBuffer? _pixels;

    /// <summary>Creates a fully transparent layer.</summary>
    public RasterLayer(int width, int height, string name = "Layer")
        : this(Guid.NewGuid(), name, CreateTransparent(width, height))
    {
    }

    /// <summary>Creates a layer that takes ownership of <paramref name="pixels" />.</summary>
    public RasterLayer(UnmanagedImageBuffer pixels, string name = "Layer")
        : this(Guid.NewGuid(), name, pixels)
    {
    }

    /// <summary>Creates a layer with a known id (used by the loader). Takes ownership of the pixels.</summary>
    public RasterLayer(Guid id, string? name, UnmanagedImageBuffer pixels)
        : base(id, name)
    {
        ArgumentNullException.ThrowIfNull(pixels);

        _pixels = pixels;
        Width = pixels.Width;
        Height = pixels.Height;
    }

    /// <summary>Gets the width in pixels.</summary>
    public int Width { get; }

    /// <summary>Gets the height in pixels.</summary>
    public int Height { get; }

    /// <summary>Gets a value indicating whether the pixel memory has been released.</summary>
    public bool IsDisposed => _pixels is null;

    /// <summary>Gets the pixel buffer. Writes bypass history; use <see cref="RasterEditRecorder" /> for that.</summary>
    public UnmanagedImageBuffer Pixels => _pixels ?? throw new ObjectDisposedException(nameof(RasterLayer));

    /// <inheritdoc />
    public override Layer Duplicate(string? name = null)
    {
        var copy = new RasterLayer(Guid.NewGuid(), name ?? $"{Name} copy", (UnmanagedImageBuffer)Pixels.Clone());
        CopySettingsTo(copy);
        return copy;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _pixels?.Dispose();
        _pixels = null;
    }

    private static UnmanagedImageBuffer CreateTransparent(int width, int height)
    {
        DocumentLimits.EnsureValidSize(width, height);

        // AllocHGlobal does not zero memory, so a new layer must be cleared explicitly.
        var buffer = new UnmanagedImageBuffer(width, height);
        buffer.Clear(0, 0, 0, 0);
        return buffer;
    }
}