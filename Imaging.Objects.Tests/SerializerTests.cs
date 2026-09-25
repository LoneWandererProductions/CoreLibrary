/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Tests
 * FILE:        SerializerTests.cs
 * PURPOSE:     Saving and loading .slimdoc containers, including damaged and too-new files.
 *              Tests marked "Gdi" need GDI+ (Windows) because they use the PNG codec.
 */

using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Imaging.Objects.Commands;
using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;
using Imaging.Objects.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Imaging.Objects.Tests
{
    [TestClass]
    public sealed class SerializerTests
    {
        /// <summary>
        /// The width
        /// </summary>
        private const int Width = 37;

        /// <summary>
        /// The height
        /// </summary>
        private const int Height = 23;

        /// <summary>
        /// Builds the sample document.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="inkId">The ink identifier.</param>
        /// <returns></returns>
        private static Document BuildSampleDocument(out Guid photoId, out Guid inkId)
        {
            var document = new Document(Width, Height);

            var photo = new RasterLayer(Width, Height, "Photo");
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    photo.Pixels.SetPixel(x, y, (byte)(x * 5), (byte)(y * 9), (byte)(x + y), (byte)(255 - x));
                }
            }

            var overlay = new RasterLayer(Width, Height, "Overlay") { Visible = false, Opacity = 0.4 };
            overlay.Pixels.SetPixel(3, 4, 1, 2, 3, 4);

            var ink = new ShapeLayer("Ink") { Opacity = 0.75 };
            ink.Insert(0,
                new RectShape(1, 2, 10, 8)
                {
                    Fill = new SolidFill(0xFFFF0000), Stroke = new StrokeSpec(0xFF00FF00, 2)
                });
            ink.Insert(1, new EllipseShape(5, 5, 6, 6) { Fill = new TextureFill("Cloud") });
            ink.Insert(2,
                new PolygonShape(ImmutableArray.Create(new PointD(1, 1), new PointD(9, 1), new PointD(5, 8)))
                {
                    Fill = new FilterFill("Sepia")
                });
            ink.Insert(3, new PolylineShape(ImmutableArray.Create(new PointD(0, 0), new PointD(3.5, 2.25))));
            ink.Insert(4, new LineShape(new PointD(0, 0), new PointD(36, 22)));

            document.InsertLayer(0, photo);
            document.InsertLayer(1, overlay);
            document.InsertLayer(2, ink);

            photoId = photo.Id;
            inkId = ink.Id;
            return document;
        }

        private static byte[] SaveToBytes(Document document, IRasterCodec codec)
        {
            using var stream = new MemoryStream();
            new DocumentSerializer(codec).Save(document, stream);
            return stream.ToArray();
        }

        private static Document LoadFromBytes(byte[] bytes, IRasterCodec codec)
        {
            using var stream = new MemoryStream(bytes);
            return new DocumentSerializer(codec).Load(stream);
        }

        private static byte[] ContainerWith(string json, params (string Name, byte[] Data)[] files)
        {
            using var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = zip.CreateEntry("document.json");
                using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
                {
                    writer.Write(json);
                }

                foreach (var (name, data) in files)
                {
                    using var fileStream = zip.CreateEntry(name).Open();
                    fileStream.Write(data);
                }
            }

            return stream.ToArray();
        }

        private static byte[] RawPixels(int width, int height)
        {
            using var buffer = new UnmanagedImageBuffer(width, height);
            buffer.Clear(1, 2, 3, 4);
            using var stream = new MemoryStream();
            new RawBgraCodec().Write(buffer, stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Rounds the trip preserves structure settings pixels and shapes.
        /// </summary>
        [TestMethod]
        public void RoundTrip_PreservesStructureSettingsPixelsAndShapes()
        {
            using var original = BuildSampleDocument(out var photoId, out var inkId);
            var codec = new RawBgraCodec();

            using var loaded = LoadFromBytes(SaveToBytes(original, codec), codec);

            Assert.AreEqual(Width, loaded.Width);
            Assert.AreEqual(Height, loaded.Height);
            Assert.AreEqual(original.Layers.Count, loaded.Layers.Count);

            for (var i = 0; i < original.Layers.Count; i++)
            {
                var a = original.Layers[i];
                var b = loaded.Layers[i];

                Assert.AreEqual(a.GetType(), b.GetType());
                Assert.AreEqual(a.Id, b.Id);
                Assert.AreEqual(a.Name, b.Name);
                Assert.AreEqual(a.Visible, b.Visible);
                Assert.AreEqual(a.Opacity, b.Opacity, 0.000001);
                Assert.AreEqual(a.Blend, b.Blend);
            }

            Assert.AreEqual(photoId, loaded.Layers[0].Id);
            Assert.IsTrue(
                ((RasterLayer)loaded.Layers[0]).Pixels.BufferSpan.SequenceEqual(((RasterLayer)original.Layers[0]).Pixels
                    .BufferSpan));
            Assert.IsTrue(
                ((RasterLayer)loaded.Layers[1]).Pixels.BufferSpan.SequenceEqual(((RasterLayer)original.Layers[1]).Pixels
                    .BufferSpan));

            var inkA = (ShapeLayer)original.Layers[2];
            var inkB = (ShapeLayer)loaded.Layers[2];
            Assert.AreEqual(inkId, inkB.Id);
            Assert.AreEqual(inkA.Shapes.Count, inkB.Shapes.Count);
            for (var i = 0; i < inkA.Shapes.Count; i++)
            {
                Assert.AreEqual(inkA.Shapes[i].GetType(), inkB.Shapes[i].GetType());
                Assert.AreEqual(JsonSerializer.Serialize(inkA.Shapes[i]), JsonSerializer.Serialize(inkB.Shapes[i]));
            }
        }

        /// <summary>
        /// Loadeds the document flattens like the original.
        /// </summary>
        [TestMethod]
        public void LoadedDocument_FlattensLikeTheOriginal()
        {
            using var original = BuildSampleDocument(out _, out _);
            var codec = new RawBgraCodec();
            var rasterizer = new TestSupport.FakeRasterizer();

            using var loaded = LoadFromBytes(SaveToBytes(original, codec), codec);
            using var a = DocumentRenderer.Flatten(original, rasterizer);
            using var b = DocumentRenderer.Flatten(loaded, rasterizer);

            Assert.IsTrue(a.BufferSpan.SequenceEqual(b.BufferSpan));
        }

        /// <summary>
        /// Loadeds the document is fully editable.
        /// </summary>
        [TestMethod]
        public void LoadedDocument_IsFullyEditable()
        {
            using var original = BuildSampleDocument(out _, out var inkId);
            var codec = new RawBgraCodec();
            using var loaded = LoadFromBytes(SaveToBytes(original, codec), codec);
            var history = new DocumentHistory(loaded);

            history.Execute(new AddShapeCommand(inkId, new RectShape(0, 0, 1, 1)));
            history.Undo();

            Assert.AreEqual(5, ((ShapeLayer)loaded.Layers[2]).Shapes.Count);
        }

        [TestMethod]
        public void Container_HasReadableDescriptionAndOneFilePerRasterLayer()
        {
            using var document = BuildSampleDocument(out var photoId, out _);
            var bytes = SaveToBytes(document, new RawBgraCodec());

            using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
            var names = zip.Entries.Select(e => e.FullName).ToList();

            Assert.IsTrue(names.Contains("document.json"));
            Assert.IsTrue(names.Contains($"layers/{photoId:N}.bgra"));
            Assert.AreEqual(3, names.Count); // json + 2 raster layers, the shape layer lives in the json

            using var reader = new StreamReader(zip.GetEntry("document.json")!.Open());
            var json = reader.ReadToEnd();
            Assert.IsTrue(json.Contains("\"format\": \"slimdoc\""), json);
            Assert.IsTrue(json.Contains("\"version\": 1"), json);
            Assert.IsTrue(json.Contains("\"type\": \"rect\""), json);
        }

        [TestMethod]
        public void SaveToFile_ReplacesExistingFileAndLeavesNoTempFile()
        {
            var path = Path.Combine(Path.GetTempPath(), $"slimdoc-test-{Guid.NewGuid():N}.slimdoc");
            try
            {
                var serializer = new DocumentSerializer(new RawBgraCodec());
                File.WriteAllText(path, "old content");

                using (var document = BuildSampleDocument(out _, out _))
                {
                    serializer.Save(document, path);
                }

                Assert.IsFalse(File.Exists(path + ".tmp"));
                using var loaded = serializer.Load(path);
                Assert.AreEqual(3, loaded.Layers.Count);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
            }
        }

        [TestMethod]
        public void NotAZipFile_ThrowsFormatException()
        {
            var serializer = new DocumentSerializer(new RawBgraCodec());

            TestSupport.Throws<DocumentFormatException>(() =>
                serializer.Load(new MemoryStream(Encoding.UTF8.GetBytes("hello, I am not a zip"))));
        }

        [TestMethod]
        public void MissingDescription_ThrowsFormatException()
        {
            using var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                zip.CreateEntry("something-else.txt");
            }

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(stream.ToArray())));
        }

        [TestMethod]
        public void BrokenJson_ThrowsFormatException()
        {
            var bytes = ContainerWith("{ this is not json");

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(bytes)));
        }

        [TestMethod]
        public void WrongFormatName_ThrowsFormatException()
        {
            var bytes = ContainerWith("{\"format\":\"other\",\"version\":1,\"width\":1,\"height\":1,\"layers\":[]}");

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(bytes)));
        }

        [TestMethod]
        public void NewerVersion_IsRejectedWithAClearMessage()
        {
            var bytes = ContainerWith("{\"format\":\"slimdoc\",\"version\":99,\"width\":1,\"height\":1,\"layers\":[]}");

            try
            {
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(bytes));
                Assert.Fail("Expected a DocumentFormatException.");
            }
            catch (DocumentFormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("newer"), ex.Message);
            }
        }

        [TestMethod]
        public void InvalidSize_ThrowsFormatException()
        {
            var bytes = ContainerWith("{\"format\":\"slimdoc\",\"version\":1,\"width\":0,\"height\":5,\"layers\":[]}");

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(bytes)));
        }

        [TestMethod]
        public void MissingPixelFile_ThrowsFormatException()
        {
            var id = Guid.NewGuid();
            var json = "{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[" +
                       $"{{\"type\":\"raster\",\"id\":\"{id}\",\"name\":\"a\",\"file\":\"layers/{id:N}.bgra\"}}]}}";

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(ContainerWith(json))));
        }

        [TestMethod]
        public void PixelFileOfWrongSize_ThrowsFormatException()
        {
            var id = Guid.NewGuid();
            var file = $"layers/{id:N}.bgra";
            var json = "{\"format\":\"slimdoc\",\"version\":1,\"width\":10,\"height\":10,\"layers\":[" +
                       $"{{\"type\":\"raster\",\"id\":\"{id}\",\"name\":\"a\",\"file\":\"{file}\"}}]}}";

            var bytes = ContainerWith(json, (file, RawPixels(5, 5)));

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(bytes)));
        }

        [TestMethod]
        public void TruncatedPixelFile_ThrowsFormatException()
        {
            var id = Guid.NewGuid();
            var file = $"layers/{id:N}.bgra";
            var json = "{\"format\":\"slimdoc\",\"version\":1,\"width\":10,\"height\":10,\"layers\":[" +
                       $"{{\"type\":\"raster\",\"id\":\"{id}\",\"name\":\"a\",\"file\":\"{file}\"}}]}}";
            var pixels = RawPixels(10, 10);

            var bytes = ContainerWith(json, (file, pixels.AsSpan(0, pixels.Length / 2).ToArray()));

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(bytes)));
        }

        [TestMethod]
        public void PixelFileOutsideTheLayerFolder_IsRejected()
        {
            var id = Guid.NewGuid();
            var json = "{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[" +
                       $"{{\"type\":\"raster\",\"id\":\"{id}\",\"name\":\"a\",\"file\":\"../evil.bgra\"}}]}}";

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(
                    new MemoryStream(ContainerWith(json, ("../evil.bgra", RawPixels(4, 4))))));
        }

        [TestMethod]
        public void UnknownLayerTypeOrBlendMode_IsRejected()
        {
            var id = Guid.NewGuid();
            var unknownType = "{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[" +
                              $"{{\"type\":\"hologram\",\"id\":\"{id}\",\"name\":\"a\"}}]}}";
            var unknownBlend = "{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[" +
                               $"{{\"type\":\"shape\",\"id\":\"{id}\",\"name\":\"a\",\"blend\":\"Multiply\"}}]}}";
            var serializer = new DocumentSerializer(new RawBgraCodec());

            TestSupport.Throws<DocumentFormatException>(() =>
                serializer.Load(new MemoryStream(ContainerWith(unknownType))));
            TestSupport.Throws<DocumentFormatException>(() =>
                serializer.Load(new MemoryStream(ContainerWith(unknownBlend))));
        }

        [TestMethod]
        public void DuplicateLayerIds_AreRejected()
        {
            var id = Guid.NewGuid();
            var layer = $"{{\"type\":\"shape\",\"id\":\"{id}\",\"name\":\"a\"}}";
            var json =
                $"{{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[{layer},{layer}]}}";

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(ContainerWith(json))));
        }

        [TestMethod]
        public void InvalidShapeInFile_IsRejected()
        {
            var id = Guid.NewGuid();
            var json = "{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[" +
                       $"{{\"type\":\"shape\",\"id\":\"{id}\",\"name\":\"a\",\"shapes\":[" +
                       "{\"type\":\"rect\",\"x\":0,\"y\":0,\"width\":-5,\"height\":1}]}]}";

            TestSupport.Throws<DocumentFormatException>(() =>
                new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(ContainerWith(json))));
        }

        [TestMethod]
        public void OutOfRangeOpacityInFile_IsClamped()
        {
            var id = Guid.NewGuid();
            var json = "{\"format\":\"slimdoc\",\"version\":1,\"width\":4,\"height\":4,\"layers\":[" +
                       $"{{\"type\":\"shape\",\"id\":\"{id}\",\"name\":\"a\",\"opacity\":7}}]}}";

            using var loaded = new DocumentSerializer(new RawBgraCodec()).Load(new MemoryStream(ContainerWith(json)));

            Assert.AreEqual(1d, loaded.Layers[0].Opacity, 0.0001);
        }

        [TestMethod]
        public void EmptyDocument_RoundTrips()
        {
            using var document = new Document(6, 7);
            var codec = new RawBgraCodec();

            using var loaded = LoadFromBytes(SaveToBytes(document, codec), codec);

            Assert.AreEqual(6, loaded.Width);
            Assert.AreEqual(7, loaded.Height);
            Assert.AreEqual(0, loaded.Layers.Count);
        }

        [TestMethod]
        [TestCategory("Gdi")]
        public void PngCodec_RoundTripsPixelsIncludingAlpha()
        {
            using var buffer = new UnmanagedImageBuffer(9, 5);
            buffer.Clear(0, 0, 0, 0);
            buffer.SetPixel(1, 1, 200, 100, 50, 255);
            buffer.SetPixel(2, 2, 10, 20, 30, 128);
            var codec = new PngRasterCodec();

            using var stream = new MemoryStream();
            codec.Write(buffer, stream);
            stream.Position = 0;
            using var back = codec.Read(stream, 9, 5);

            TestSupport.AssertColor(back.GetPixel(1, 1), (200, 100, 50, 255), 0);
            TestSupport.AssertColor(back.GetPixel(2, 2), (10, 20, 30, 128), 1);
            TestSupport.AssertColor(back.GetPixel(0, 0), (0, 0, 0, 0), 0);
        }

        [TestMethod]
        [TestCategory("Gdi")]
        public void DefaultSerializer_UsesPng_AndCanReadRawFilesToo()
        {
            using var document = BuildSampleDocument(out var photoId, out _);
            var png = new DocumentSerializer();

            using var stream = new MemoryStream();
            png.Save(document, stream);
            stream.Position = 0;

            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
            {
                Assert.IsNotNull(zip.GetEntry($"layers/{photoId:N}.png"));
            }

            stream.Position = 0;
            using var loaded = png.Load(stream);
            Assert.AreEqual(3, loaded.Layers.Count);

            // A serializer whose write codec is PNG still reads .bgra files.
            using var fromRaw = png.Load(new MemoryStream(SaveToBytes(document, new RawBgraCodec())));
            Assert.AreEqual(3, fromRaw.Layers.Count);
        }
    }
}
