/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/IrtHelper.cs
 * PURPOSE:     Stuff that does not belong into the Interpreter and actual does stuff on the Host, so handle with care.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Interpreter
{
    internal static class IrtHelper
    {
        /// <summary>
        ///     Reads a .batch file
        /// </summary>
        /// <param name="filepath">path of the .txt file</param>
        /// <returns>the values as String[]. Can return null.</returns>
        public static string ReadBatchFile(string filepath)
        {
            var parts = new List<string>();
            try
            {
                using var reader = new StreamReader(filepath);
                while (reader.ReadLine() is { } line)
                {
                    parts.Add(line);
                }
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex);
                return string.Empty;
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
                return string.Empty;
            }

            return parts.Aggregate(string.Empty, string.Concat);
        }
    }
}
