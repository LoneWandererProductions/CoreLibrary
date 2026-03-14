/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataHelper.cs
 * PURPOSE:     Basic stuff shared over all operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataFormatter
{
    /// <summary>
    ///     Basic helper Operations
    /// </summary>
    internal static class DataHelper
    {
        /// <summary>
        ///     Gets the parts.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param separator="separator">The splitter for the csv file.</param>
        /// <param name="separator">the splitter used in the csv</param>
        /// <returns>split Parts</returns>
        internal static List<string> GetParts(string str, char separator)
        {
            if (string.IsNullOrEmpty(str)) return new List<string>();

            return str.Split(separator).ToList();
        }

        /// <summary>
        ///     Helper function to detect the encoding of a file based on BOM
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Type of Encoding</returns>
        internal static Encoding GetFileEncoding(string filePath)
        {
            // Use FileShare.Read to avoid locking the file unnecessarily
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var buffer = new byte[5];
            var bytesRead = fs.Read(buffer, 0, 5);

            if (bytesRead < 2)
            {
                return Encoding.Default;
            }

            // Check for UTF-32 LE (FF FE 00 00) - MUST be checked before UTF-16 LE
            if (bytesRead >= 4 && buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0x00 && buffer[3] == 0x00)
            {
                return Encoding.UTF32;
            }

            // Check for UTF-32 BE (00 00 FE FF) - MUST be checked before UTF-16 BE (rare collision, but safe practice)
            if (bytesRead >= 4 && buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xfe && buffer[3] == 0xff)
            {
                // Assuming "utf-32BE" is the resource string, or use explicit encoding:
                return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
            }

            // Now check for the shorter BOMs
            switch (buffer[0])
            {
                // UTF-8 (EF BB BF)
                case 0xef when bytesRead >= 3 && buffer[1] == 0xbb && buffer[2] == 0xbf:
                    return Encoding.UTF8;

                // UTF-16 LE (FF FE)
                case 0xff when buffer[1] == 0xfe:
                    return Encoding.Unicode;

                // UTF-16 BE (FE FF)
                case 0xfe when buffer[1] == 0xff:
                    return Encoding.BigEndianUnicode;

                default:
                    return Encoding.Default;
            }
        }
    }
}
