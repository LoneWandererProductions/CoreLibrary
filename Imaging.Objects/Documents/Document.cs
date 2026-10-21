/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        Document.cs
 * PURPOSE:     The layered document: an ordered stack of layers plus change notification.
 *              Pure data, no UI types. Index 0 is the bottom layer (same order as LayeredImageContainer).
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     A layered image. The mutating methods are public so commands living in other assemblies can use them,
    ///     but they bypass undo: application code should go through <see cref="DocumentHistory" />.
    /// </summary>
    public sealed class Document : IDisposable
    {
        /// <summary>
        /// The layers
        /// </summary>
        private readonly List<Layer> _layers = new();

        /// <summary>
        /// Creates an empty document.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Document(int width, int height)
        {
            DocumentLimits.EnsureValidSize(width, height);

            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the width in pixels.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; }

        /// <summary>
        /// Gets the height in pixels.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; }

        /// <summary>
        /// Gets the whole document as a rectangle.
        /// </summary>
        /// <value>
        /// The bounds.
        /// </value>
        public PixelRect Bounds => new(0, 0, Width, Height);

        /// <summary>
        /// Gets the layers, bottom to top.
        /// </summary>
        /// <value>
        /// The layers.
        /// </value>
        public IReadOnlyList<Layer> Layers => _layers;

        /// <summary>
        /// Raised after every change, whether it came from a command, undo or redo.
        /// </summary>
        public event EventHandler<DocumentChangedEventArgs>? Changed;

        /// <summary>
        /// Creates a document with one "Background" raster layer that takes ownership of <paramref name="pixels" />.
        /// This is how an ordinary opened image becomes a document.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="backgroundName">Name of the background.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Document FromBuffer(UnmanagedImageBuffer pixels, string backgroundName = "Background")
        {
            ArgumentNullException.ThrowIfNull(pixels);

            var document = new Document(pixels.Width, pixels.Height);
            document._layers.Add(new RasterLayer(pixels, backgroundName));
            return document;
        }

        /// <summary>
        /// Creates a document with the bitmap as its background layer (the bitmap is copied).
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="backgroundName">Name of the background.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static Document FromBitmap(Bitmap bitmap, string backgroundName = "Background")
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            return FromBuffer(UnmanagedImageBuffer.FromBitmap(bitmap), backgroundName);
        }

        /// <summary>
        /// Gets the index of a layer, or -1.
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <returns>Index of Layer.</returns>
        public int IndexOf(Guid layerId)
        {
            for (var i = 0; i < _layers.Count; i++)
            {
                if (_layers[i].Id == layerId) return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds a layer by id.
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <returns>The layer by guid.</returns>
        public Layer? FindLayer(Guid layerId)
        {
            var index = IndexOf(layerId);
            return index < 0 ? null : _layers[index];
        }

        /// <summary>
        /// Gets a layer by id or throws <see cref="KeyNotFoundException" />.
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <returns>The Layer by guid.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public Layer GetLayer(Guid layerId)
        {
            return FindLayer(layerId) ??
                   throw new KeyNotFoundException(string.Format(ImageResource.ErrorLayerNotFound, layerId));
        }

        /// <summary>
        /// Gets a layer of a specific type or throws.
        /// </summary>
        /// <typeparam name="T">Generic Parameter.</typeparam>
        /// <param name="layerId">The layer identifier.</param>
        /// <returns>The layer by Guid.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public T GetLayer<T>(Guid layerId)
            where T : Layer
        {
            return GetLayer(layerId) as T ??
                   throw new InvalidOperationException(string.Format(ImageResource.ErrorInvalidLayerType, layerId,
                       typeof(T).Name));
        }

        /// <summary>
        /// Inserts a layer. The document owns it from now on (and disposes raster layers).
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="layer">The layer.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public void InsertLayer(int index, Layer layer)
        {
            ArgumentNullException.ThrowIfNull(layer);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _layers.Count);

            if (IndexOf(layer.Id) >= 0)
                throw new ArgumentException(string.Format(ImageResource.ErrorLayerExists, layer.Id));

            if (layer is RasterLayer raster && (raster.Width != Width || raster.Height != Height))
            {
                throw new ArgumentException(ImageResource.ErrorLayerSize);
            }

            _layers.Insert(index, layer);
            Raise(DocumentChangeKind.LayerAdded, layer.Id, Bounds);
        }

        /// <summary>
        /// Removes a layer and hands ownership to the caller.
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <param name="index">The index.</param>
        /// <returns>The removed layer.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public Layer RemoveLayer(Guid layerId, out int index)
        {
            index = IndexOf(layerId);
            if (index < 0) throw new KeyNotFoundException(string.Format(ImageResource.ErrorLayerNotFound, layerId));

            var layer = _layers[index];
            _layers.RemoveAt(index);
            Raise(DocumentChangeKind.LayerRemoved, layerId, Bounds);
            return layer;
        }

        /// <summary>
        /// Moves a layer to a new index (as seen after the move).
        /// </summary>
        /// <param name="layerId">The layer identifier.</param>
        /// <param name="newIndex">The new index.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public void MoveLayer(Guid layerId, int newIndex)
        {
            var index = IndexOf(layerId);
            if (index < 0) throw new KeyNotFoundException(string.Format(ImageResource.ErrorLayerNotFound, layerId));

            ArgumentOutOfRangeException.ThrowIfNegative(newIndex);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(newIndex, _layers.Count);

            var layer = _layers[index];
            _layers.RemoveAt(index);
            _layers.Insert(newIndex, layer);
            Raise(DocumentChangeKind.LayerMoved, layerId, Bounds);
        }

        /// <summary>
        /// Raises <see cref="Changed" />. Commands call this after touching layer content.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="layerId">The layer identifier.</param>
        /// <param name="region">The region.</param>
        public void Raise(DocumentChangeKind kind, Guid layerId, PixelRect region)
        {
            Changed?.Invoke(this, new DocumentChangedEventArgs(kind, layerId, region));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var layer in _layers)
            {
                (layer as IDisposable)?.Dispose();
            }

            _layers.Clear();
        }
    }
}
