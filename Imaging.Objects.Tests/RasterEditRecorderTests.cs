/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        RasterEditRecorderTests.cs
 * PURPOSE:     Tile based copy-on-write undo for pixel edits.
 */

using Imaging.Objects.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    [TestClass]
    public sealed class RasterEditRecorderTests
    {
        // 200x130 is deliberately not a multiple of 64 so the edge tiles are partial.
        private const int Width = 200;

        private const int Height = 130;

        private static (Document Document, RasterLayer Layer, DocumentHistory History) Setup()
        {
            var document = new Document(Width, Height);
            var layer = new RasterLayer(Width, Height, "paint");
            document.InsertLayer(0, layer);
            return (document, layer, new DocumentHistory(document));
        }

        [TestMethod]
        public void Stroke_CanBeUndoneAndRedoneExactly()
        {
            var (document, layer, history) = Setup();
            using (document)
            {
                var original = TestSupport.Snapshot(layer);
                var recorder = new RasterEditRecorder(layer);

                var area = new PixelRect(10, 10, 20, 20);
                recorder.Touch(area);
                TestSupport.Fill(layer, area, 255, 0, 0);
                var painted = TestSupport.Snapshot(layer);

                var command = recorder.Commit("Brush");
                Assert.IsNotNull(command);
                history.Record(command!);

                history.Undo();
                Assert.IsTrue(TestSupport.SamePixels(layer, original));

                history.Redo();
                Assert.IsTrue(TestSupport.SamePixels(layer, painted));
            }
        }

        [TestMethod]
        public void OnlyTouchedTilesAreStored()
        {
            var (document, layer, _) = Setup();
            using (document)
            {
                var recorder = new RasterEditRecorder(layer);

                recorder.Touch(new PixelRect(10, 10, 20, 20));
                Assert.AreEqual(1, recorder.TouchedTileCount);

                // x = 60..69 crosses the tile border at 64: tile (0,0) is already captured, (1,0) is new.
                recorder.Touch(new PixelRect(60, 10, 10, 5));
                Assert.AreEqual(2, recorder.TouchedTileCount);

                // Crosses both borders (x and y): (0,1) and (1,1) are new.
                recorder.Touch(new PixelRect(60, 60, 10, 10));
                Assert.AreEqual(4, recorder.TouchedTileCount);
            }
        }

        [TestMethod]
        public void UnchangedTiles_AreDroppedFromThePatch()
        {
            var (document, layer, _) = Setup();
            using (document)
            {
                var recorder = new RasterEditRecorder(layer);
                recorder.Touch(new PixelRect(60, 10, 10, 5)); // two tiles
                TestSupport.Fill(layer, new PixelRect(60, 10, 2, 2), 1, 2, 3); // only left tile changes

                var command = recorder.Commit();

                Assert.IsNotNull(command);
                Assert.AreEqual(1, command!.TileCount);
                Assert.IsTrue(command.ByteSize > 0);
            }
        }

        [TestMethod]
        public void NothingChanged_CommitReturnsNull()
        {
            var (document, layer, _) = Setup();
            using (document)
            {
                var recorder = new RasterEditRecorder(layer);
                recorder.Touch(new PixelRect(0, 0, 100, 100));

                Assert.IsNull(recorder.Commit());
            }
        }

        [TestMethod]
        public void TouchingTwice_KeepsTheOriginalPixels()
        {
            var (document, layer, history) = Setup();
            using (document)
            {
                var original = TestSupport.Snapshot(layer);
                var recorder = new RasterEditRecorder(layer);
                var area = new PixelRect(5, 5, 10, 10);

                recorder.Touch(area);
                TestSupport.Fill(layer, area, 255, 0, 0);
                recorder.Touch(area); // second batch of the same stroke
                TestSupport.Fill(layer, area, 0, 255, 0);

                history.Record(recorder.Commit()!);
                history.Undo();

                Assert.IsTrue(TestSupport.SamePixels(layer, original),
                    "Undo must restore the state before the FIRST touch.");
            }
        }

        [TestMethod]
        public void PartialEdgeTile_RoundTrips()
        {
            var (document, layer, history) = Setup();
            using (document)
            {
                var original = TestSupport.Snapshot(layer);
                var recorder = new RasterEditRecorder(layer);
                var area = new PixelRect(Width - 3, Height - 2, 3, 2); // bottom-right tile is only 8x2

                recorder.Touch(area);
                TestSupport.Fill(layer, area, 9, 9, 9);
                var painted = TestSupport.Snapshot(layer);
                history.Record(recorder.Commit()!);

                history.Undo();
                Assert.IsTrue(TestSupport.SamePixels(layer, original));
                history.Redo();
                Assert.IsTrue(TestSupport.SamePixels(layer, painted));
            }
        }

        [TestMethod]
        public void Touch_IsClippedToTheLayer()
        {
            var (document, layer, _) = Setup();
            using (document)
            {
                var recorder = new RasterEditRecorder(layer);

                recorder.Touch(new PixelRect(-50, -50, 40, 40));
                Assert.AreEqual(0, recorder.TouchedTileCount);

                recorder.Touch(new PixelRect(-10, -10, 20, 20));
                Assert.AreEqual(1, recorder.TouchedTileCount);
            }
        }

        [TestMethod]
        public void Rollback_RestoresTouchedPixels()
        {
            var (document, layer, _) = Setup();
            using (document)
            {
                TestSupport.Fill(layer, new PixelRect(0, 0, Width, Height), 7, 7, 7);
                var original = TestSupport.Snapshot(layer);
                var recorder = new RasterEditRecorder(layer);
                var area = new PixelRect(20, 20, 30, 30);

                recorder.Touch(area);
                TestSupport.Fill(layer, area, 255, 255, 255);
                recorder.Rollback();

                Assert.IsTrue(TestSupport.SamePixels(layer, original));
                Assert.AreEqual(0, recorder.TouchedTileCount);
            }
        }

        [TestMethod]
        public void TouchAll_CoversWholeLayer_AndReportsDirtyRegion()
        {
            var (document, layer, history) = Setup();
            using (document)
            {
                var regions = new List<PixelRect>();
                document.Changed += (_, e) =>
                {
                    if (e.Kind == DocumentChangeKind.PixelsChanged) regions.Add(e.Region);
                };

                var recorder = new RasterEditRecorder(layer);
                recorder.TouchAll();
                TestSupport.Fill(layer, new PixelRect(0, 0, Width, Height), 1, 1, 1);
                history.Record(recorder.Commit("Filter")!);

                history.Undo();

                Assert.AreEqual(1, regions.Count);
                Assert.AreEqual(new PixelRect(0, 0, Width, Height), regions[0]);
                TestSupport.AssertColor(TestSupport.Pixel(layer, 100, 100), (0, 0, 0, 0), 0);
            }
        }
    }
}
