﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreHelper.cs
 * PURPOSE:     Helper File that shares logic in the Project
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Linq;

namespace CoreBuilder;

/// <summary>
///     Static Helper class
/// </summary>
internal static class CoreHelper
{
    /// <summary>
    ///     Files that should be ignored.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>Checks if file should be ignored.</returns>
    internal static bool ShouldIgnoreFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        // Check filename-based exclusions
        if (fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check for <auto-generated> comment in the first few lines
        try
        {
            if (File.ReadLines(filePath).Take(10).Any(line =>
                    line.Contains("<auto-generated", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }
        catch
        {
            return true; // If unreadable, best to skip
        }

        return false;
    }
}
