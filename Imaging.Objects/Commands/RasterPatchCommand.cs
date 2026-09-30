/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        RasterPatchCommand.cs
 * PURPOSE:     Undoable edits of a document. A command stores just enough to apply and revert itself:
 *              ids and small records for structure/shape edits, tile patches for pixel edits.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Commands
{
    /// <summary>
    ///     Pixel edit stored as tile patches. Created by <see cref="RasterEditRecorder.Commit" />; the pixels are
    ///     already changed at that point, so hand it to <see cref="DocumentHistory.Record" />, not
    ///     <see cref="DocumentHistory.Execute" />.
    /// </summary>
    public sealed class RasterPatchCommand : IDocumentCommand
    {
        private readonly List<TilePatch> _tiles;

        private readonly Guid _layerId;

        internal RasterPatchCommand(Guid layerId, string description, List<TilePatch> tiles)
        {
            _layerId = layerId;
            Description = description;
            _tiles = tiles;
        }

        /// <inheritdoc />
        public string Description { get; }

        /// <summary>Gets the number of stored tiles.</summary>
        public int TileCount => _tiles.Count;

        /// <summary>Gets the memory held by this step in bytes (before + after pixels).</summary>
        public long ByteSize
        {
            get
            {
                long total = 0;
                foreach (var tile in _tiles)
                {
                    total += tile.Before.Length + tile.After.Length;
                }

                return total;
            }
        }

        /// <inheritdoc />
        public void Apply(Document document)
        {
            Write(document, useAfter: true);
        }

        /// <inheritdoc />
        public void Revert(Document document)
        {
            Write(document, useAfter: false);
        }

        internal static void WriteTile(RasterLayer layer, int tx, int ty, int width, int height, byte[] pixels)
        {
            const int bpp = UnmanagedImageBuffer.BytesPerPixel;
            var target = layer.Pixels.BufferSpan;

            for (var y = 0; y < height; y++)
            {
                var offset = ((ty * RasterEditRecorder.TileSize + y) * layer.Width +
                              tx * RasterEditRecorder.TileSize) * bpp;

                pixels.AsSpan(y * width * bpp, width * bpp).CopyTo(target.Slice(offset, width * bpp));
            }
        }

        private void Write(Document document, bool useAfter)
        {
            var layer = document.GetLayer<RasterLayer>(_layerId);
            var region = PixelRect.Empty;

            foreach (var tile in _tiles)
            {
                WriteTile(layer, tile.Tx, tile.Ty, tile.Width, tile.Height, useAfter ? tile.After : tile.Before);

                region = region.Union(new PixelRect(tile.Tx * RasterEditRecorder.TileSize,
                    tile.Ty * RasterEditRecorder.TileSize, tile.Width, tile.Height));
            }

            document.Raise(DocumentChangeKind.PixelsChanged, _layerId, region);
        }
    }
}
