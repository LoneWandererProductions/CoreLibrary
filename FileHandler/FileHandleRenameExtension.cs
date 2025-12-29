/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandleRenameExtension.cs
 * PURPOSE:     Unified safe rename helpers (append/remove/replace/reorder)
 * PROGRAMER:   Peter Geinitz (Wayfarer) 
 */

using System;
using System.Linq;

namespace FileHandler
{
    /// <summary>
    ///     String helpers for renaming files.
    ///     All operations share a unified input normalization pipeline,
    ///     but preserve the outward behavior of the original implementation.
    /// </summary>
    public static class FileHandleRenameExtension
    {
        /// <summary>
        /// Removes an appendage if present.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="appendage">The appendage.</param>
        /// <param name="comparison">The comparison.</param>
        /// <returns>New File name.</returns>
        public static string RemoveAppendage(this string str, string appendage,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            str = NormalizeInput(str);
            appendage = NormalizeSubInput(appendage);

            return !str.StartsWith(appendage, comparison)
                ? str
                : str.Substring(appendage.Length);
        }

        /// <summary>
        /// Adds an appendage if missing.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="appendage">The appendage.</param>
        /// <param name="comparison">The comparison.</param>
        /// <returns>New File name.</returns>
        public static string AddAppendage(this string str, string appendage,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            str = NormalizeInput(str);
            appendage = NormalizeSubInput(appendage);

            return str.StartsWith(appendage, comparison)
                ? str
                : appendage + str;
        }

        /// <summary>
        /// Replaces a substring only if it exists.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="targetStr">The target string.</param>
        /// <param name="update">The update.</param>
        /// <param name="comparison">The comparison.</param>
        /// <returns>New File name.</returns>
        public static string ReplacePart(this string str, string targetStr, string update,
            StringComparison comparison = StringComparison.Ordinal)
        {
            str = NormalizeInput(str);

            if (string.IsNullOrEmpty(targetStr))
                return str;

            return str.Contains(targetStr, comparison)
                ? str.Replace(targetStr, update)
                : str;
        }

        /// <summary>
        /// Reorders all digits to the end and appends them with a separator.
        /// Preserves original non-digit order and old behavior.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>New File name.</returns>
        public static string ReOrderNumbers(this string str)
        {
            str = NormalizeInput(str);

            if (string.IsNullOrEmpty(str))
                return str;

            // Extract numeric parts
            var digits = string.Concat(str.Where(char.IsDigit));

            // Remove digits from the original string
            var nonDigits = string.Concat(str.Where(c => !char.IsDigit(c)));

            // Append separator + digits only if digits exist
            if (string.IsNullOrEmpty(digits))
                return str;

            return nonDigits + FileHandlerResources.Append + digits;
        }

        /// <summary>
        /// Ensures safe null handling and returns the normalized input.
        /// Does NOT modify casing, punctuation, ordering, etc.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Normalized File name.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private static string NormalizeInput(string str)
        {
            // preserve original NullException behavior for compatibility
            ArgumentNullException.ThrowIfNull(str);
            return str;
        }

        /// <summary>
        /// Normalizes append/remove/replace inputs.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Normalized File name.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        private static string NormalizeSubInput(string str)
        {
            ArgumentNullException.ThrowIfNull(str);
            return str;
        }
    }
}
