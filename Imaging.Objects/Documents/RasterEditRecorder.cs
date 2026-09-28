/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        RasterEditRecorder.cs
 * PURPOSE:     Copy-on-write undo for pixel edits: only the 64x64 tiles a stroke touches are stored, once.
 *              Replaces "clone the whole bitmap for every undo step".
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Commands;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Records a pixel edit. Usage: call <see cref="Touch" /> with the area you are about to change
    ///     <b>before</b> writing to it (as often as you like), do the drawing, then <see cref="Commit" /> and hand the
    ///     result to <see cref="DocumentHistory.Record" />. Pixels outside touched areas are not part of the undo step.
    /// </summary>
    public sealed class RasterEditRecorder
    {
        /// <summary>Edge length of a tile in pixels.</summary>
        public const int TileSize = 64;

        private const int Bpp = UnmanagedImageBuffer.BytesPerPixel;

        private readonly Dictionary<(int Tx, int Ty), byte[]> _before = new();

        private readonly RasterLayer _layer;

        /// <summary>Initializes a new instance of the <see cref="RasterEditRecorder" /> class.</summary>
        public RasterEditRecorder(RasterLayer layer)
        {
            ArgumentNullException.ThrowIfNull(layer);
            _layer = layer;
        }

        /// <summary>Gets the number of tiles captured so far.</summary>
        public int TouchedTileCount => _before.Count;

        /// <summary>Captures the original pixels of every tile overlapping <paramref name="area" />.</summary>
        public void Touch(PixelRect area)
        {
            area = area.Intersect(new PixelRect(0, 0, _layer.Width, _layer.Height));
            if (area.IsEmpty) return;

            var tx0 = area.X / TileSize;
            var tx1 = (area.Right - 1) / TileSize;
            var ty0 = area.Y / TileSize;
            var ty1 = (area.Bottom - 1) / TileSize;

            for (var ty = ty0; ty <= ty1; ty++)
            {
                for (var tx = tx0; tx <= tx1; tx++)
                {
                    if (!_before.ContainsKey((tx, ty)))
                    {
                        _before[(tx, ty)] = CopyTile(tx, ty);
                    }
                }
            }
        }

        /// <summary>Captures the whole layer (for tools that rewrite everything, like a filter).</summary>
        public void TouchAll()
        {
            Touch(new PixelRect(0, 0, _layer.Width, _layer.Height));
        }

        /// <summary>
        ///     Builds the undo step from everything touched since the last commit. Tiles whose pixels ended up
        ///     unchanged are dropped. Returns null if nothing actually changed.
        /// </summary>
        public RasterPatchCommand? Commit(string description = "Edit pixels")
        {
            var tiles = new List<TilePatch>();

            foreach (var ((tx, ty), before) in _before)
            {
                var after = CopyTile(tx, ty);
                if (before.AsSpan().SequenceEqual(after)) continue;

                tiles.Add(new TilePatch(tx, ty, TileWidth(tx), TileHeight(ty), before, after));
            }

            _before.Clear();
            return tiles.Count == 0 ? null : new RasterPatchCommand(_layer.Id, description, tiles);
        }

        /// <summary>Puts back everything touched so far (for an aborted stroke) and forgets it.</summary>
        public void Rollback()
        {
            foreach (var ((tx, ty), before) in _before)
            {
                RasterPatchCommand.WriteTile(_layer, tx, ty, TileWidth(tx), TileHeight(ty), before);
            }

            _before.Clear();
        }

        private int TileWidth(int tx)
        {
            return Math.Min(TileSize, _layer.Width - (tx * TileSize));
        }

        private int TileHeight(int ty)
        {
            return Math.Min(TileSize, _layer.Height - (ty * TileSize));
        }

        private byte[] CopyTile(int tx, int ty)
        {
            var width = TileWidth(tx);
            var height = TileHeight(ty);
            var result = new byte[width * height * Bpp];
            var pixels = _layer.Pixels.BufferSpan;

            for (var y = 0; y < height; y++)
            {
                var source = (((((ty * TileSize) + y) * _layer.Width) + (tx * TileSize)) * Bpp);
                pixels.Slice(source, width * Bpp).CopyTo(result.AsSpan(y * width * Bpp, width * Bpp));
            }

            return result;
        }
    }
}