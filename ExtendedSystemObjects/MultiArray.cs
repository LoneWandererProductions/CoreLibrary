/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/MultiArray.cs
 * PURPOSE:     Utility extensions for 2D and jagged arrays, with focus on performance and unsafe access.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 *
 * DESCRIPTION:
 *     This file contains a set of static helper methods to simplify working with multi-dimensional arrays.
 *     It includes operations like deep cloning, row/column swapping, equality checks, span conversion,
 *     and conversions between jagged and rectangular arrays using unsafe code for maximum performance.
 */

using System;
using System.Runtime.InteropServices;
using System.Text;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <summary>
    ///     Provides utility extensions for 2D arrays (`[,]`) and jagged arrays (`[][]`),
    ///     including efficient operations such as swapping rows/columns, deep copying, comparing,
    ///     and converting to spans or between formats.
    ///     Designed for use with unmanaged types to leverage unsafe memory access for performance.
    /// </summary>
    /// <remarks>
    ///     All methods are `static` and operate on arrays passed as parameters.
    ///     Unsafe context is used in several methods to improve performance via pointer access.
    /// </remarks>
    public static class MultiArray
    {
        /// <summary>
        ///     Swaps the row.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="xOne">The first row.</param>
        /// <param name="xTwo">The second row.</param>
        public static void SwapColumn<TValue>(this TValue[,] array, int xOne, int xTwo)
        {
            for (var i = 0; i < array.GetLength(1); i++)
            {
                (array[xOne, i], array[xTwo, i]) = (array[xTwo, i], array[xOne, i]);
            }
        }

        /// <summary>
        ///     Swaps the row.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="xOne">The x one.</param>
        /// <param name="xTwo">The x two.</param>
        public static void SwapRow<TValue>(this TValue[,] array, int xOne, int xTwo)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                (array[i, xOne], array[i, xTwo]) = (array[i, xTwo], array[i, xOne]);
            }
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="array">The array.</param>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public static unsafe string ToText<TValue>(this TValue[,] array) where TValue : unmanaged
        {
            var str = new StringBuilder();

            var length = array.GetLength(0) * array.GetLength(1);
            var row = array.GetLength(1);

            fixed (TValue* one = array)
            {
                for (var i = 0; i < length; i++)
                {
                    var tmp = one[i];
                    _ = str.Append(tmp);

                    if ((i + 1) % row == 0 && i + 1 >= row)
                    {
                        _ = str.Append(Environment.NewLine);
                    }
                    else
                    {
                        _ = str.Append(SharedResources.Separator);
                    }
                }
            }

            return str.ToString();
        }

        /// <summary>
        ///     Duplicates the specified array.
        ///     Deep Copy of the array
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="array">The array.</param>
        /// <returns>Copy of the called array</returns>
        public static unsafe TValue[,] Duplicate<TValue>(this TValue[,] array) where TValue : unmanaged
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var result = new TValue[rows, cols];

            fixed (TValue* src = array, dest = result)
            {
                long bytes = (long)rows * cols * sizeof(TValue);
                Buffer.MemoryCopy(src, dest, bytes, bytes);
            }
            return result;
        }

        /// <summary>
        ///     Equals the specified arrays.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="compare">The compare target.</param>
        /// <returns>Equal or not</returns>
        public static bool Equal<TValue>(this TValue[,] array, TValue[,] compare) where TValue : unmanaged
        {
            if (array.GetLength(0) != compare.GetLength(0) ||
                array.GetLength(1) != compare.GetLength(1)) return false;

            return array.ToSpan().SequenceEqual(compare.ToSpan());
        }

        /// <summary>
        ///     Converts an array to span.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="array">The array.</param>
        /// <returns>A Multi array as span Type.</returns>
        public static Span<TValue> ToSpan<TValue>(this TValue[,] array) where TValue : unmanaged
        {
            return MemoryMarshal.CreateSpan(ref array[0, 0], array.Length);
        }

        /// <summary>
        ///     Converts the specified jagged array.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="jaggedArray">The jagged array.</param>
        /// <returns>Converted Jagged Array</returns>
        public static unsafe TValue[,] Convert<TValue>(this TValue[][] jaggedArray) where TValue : unmanaged
        {
            var rows = jaggedArray.Length;
            var cols = jaggedArray[0].Length;

            // Allocate a rectangular array
            var result = new TValue[rows, cols];

            // Iterate through each row
            for (var i = 0; i < rows; i++)
            {
                // Pin the current row
                fixed (TValue* rowPtr = jaggedArray[i])
                {
                    for (var j = 0; j < cols; j++)
                    {
                        result[i, j] = rowPtr[j]; // Copy the value directly
                    }
                }
            }

            return result;
        }
    }
}
