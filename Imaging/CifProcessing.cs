﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/CifProcessing.cs
 * PURPOSE:     Processing of our custom Image Format, with custom compression
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DataFormatter;
using ExtendedSystemObjects;
using Mathematics;

namespace Imaging
{
    //TODO add checksum Fun and custom image Exceptions

    /// <summary>
    ///     Basic Cif Processing
    /// </summary>
    internal static class CifProcessing
    {
        /// <summary>
        ///     Converts to cif.
        /// </summary>
        /// <param name="image">The image.</param>
        internal static Dictionary<Color, SortedSet<int>> ConvertToCif(Bitmap image)
        {
            var imageFormat = new Dictionary<Color, SortedSet<int>>();

            var dbm = DirectBitmap.GetInstance(image);

            var colorMap = dbm.GetColors();

            for (var i = 0; i < image.Height * image.Width; i++)
            {
                var color = colorMap[i];

                if (!imageFormat.ContainsKey(color))
                {
                    imageFormat[color] = new SortedSet<int>();
                }

                imageFormat[color].Add(i);
            }

            return imageFormat;
        }

        /// <summary>
        ///     Cif to image.
        /// </summary>
        /// <param name="csv">The csv.</param>
        /// <returns>The converted Image</returns>
        [return: MaybeNull]
        internal static Bitmap CifToImage(List<List<string>> csv)
        {
            //get image size
            var check = int.TryParse(csv[0][0], out var height);
            if (!check)
            {
                return null;
            }

            check = int.TryParse(csv[0][1], out var width);
            if (!check)
            {
                return null;
            }

            //remove the Height, length csv
            csv.RemoveAt(0);

            var image = new Bitmap(height, width);

            var dbm = DirectBitmap.GetInstance(image);

            foreach (var line in csv)
            {
                var hex = line[0];

                check = int.TryParse(line[1], out var a);

                if (!check)
                {
                    continue;
                }

                var converter = new ColorHsv(hex, a);

                var color = Color.FromArgb((byte)converter.A, (byte)converter.R, (byte)converter.G,
                    (byte)converter.B);

                //get coordinates
                for (var i = 2; i < line.Count; i++)
                {
                    check = int.TryParse(line[i], out var idMaster);
                    if (!check)
                    {
                        continue;
                    }

                    var coordinate = Coordinate2D.GetInstance(idMaster, width);
                    dbm.SetPixel(coordinate.X, coordinate.Y, color);
                }
            }

            return dbm.Bitmap;
        }

        /// <summary>
        ///     Gets the cif from file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Cif Image</returns>
        internal static Cif GetCifFromFile(string path)
        {
            var csv = CsvHandler.ReadCsv(path, ImagingResources.Separator);
            if (csv == null)
            {
                return null;
            }

            //get image size
            var check = int.TryParse(csv[0][0], out var height);

            if (!check)
            {
                return null;
            }


            check = int.TryParse(csv[0][1], out var width);
            if (!check)
            {
                return null;
            }

            var cif = new Cif {Height = height, Width = width, CifImage = new Dictionary<Color, SortedSet<int>>()};


            if (csv[0][2] == ImagingResources.CifCompressed)
            {
                cif.Compressed = true;
            }

            //remove the Height, length csv
            csv.RemoveAt(0);

            if (cif.Compressed)
            {
                foreach (var line in csv)
                {
                    var hex = line[0];

                    check = int.TryParse(line[1], out var a);

                    if (!check)
                    {
                        continue;
                    }

                    var converter = new ColorHsv(hex, a);

                    var color = Color.FromArgb((byte)converter.A, (byte)converter.R, (byte)converter.G,
                        (byte)converter.B);

                    //get coordinates
                    for (var i = 2; i < line.Count; i++)
                    {
                        if (line[i].Contains("-"))
                        {
                            //split get start and end
                            var lst = line[i].Split(ImagingResources.CifSeparator).ToList();
                            check = int.TryParse(lst[0], out var start);

                            if (!check)
                            {
                                continue;
                            }

                            check = int.TryParse(lst[1], out var end);

                            if (!check)
                            {
                                continue;
                            }

                            //paint area
                            for (var idMaster = start; idMaster <= end; idMaster++)
                            {
                                if (cif.CifImage.ContainsKey(color))
                                {
                                    cif.CifImage[color].Add(idMaster);
                                }
                                else
                                {
                                    cif.CifImage.Add(color, new SortedSet<int>());
                                }

                                cif.CifImage[color].Add(idMaster);
                            }
                        }
                        else
                        {
                            check = int.TryParse(line[i], out var idMaster);

                            if (!check)
                            {
                                continue;
                            }

                            if (cif.CifImage.ContainsKey(color))
                            {
                                cif.CifImage[color].Add(idMaster);
                            }
                            else
                            {
                                cif.CifImage.Add(color, new SortedSet<int>());
                            }

                            cif.CifImage[color].Add(idMaster);
                        }
                    }
                }
            }
            else
            {
                foreach (var line in csv)
                {
                    var hex = line[0];

                    check = int.TryParse(line[1], out var a);

                    if (!check)
                    {
                        continue;
                    }

                    var converter = new ColorHsv(hex, a);

                    var color = Color.FromArgb((byte)converter.A, (byte)converter.R, (byte)converter.G,
                        (byte)converter.B);

                    //get coordinates
                    for (var i = 2; i < line.Count; i++)
                    {
                        check = int.TryParse(line[i], out var idMaster);
                        if (!check)
                        {
                            continue;
                        }

                        if (cif.CifImage.ContainsKey(color))
                        {
                            cif.CifImage[color].Add(idMaster);
                        }
                        else
                        {
                            cif.CifImage.Add(color, new SortedSet<int>());
                        }

                        cif.CifImage[color].Add(idMaster);
                    }
                }
            }

            return cif;
        }

        /// <summary>
        ///     Cif to image.
        /// </summary>
        /// <param name="csv">The csv.</param>
        /// <returns>The converted Image</returns>
        [return: MaybeNull]
        internal static Bitmap CifToImageCompressed(List<List<string>> csv)
        {
            //get image size
            var check = int.TryParse(csv[0][0], out var height);

            if (!check)
            {
                return null;
            }

            check = int.TryParse(csv[0][1], out var width);
            if (!check)
            {
                return null;
            }

            //remove the Height, length csv
            csv.RemoveAt(0);

            var image = new Bitmap(height, width);

            var dbm = DirectBitmap.GetInstance(image);

            foreach (var line in csv)
            {
                var hex = line[0];

                check = int.TryParse(line[1], out var a);

                if (!check)
                {
                    continue;
                }

                var converter = new ColorHsv(hex, a);

                var color = Color.FromArgb((byte)converter.A, (byte)converter.R, (byte)converter.G,
                    (byte)converter.B);

                //get coordinates
                for (var i = 2; i < line.Count; i++)
                {
                    if (line[i].Contains("-"))
                    {
                        //split get start and end
                        var lst = line[i].Split(ImagingResources.CifSeparator).ToList();
                        check = int.TryParse(lst[0], out var start);

                        if (!check)
                        {
                            continue;
                        }

                        check = int.TryParse(lst[1], out var end);

                        if (!check)
                        {
                            continue;
                        }

                        //paint area
                        for (var idMaster = start; idMaster <= end; idMaster++)
                        {
                            var coordinate = Coordinate2D.GetInstance(idMaster, width);
                            dbm.SetPixel(coordinate.X, coordinate.Y, color);
                        }
                    }
                    else
                    {
                        check = int.TryParse(line[i], out var idMaster);

                        if (!check)
                        {
                            continue;
                        }

                        var coordinate = Coordinate2D.GetInstance(idMaster, width);
                        dbm.SetPixel(coordinate.X, coordinate.Y, color);
                    }
                }
            }

            return dbm.Bitmap;
        }

        /// <summary>
        ///     Generates the CSV.
        /// </summary>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageFormat">The image format.</param>
        /// <returns>Cif Format ready to be saved as csv.</returns>
        internal static List<List<string>> GenerateCsv(int imageHeight, int imageWidth,
            Dictionary<Color, SortedSet<int>> imageFormat)
        {
            var master = new List<List<string>>();
            //first line is size of the image, compression, Number of Ids, used for checking and lines and number of Colors, added later
            var child = new List<string>(4)
            {
                imageHeight.ToString(),
                imageWidth.ToString(),
                ImagingResources.CifUnCompressed,
                (imageHeight * imageWidth).ToString()
            };

            master.Add(child);

            //add colors and Points
            foreach (var (key, value) in imageFormat)
            {
                //Possible error here
                var converter = new ColorHsv(key.R, key.G, key.B, key.A);
                //First two keys are color and Hue
                var subChild = new List<string>(2) {converter.Hex, key.A.ToString()};

                subChild.AddRange(value.Select(id => id.ToString()));

                master.Add(subChild);
            }

            //add number of Colors as CheckSum
            master[0].Add(master.Count.ToString());

            return master;
        }

        /// <summary>
        ///     Generates the CSV compressed.
        /// </summary>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageFormat">The image format.</param>
        /// <returns>Compressed image</returns>
        internal static List<List<string>> GenerateCsvCompressed(int imageHeight, int imageWidth,
            Dictionary<Color, SortedSet<int>> imageFormat)
        {
            var master = new List<List<string>>();
            //first line is size of the image, compression, Number of Ids, used for checking and lines and number of Colors, added later
            var child = new List<string>(4)
            {
                imageHeight.ToString(),
                imageWidth.ToString(),
                ImagingResources.CifCompressed,
                (imageHeight * imageWidth).ToString()
            };

            master.Add(child);

            //add colors and Points
            foreach (var (key, value) in imageFormat)
            {
                var converter = new ColorHsv(key.R, key.G, key.B, key.A);
                //First two keys are color and Hue
                var subChild = new List<string>(2) {converter.Hex, key.A.ToString()};

                var sequence = Utility.Sequencer(value, 3);

                var compressed = new List<int>();

                if (sequence == null)
                {
                    continue;
                }

                var sortedList = new List<int>(value);

                foreach (var (startS, endS) in sequence)
                {
                    var start = sortedList[startS];
                    var end = sortedList[endS];
                    var cache = string.Concat(start, ImagingResources.CifSeparator, end);
                    subChild.Add(cache);

                    var range = new List<int>(sortedList.GetRange(startS, endS - startS));
                    compressed.AddRange(range);
                }

                var intersect = compressed.Except(value).ToList();
                var modulo = intersect.ConvertAll(i => i.ToString());

                subChild.AddRange(modulo);

                master.Add(subChild);
            }

            //add number of Colors as CheckSum
            master[0].Add(master.Count.ToString());

            return master;
        }
    }
}
