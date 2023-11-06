/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/HelperMethods.cs
 * PURPOSE:    Helper Methods
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;

namespace CommonLibraryTests
{
    /// <summary>
    ///     The helper methods class.
    /// </summary>
    internal static class HelperMethods
    {
        /// <summary>
        ///     Creates some Dummy Files we will delete
        /// </summary>
        /// <param name="path">target Path</param>
        /// <param name="fileExtList">Extension List</param>
        internal static void CreateFiles(string path, IEnumerable<string> fileExtList)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = 0;

            foreach (var ext in fileExtList)
            {
                fileName++;

                var file = path + Path.DirectorySeparatorChar + fileName + ext;

                if (File.Exists(file))
                {
                    continue;
                }

                using var fs = File.Create(file);
                for (byte i = 0; i < 10; i++)
                {
                    fs.WriteByte(i);
                }
            }
        }

        /// <summary>
        ///     Create a Dummy Files we will delete
        /// </summary>
        /// <param name="path">target Path</param>
        internal static void CreateFile(string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (folder == null)
            {
                return;
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (File.Exists(path))
            {
                return;
            }

            using var fs = File.Create(path);
            for (byte i = 0; i < 100; i++)
            {
                fs.WriteByte(i);
            }
        }
    }
}
