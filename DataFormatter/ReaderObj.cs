/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        ReaderObj.cs
 * PURPOSE:     A really basic obj File reader, does the basic stuff nothing more!
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataFormatter
{
    /// <summary>
    ///     Basic implementation to read Blender Files
    ///     Not feature complete
    /// </summary>
    public static class ReaderObj
    {
        /// <summary>
        ///     Reads the object.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Readable Obj File</returns>
        public static ObjFile? ReadObj(string filePath)
        {
            var lst = ReadText.ReadFile(filePath);
            if (lst == null) return null;

            var vectors = new List<TertiaryVector>();
            var faces = new List<TertiaryFace>();
            var other = new List<string>();

            foreach (var line in lst)
            {
                var trim = line.Trim();
                // Skip empty lines or comments
                if (string.IsNullOrEmpty(trim) || trim.StartsWith(DataFormatterResources.Comment))
                    continue;

                // 1. ROBUSTNESS FIX: Remove empty entries to handle multiple spaces safely
                // This prevents "v  1.0" from creating empty strings that break parsing.
                var rawBits = DataHelper.GetParts(trim, DataFormatterResources.Space);
                var bits = rawBits.Where(b => !string.IsNullOrWhiteSpace(b)).ToList();

                // --- VECTORS ---
                if (trim.StartsWith(DataFormatterResources.Vector))
                {
                    // bits[0] is "v", so we look at 1, 2, 3
                    if (bits.Count >= 4)
                    {
                        // 2. CULTURE FIX: Use InvariantCulture to handle "." decimals globally
                        if (double.TryParse(bits[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                            double.TryParse(bits[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                            double.TryParse(bits[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
                        {
                            vectors.Add(new TertiaryVector { X = x, Y = y, Z = z });
                        }
                    }

                    continue;
                }

                // --- FACES ---
                if (trim.StartsWith(DataFormatterResources.Face))
                {
                    // bits[0] is "f", so we look at 1, 2, 3
                    if (bits.Count >= 4)
                    {
                        // Helper to extract index from formats like "1/2/3"
                        var v1 = ParseIndex(bits[1]);
                        var v2 = ParseIndex(bits[2]);

                        // 3. LOGIC FIX: Changed to ParseIndex(bits[3]) instead of bits[1]
                        var v3 = ParseIndex(bits[3]);

                        if (v1 != 0 && v2 != 0 && v3 != 0)
                        {
                            faces.Add(new TertiaryFace { X = v1, Y = v2, Z = v3 });
                        }
                    }

                    continue;
                }

                other.Add(trim);
            }

            return new ObjFile { Face = faces, Vectors = vectors, Other = other };
        }

        /// <summary>
        /// Parses the index.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>Parsed value.</returns>
        private static int ParseIndex(string token)
        {
            // Split by '/' and take the first part (the vertex index)
            var parts = DataHelper.GetParts(token, '/');
            if (parts.Count > 0 && int.TryParse(parts[0], out var result))
            {
                return result;
            }

            return 0;
        }
    }
}
