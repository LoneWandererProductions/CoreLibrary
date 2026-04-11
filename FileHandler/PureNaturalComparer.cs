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
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;
            for (int ix = 0, iy = 0; ix < lx && iy < ly;)
            {
                if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
                {
                    // Extract full numeric parts
                    string nx = "", ny = "";
                    while (ix < lx && char.IsDigit(x[ix])) nx += x[ix++];
                    while (iy < ly && char.IsDigit(y[iy])) ny += y[iy++];

                    // Compare as numbers
                    var r = long.Parse(nx).CompareTo(long.Parse(ny));
                    if (r != 0) return r;
                }
                else
                {
                    var r = x[ix].CompareTo(y[iy]);
                    if (r != 0) return r;

                    ix++;
                    iy++;
                }
            }

            return lx - ly;
        }
    }
}
