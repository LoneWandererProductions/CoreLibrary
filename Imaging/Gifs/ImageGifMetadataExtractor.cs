/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Imaging.Gifs
* FILE:        ImageGifMetadataExtractor.cs
* PURPOSE:     Get all the info of a gif
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Diagnostics;
using System.IO;

namespace Imaging.Gifs
{
    /// <summary>
    ///     Information about the gif
    /// </summary>
    internal static class ImageGifMetadataExtractor
    {
        /// <summary>
        ///     Extracts the GIF metadata.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Metadata of the gif in question.</returns>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        /// <exception cref="InvalidDataException">Not a valid GIF file.</exception>
        internal static ImageGifInfo? ExtractGifMetadata(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(ImagingResources.FileNotFoundMessage, filePath);
            }

            var metadata = new ImageGifInfo
            {
                Name = Path.GetFileName(filePath),
                Size = new FileInfo(filePath).Length
            };

            double lastFrameDelay = 0;

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            // 1. Read GIF Header
            metadata.Header = new string(reader.ReadChars(ImagingResources.GifHeaderLength));
            if (!metadata.Header.StartsWith(ImagingResources.GifHeaderStart, StringComparison.Ordinal))
            {
                throw new InvalidDataException(ImagingResources.InvalidGifMessage);
            }

            // 2. Logical Screen Descriptor
            metadata.Width = reader.ReadInt16();
            metadata.Height = reader.ReadInt16();
            var packedFields = reader.ReadByte();
            metadata.BackgroundColorIndex = reader.ReadByte();
            metadata.PixelAspectRatio = reader.ReadByte();

            metadata.HasGlobalColorTable = (packedFields & ImagingResources.GlobalColorTableFlag) != 0;
            metadata.ColorResolution = ((packedFields & ImagingResources.ColorResolutionMask) >> 4) + 1;

            if (metadata.HasGlobalColorTable)
            {
                metadata.GlobalColorTableSize = 3 * (1 << ((packedFields & ImagingResources.TableSizeMask) + 1));
                reader.BaseStream.Seek(metadata.GlobalColorTableSize, SeekOrigin.Current);
            }

            // 3. Main Block Loop
            // We use a single loop to process the file linearly
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var blockId = reader.ReadByte();

                // Skip padding/null bytes between blocks
                if (blockId == ImagingResources.PaddingBlockId) continue;

                switch (blockId)
                {
                    case ImagingResources.ExtensionIntroducer:
                        var extensionLabel = reader.ReadByte();
                        switch (extensionLabel)
                        {
                            case ImagingResources.ApplicationExtensionLabel:
                                reader.BaseStream.Seek(1, SeekOrigin.Current); // Block Size
                                var appIdentifier = new string(reader.ReadChars(ImagingResources.AppIdentifierLength));
                                reader.BaseStream.Seek(ImagingResources.AppAuthCodeLength, SeekOrigin.Current);

                                if (appIdentifier == ImagingResources.NetScapeIdentifier)
                                {
                                    reader.BaseStream.Seek(2, SeekOrigin.Current); // Sub-block size + loop flag
                                    metadata.LoopCount = reader.ReadInt16();
                                    reader.ReadByte(); // Block terminator
                                }
                                else
                                {
                                    SkipExtensionBlocks(reader);
                                }
                                break;

                            case ImagingResources.GraphicsControlExtensionLabel:
                                reader.BaseStream.Seek(1, SeekOrigin.Current); // Block Size (usually 4)
                                reader.BaseStream.Seek(1, SeekOrigin.Current); // Packed fields

                                // Delay is in 100ths of a second. 
                                // Note: If you want ms, multiply by 10 here or in the ImageControl.
                                lastFrameDelay = reader.ReadInt16();

                                reader.BaseStream.Seek(1, SeekOrigin.Current); // Transparent Color Index
                                reader.ReadByte(); // Block Terminator (0x00)
                                break;

                            default:
                                // Comment, Plain Text, or unknown extensions
                                SkipExtensionBlocks(reader);
                                break;
                        }
                        break;

                    case ImagingResources.ImageDescriptorId:
                        // Record the frame info
                        metadata.Frames.Add(new FrameInfo
                        {
                            Description = ImagingResources.ImageFrameDescription,
                            DelayTime = lastFrameDelay
                        });

                        // Skip Image Descriptor (Left, Top, Width, Height = 8 bytes)
                        reader.BaseStream.Seek(8, SeekOrigin.Current);
                        var imgPacked = reader.ReadByte();

                        // Local Color Table
                        if ((imgPacked & ImagingResources.LocalColorTableFlag) != 0)
                        {
                            var localTableSize = 3 * (1 << ((imgPacked & ImagingResources.TableSizeMask) + 1));
                            reader.BaseStream.Seek(localTableSize, SeekOrigin.Current);
                        }

                        // LZW Minimum Code Size (1 byte)
                        reader.ReadByte();

                        // Skip Image Data Sub-blocks (This is where the 'forever' hang usually was)
                        SkipSubBlocks(reader);
                        break;

                    case ImagingResources.TrailerBlockId:
                        Trace.WriteLine(ImagingResources.GifTrailerMessage);
                        return metadata;

                    default:
                        Trace.WriteLine(string.Format(ImagingResources.UnknownBlockMessage, blockId));
                        SkipUnknownBlock(reader, blockId);
                        break;
                }
            }

            return metadata;
        }

        /// <summary>
        ///     Skips the unknown block.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="blockId">The block identifier.</param>
        private static void SkipUnknownBlock(BinaryReader reader, byte blockId)
        {
            Trace.WriteLine(string.Format(ImagingResources.SkipUnknownBlockMessage, blockId));

            // Quick and dirty: loop only as long as there is data to read
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var subBlockSize = reader.ReadByte();

                // If size is 0, we found the terminator. 
                // Using '0' directly is safer than a resource ID if the file is weird.
                if (subBlockSize == 0) break;

                // Skip the chunk. If the seek would go out of bounds, just jump to the end.
                if (reader.BaseStream.Position + subBlockSize > reader.BaseStream.Length)
                {
                    reader.BaseStream.Position = reader.BaseStream.Length;
                    break;
                }

                reader.BaseStream.Seek(subBlockSize, SeekOrigin.Current);
            }
        }

        /// <summary>
        /// Safely skips sub-blocks until a terminator (0x00) is found.
        /// Prevents infinite loops on corrupted files.
        /// </summary>
        private static void SkipSubBlocks(BinaryReader reader)
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var size = reader.ReadByte();
                if (size == 0) break;

                if (reader.BaseStream.Position + size > reader.BaseStream.Length)
                {
                    reader.BaseStream.Position = reader.BaseStream.Length;
                    break;
                }

                reader.BaseStream.Seek(size, SeekOrigin.Current);
            }
        }

        /// <summary>
        ///     Skips the extension blocks.
        /// </summary>
        /// <param name="reader">The reader.</param>
        private static void SkipExtensionBlocks(BinaryReader reader)
        {
            // Use the stream length check to prevent infinite loops
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var blockSize = reader.ReadByte();
                if (blockSize == 0) // 0 is always the terminator
                {
                    break;
                }

                // Safety check before seeking
                if (reader.BaseStream.Position + blockSize > reader.BaseStream.Length)
                {
                    reader.BaseStream.Position = reader.BaseStream.Length;
                    break;
                }

                reader.BaseStream.Seek(blockSize, SeekOrigin.Current);
            }
        }
    }
}
