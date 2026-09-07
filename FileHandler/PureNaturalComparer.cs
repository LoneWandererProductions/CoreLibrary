/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        PathInformation.cs
 * PURPOSE:     File name comparer that implements pure natural sorting, comparing numeric parts of strings as numbers and non-numeric parts as text.
 *              This ensures that "file2" comes before "file10", and "file1a" comes before "file1b". It handles nulls and empty strings gracefully, treating null as less than any non-null string.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace FileHandler
{
    /// <inheritdoc />
    /// <summary>
    /// File name comparer that implements pure natural sorting, comparing numeric parts of strings as numbers and non-numeric parts as text.
    /// This ensures that "file2" comes before "file10", and "file1a" comes before "file1b". It handles nulls and empty strings gracefully, treating null as less than any non-null string.
    /// </summary>
    /// <seealso cref="!:System.Collections.Generic.IComparer&lt;System.String&gt;" />
    public sealed class PureNaturalComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;
            int ix = 0, iy = 0;

            while (ix < lx && iy < ly)
            {
                if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
                {
                    // Skip leading zeros to accurately check significant length
                    int startX = ix;
                    while (ix < lx && x[ix] == '0') ix++;

                    int startY = iy;
                    while (iy < ly && y[iy] == '0') iy++;

                    // Track start of non-zero digits
                    int nonZeroX = ix;
                    int nonZeroY = iy;

                    // Count total digit length
                    while (ix < lx && char.IsDigit(x[ix])) ix++;
                    while (iy < ly && char.IsDigit(y[iy])) iy++;

                    int lenX = ix - nonZeroX;
                    int lenY = iy - nonZeroY;

                    // 1. The number with more non-zero digits is larger
                    if (lenX != lenY)
                        return lenX.CompareTo(lenY);

                    // 2. Same digit count: compare digit by digit
                    for (int i = 0; i < lenX; i++)
                    {
                        if (x[nonZeroX + i] != y[nonZeroY + i])
                            return x[nonZeroX + i].CompareTo(y[nonZeroY + i]);
                    }

                    // 3. Numbers are numerically equal: tie-break using leading zero count (e.g., "01" vs "1")
                    int zeroCountX = nonZeroX - startX;
                    int zeroCountY = nonZeroY - startY;

                    if (zeroCountX != zeroCountY)
                        return zeroCountY.CompareTo(zeroCountX); // More leading zeros comes first ("01" < "1")
                }
                else
                {
                    int r = x[ix].CompareTo(y[iy]);
                    if (r != 0) return r;

                    ix++;
                    iy++;
                }
            }

            return lx.CompareTo(ly);
        }
    }
}