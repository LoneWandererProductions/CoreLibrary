/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        DocumentSerializer.cs
 * PURPOSE:     Saves and loads a Document as a zip container:
 *                document.json            layer list, settings, all shapes (versioned)
 *                layers/{id}.png|.bgra    pixels of every raster layer
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Interfaces;
using Imaging.Objects.Shapes;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Reads and writes the <c>.slimdoc</c> format. The file layout is described by small private DTOs, not by the
    ///     runtime classes, so the model can change without silently changing the format.
    /// </summary>
    public sealed class DocumentSerializer
    {
        /// <summary>Value of the <c>format</c> field.</summary>
        public const string FormatName = "slimdoc";

        /// <summary>The newest version this build reads and writes. Files with a higher version are rejected.</summary>
        public const int CurrentVersion = 1;

        /// <summary>Recommended file extension.</summary>
        public const string FileExtension = ".slimdoc";

        private const string DocumentEntry = "document.json";

        private const string LayerFolder = "layers/";

        private const string RasterType = "raster";

        private const string ShapeType = "shape";

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IReadOnlyList<IRasterCodec> _readCodecs;

        private readonly IRasterCodec _writeCodec;

        /// <summary>Creates a serializer that stores layers as PNG.</summary>
        public DocumentSerializer()
            : this(new PngRasterCodec())
        {
        }

        /// <summary>Creates a serializer with a specific codec for writing. Both known codecs are accepted when reading.</summary>
        public DocumentSerializer(IRasterCodec writeCodec)
        {
            ArgumentNullException.ThrowIfNull(writeCodec);

            _writeCodec = writeCodec;

            var codecs = new List<IRasterCodec> { writeCodec };
            foreach (var known in new IRasterCodec[] { new PngRasterCodec(), new RawBgraCodec() })
            {
                if (!codecs.Exists(c =>
                        string.Equals(c.Extension, known.Extension, StringComparison.OrdinalIgnoreCase)))
                {
                    codecs.Add(known);
                }
            }

            _readCodecs = codecs;
        }

        /// <summary>Saves to a file. The data goes to a temporary file first, so a crash cannot destroy an existing document.</summary>
        public void Save(Document document, string path)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentException.ThrowIfNullOrEmpty(path);

            var temp = path + ".tmp";
            try
            {
                using (var stream = File.Create(temp))
                {
                    Save(document, stream);
                }

                File.Move(temp, path, overwrite: true);
            }
            catch
            {
                if (File.Exists(temp)) File.Delete(temp);
                throw;
            }
        }

        /// <summary>Saves to a stream (left open).</summary>
        public void Save(Document document, Stream output)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(output);

            var dto = new DocumentDto
            {
                Format = FormatName,
                Version = CurrentVersion,
                Width = document.Width,
                Height = document.Height,
                Layers = new List<LayerDto>()
            };

            foreach (var layer in document.Layers)
            {
                var item = new LayerDto
                {
                    Id = layer.Id,
                    Name = layer.Name,
                    Visible = layer.Visible,
                    Opacity = layer.Opacity,
                    Blend = layer.Blend.ToString()
                };

                switch (layer)
                {
                    case RasterLayer raster:
                        item.Type = RasterType;
                        item.File = $"{LayerFolder}{raster.Id:N}{_writeCodec.Extension}";
                        break;

                    case ShapeLayer shapes:
                        item.Type = ShapeType;
                        item.Shapes = new List<Shape>(shapes.Shapes);
                        break;

                    default:
                        throw new NotSupportedException($"Layer type {layer.GetType().Name} cannot be saved.");
                }

                dto.Layers.Add(item);
            }

            using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);

            var json = zip.CreateEntry(DocumentEntry, CompressionLevel.Optimal);
            using (var jsonStream = json.Open())
            {
                JsonSerializer.Serialize(jsonStream, dto, Options);
            }

            foreach (var layer in document.Layers)
            {
                if (layer is not RasterLayer raster) continue;

                var entry = zip.CreateEntry($"{LayerFolder}{raster.Id:N}{_writeCodec.Extension}",
                    _writeCodec.IsCompressed ? CompressionLevel.NoCompression : CompressionLevel.Fastest);

                using var entryStream = entry.Open();
                _writeCodec.Write(raster.Pixels, entryStream);
            }
        }

        /// <summary>Loads a document from a file.</summary>
        /// <exception cref="DocumentFormatException">The file is damaged, unsupported or too new.</exception>
        public Document Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);

            using var stream = File.OpenRead(path);
            return Load(stream);
        }

        /// <summary>Loads a document from a stream (left open).</summary>
        /// <exception cref="DocumentFormatException">The data is damaged, unsupported or too new.</exception>
        public Document Load(Stream input)
        {
            ArgumentNullException.ThrowIfNull(input);

            try
            {
                using var zip = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
                return Read(zip);
            }
            catch (InvalidDataException ex)
            {
                throw new DocumentFormatException("The file is not a valid document container.", ex);
            }
            catch (JsonException ex)
            {
                throw new DocumentFormatException("The document description is damaged.", ex);
            }
        }

        private Document Read(ZipArchive zip)
        {
            var entry = zip.GetEntry(DocumentEntry) ??
                        throw new DocumentFormatException($"The container has no {DocumentEntry}.");

            DocumentDto? dto;
            using (var stream = entry.Open())
            {
                dto = JsonSerializer.Deserialize<DocumentDto>(stream, Options);
            }

            if (dto is null || dto.Format != FormatName)
            {
                throw new DocumentFormatException("This is not a SlimViewer document.");
            }

            if (dto.Version < 1)
            {
                throw new DocumentFormatException($"Invalid document version {dto.Version}.");
            }

            if (dto.Version > CurrentVersion)
            {
                throw new DocumentFormatException(
                    $"The document was written by a newer version (format {dto.Version}, this build reads up to {CurrentVersion}).");
            }

            Document document;
            try
            {
                document = new Document(dto.Width, dto.Height);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new DocumentFormatException($"Invalid document size {dto.Width}x{dto.Height}.", ex);
            }

            try
            {
                var seen = new HashSet<Guid>();

                foreach (var item in dto.Layers ?? new List<LayerDto>())
                {
                    if (item is null) throw new DocumentFormatException("The layer list contains an empty entry.");
                    if (!seen.Add(item.Id)) throw new DocumentFormatException($"Layer id {item.Id} appears twice.");

                    var layer = ReadLayer(zip, item, document.Width, document.Height);
                    try
                    {
                        document.InsertLayer(document.Layers.Count, layer);
                    }
                    catch
                    {
                        (layer as IDisposable)?.Dispose();
                        throw;
                    }
                }

                return document;
            }
            catch
            {
                document.Dispose();
                throw;
            }
        }

        private Layer ReadLayer(ZipArchive zip, LayerDto item, int width, int height)
        {
            if (!Enum.TryParse<BlendMode>(item.Blend ?? nameof(BlendMode.Normal), true, out var blend) ||
                !Enum.IsDefined(blend))
            {
                throw new DocumentFormatException($"Layer {item.Id} uses the unknown blend mode '{item.Blend}'.");
            }

            Layer layer;

            switch (item.Type)
            {
                case RasterType:
                    layer = ReadRasterLayer(zip, item, width, height);
                    break;

                case ShapeType:
                    var shapes = new ShapeLayer(item.Id, item.Name);
                    foreach (var shape in item.Shapes ?? new List<Shape>())
                    {
                        if (shape?.IsValid() != true)
                        {
                            throw new DocumentFormatException($"Layer {item.Id} contains an invalid shape.");
                        }

                        if (shapes.IndexOf(shape.Id) >= 0)
                        {
                            throw new DocumentFormatException($"Layer {item.Id} contains shape {shape.Id} twice.");
                        }

                        shapes.Insert(shapes.Shapes.Count, shape);
                    }

                    layer = shapes;
                    break;

                default:
                    throw new DocumentFormatException($"Layer {item.Id} has the unknown type '{item.Type}'.");
            }

            layer.Visible = item.Visible;
            layer.Opacity = item.Opacity;
            layer.Blend = blend;
            return layer;
        }

        private RasterLayer ReadRasterLayer(ZipArchive zip, LayerDto item, int width, int height)
        {
            var file = item.File;
            if (string.IsNullOrEmpty(file) || !file.StartsWith(LayerFolder, StringComparison.Ordinal))
            {
                throw new DocumentFormatException($"Layer {item.Id} has an invalid pixel file name.");
            }

            var entry = zip.GetEntry(file) ??
                        throw new DocumentFormatException($"The pixel data '{file}' of layer {item.Id} is missing.");

            var extension = Path.GetExtension(file);
            var codec = _readCodecs.FirstOrDefault(c =>
                            string.Equals(c.Extension, extension, StringComparison.OrdinalIgnoreCase)) ??
                        throw new DocumentFormatException(
                            $"Layer {item.Id} uses the unsupported pixel format '{extension}'.");

            using var stream = entry.Open();
            var pixels = codec.Read(stream, width, height);
            return new RasterLayer(item.Id, item.Name, pixels);
        }

        private sealed class DocumentDto
        {
            public string? Format { get; set; }

            public int Version { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public List<LayerDto>? Layers { get; set; }
        }

        private sealed class LayerDto
        {
            public string? Type { get; set; }

            public Guid Id { get; set; }

            public string? Name { get; set; }

            public bool Visible { get; set; } = true;

            public double Opacity { get; set; } = 1.0;

            public string? Blend { get; set; }

            public string? File { get; set; }

            public List<Shape>? Shapes { get; set; }
        }
    }
}
