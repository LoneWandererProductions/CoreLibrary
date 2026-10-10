/*
 * PROJECT:     DataFormatter
 * FILE:        DataFormatter/WriterObj.cs
 * PURPOSE:     A basic obj File writer to match ReaderObj.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System.IO;
using System.Text;

namespace DataFormatter
{
    /// <summary>
    /// Basic implementation to write ObjFile objects to disk.
    /// </summary>
    public static class WriterObj
    {
        /// <summary>
        /// Writes an ObjFile to the specified path.
        /// </summary>
        /// <param name="objData">The object data.</param>
        /// <param name="filePath">The file path.</param>
        public static void WriteObj(ObjFile? objData, string filePath)
        {
            if (objData == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("# Exported via DataFormatter");

            if (objData.Vectors != null)
            {
                foreach (var v in objData.Vectors)
                {
                    // Invert culture formatting if necessary based on your DataHelper
                    sb.AppendLine($"v {v.X} {v.Y} {v.Z}");
                }
            }

            if (objData.Face != null)
            {
                foreach (var f in objData.Face)
                {
                    sb.AppendLine($"f {f.X} {f.Y} {f.Z}");
                }
            }

            if (objData.Other != null)
            {
                foreach (var other in objData.Other)
                {
                    sb.AppendLine(other);
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
