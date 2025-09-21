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
using Interpreter.Resources;

namespace Interpreter;

/// <summary>
///     Mostly handles Batch Files for now in the future it will do more stuff
/// </summary>
internal static class IrtHelper
{
    /// <summary>
    ///     Reads a .batch file
    /// </summary>
    /// <param name="filepath">path of the .txt file</param>
    /// <returns>the values as String[]. Can return null.</returns>
    internal static string ReadBatchFile(string filepath)
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


    /// <summary>
    ///     Checks the input.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="feedbackOptions">The feedback options.</param>
    /// <returns>Id of the option or the fitting error</returns>
    internal static int CheckInput(string input, Dictionary<AvailableFeedback, string> feedbackOptions)
    {
        input = input.Trim().ToUpper();

        // First try parsing as enum name
        if (Enum.TryParse(input, true, out AvailableFeedback parsedFeedback))
        {
            if (feedbackOptions.ContainsKey(parsedFeedback))
                return (int)parsedFeedback;

            return -2; // parsed enum, but not in allowed options
        }

        // Otherwise, match against dictionary values
        foreach (var kvp in feedbackOptions)
        {
            if (kvp.Value.Trim().Equals(input, StringComparison.CurrentCultureIgnoreCase))
                return (int)kvp.Key;
        }

        return -1; // not an enum name and not a value match
    }
}
