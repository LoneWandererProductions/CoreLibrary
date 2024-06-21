using System;
/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/EncodingHelper.cs
 * PURPOSE:     Basic stuff for Encodin.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;
using System.Text;

namespace DataFormatter
{
    /// <summary>
    /// Internal Helper for Encodings
    /// </summary>
    internal static class EncodingHelper
    {
        /// <summary>
        /// Helper function to detect the encoding of a file based on BOM
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Type of Encoding</returns>
        internal static Encoding GetFileEncoding(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // Read the first few bytes to detect the encoding
            byte[] buffer = new byte[5];
            fs.Read(buffer, 0, 5);

            // Check for UTF-8 BOM
            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
            {
                return Encoding.UTF8;
            }
            // Check for UTF-16 LE BOM
            else if (buffer[0] == 0xff && buffer[1] == 0xfe)
            {
                return Encoding.Unicode; // UTF-16 LE
            }
            // Check for UTF-16 BE BOM
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
            {
                return Encoding.BigEndianUnicode; // UTF-16 BE
            }
            // Check for UTF-32 LE BOM
            else if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0x00 && buffer[3] == 0x00)
            {
                return Encoding.UTF32; // UTF-32 LE
            }
            // Check for UTF-32 BE BOM
            else if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xfe && buffer[3] == 0xff)
            {
                return Encoding.GetEncoding(DataFormatterResources.EncodingUTF32); // UTF-32 BE
            }
            else
            {
                // Default encoding (change this as per your requirements)
                return Encoding.Default; // Default ANSI code page
            }
        }
    }
}
