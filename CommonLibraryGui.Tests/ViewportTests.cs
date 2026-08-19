/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGui.Tests
 * FILE:        ViewportTests.cs
 * PURPOSE:     NUnit test cases for Viewport camera transforms, projections, frustum culling, and dirty tracking.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using NUnit.Framework;
using Solaris;

namespace CommonLibraryGui.Tests
{
    /// <summary>
    /// Unit tests verifying viewport coordinate projections, frustum culling calculations, and dirty tracking state.
    /// </summary>
    [TestFixture]
    public class ViewportTests
    {
        /// <summary>
        /// Verifies that WorldToScreen correctly applies pan offset and zoom level to world tile indices.
        /// </summary>
        [Test]
        public void WorldToScreen_Orthographic_ReturnsExpectedPixelPoint()
        {
            var viewport = new Viewport
            {
                PanX = 50,
                PanY = 100,
                Zoom = 2.0f,
                Projection = ProjectionMode.Orthographic2D
            };

            // Tile index 11 -> TileX = 1, TileY = 1 in a 10-wide map, textureSize = 32
            // WorldX = 32, WorldY = 32
            // Expected ScreenX = (32 * 2.0) + 50 = 114
            // Expected ScreenY = (32 * 2.0) + 100 = 164
            var screenPoint = viewport.WorldToScreen(tileIndex: 11, mapWidth: 10, textureSize: 32);

            Assert.That(screenPoint.X, Is.EqualTo(114));
            Assert.That(screenPoint.Y, Is.EqualTo(164));
        }

        /// <summary>
        /// Verifies that ScreenToWorld accurately converts screen click coordinates back to map tile IDs under pan and zoom.
        /// </summary>
        [Test]
        public void ScreenToWorld_WithPanAndZoom_ResolvesCorrectTileIndex()
        {
            var viewport = new Viewport
            {
                PanX = 100,
                PanY = 100,
                Zoom = 2.0f,
                Projection = ProjectionMode.Orthographic2D
            };

            // Click at screen pixel (300, 300)
            // Unzoomed Point: X = (300 - 100) / 2 = 100, Y = (300 - 100) / 2 = 100
            // Tile Coordinates (textureSize = 50): TileX = 2, TileY = 2
            // Tile Index = 2 * 10 + 2 = 22
            var clickPoint = new PointF(300f, 300f);
            var tileIndex = viewport.ScreenToWorld(clickPoint, mapWidth: 10, mapHeight: 10, textureSize: 50);

            Assert.That(tileIndex, Is.EqualTo(22));
        }

        /// <summary>
        /// Verifies that clicks outside the valid map boundaries return -1.
        /// </summary>
        [Test]
        public void ScreenToWorld_OutOfBoundsPoint_ReturnsNegativeOne()
        {
            var viewport = new Viewport
            {
                PanX = 0,
                PanY = 0,
                Zoom = 1.0f
            };

            var outOfBoundsPoint = new PointF(-50f, -50f);
            var tileIndex = viewport.ScreenToWorld(outOfBoundsPoint, mapWidth: 10, mapHeight: 10, textureSize: 100);

            Assert.That(tileIndex, Is.EqualTo(-1));
        }

        /// <summary>
        /// Verifies that camera panning correctly shifts the calculated visible frustum tile bounds for spatial culling.
        /// </summary>
        [Test]
        public void GetVisibleTileBounds_PannedCamera_CalculatesCorrectCullingRectangle()
        {
            var viewport = new Viewport
            {
                PanX = -200, // Camera shifted right by 2 tiles
                PanY = -300, // Camera shifted down by 3 tiles
                Zoom = 1.0f,
                ScreenWidth = 800,
                ScreenHeight = 600
            };

            // TextureSize = 100 -> MinTileX = floor(200 / 100) = 2, MinTileY = floor(300 / 100) = 3
            var bounds = viewport.GetVisibleTileBounds(mapWidth: 50, mapHeight: 50, textureSize: 100);

            Assert.That(bounds.Left, Is.EqualTo(2));
            Assert.That(bounds.Top, Is.EqualTo(3));
            Assert.That(bounds.Width, Is.GreaterThan(0));
            Assert.That(bounds.Height, Is.GreaterThan(0));
        }

        /// <summary>
        /// Verifies that DirtyTracker aggregates Viewport flags alongside TileMap flags and clears properly.
        /// </summary>
        [Test]
        public void DirtyTracker_MarkingViewportDirty_AggregatesFlagsAndClears()
        {
            var tracker = new DirtyTracker();

            tracker.MarkLayerDirty(DirtyFlags.Viewport);
            tracker.MarkTileDirty(5, DirtyFlags.TileMap);

            Assert.That(tracker.Flags.HasFlag(DirtyFlags.Viewport), Is.True);
            Assert.That(tracker.Flags.HasFlag(DirtyFlags.TileMap), Is.True);
            Assert.That(tracker.DirtyTileIndices, Contains.Item(5));

            tracker.Clear();

            Assert.That(tracker.Flags, Is.EqualTo(DirtyFlags.None));
            Assert.That(tracker.DirtyTileIndices, Is.Empty);
        }
    }
}
