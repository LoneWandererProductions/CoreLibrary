/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandleSort.cs
 * PURPOSE:     Extension for File Sort
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace FileHandler
{
    /// <summary>
    ///     Sorting helpers for file paths using <see cref="FilePathStruct"/> for
    ///     natural, human-friendly path sorting.
    /// </summary>
    public static class FileHandleSort
    {
        /// <summary>
        /// Sorts any sequence based on a path string extracted from the objects.
        /// </summary>
        /// <typeparam name="T">Generic Container, it must contain paths though.</typeparam>
        /// <param name="values">The values.</param>
        /// <param name="pathSelector">The path selector.</param>
        /// <returns>
        ///     A sorted list. If the input is <c>null</c> or has 0–1 elements,
        ///     the original list is returned unchanged.
        /// </returns>
        public static List<T>? PathSort<T>(this IEnumerable<T> values, Func<T, string> pathSelector)
        {
            if (values is null)
                return new List<T>();

            return values
                .OrderBy(v => new FilePathWrapper(pathSelector(v)))
                .ToList();
        }

        /// <summary>
        ///     Sorts a list of file path strings using <see cref="FilePathStruct"/>
        ///     to improve ordering (e.g. <c>file2</c> comes before <c>file10</c>).
        /// </summary>
        /// <param name="value">The list of file paths.</param>
        /// <returns>
        ///     A sorted list. If the input is <c>null</c> or has 0–1 elements,
        ///     the original list is returned unchanged.
        /// </returns>
        public static List<string> PathSort(this List<string>? value)
        {
            if (value is null || value.Count <= 1)
                return value ?? new List<string>();

            return value
                .Select(path => new FilePathWrapper(path))
                .OrderBy(fps => fps)
                .Select(fps => fps.FullPath)
                .ToList();
        }
    }
}
