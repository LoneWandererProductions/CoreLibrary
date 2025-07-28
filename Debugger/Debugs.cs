/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/Debugs.cs
 * PURPOSE:     Handle the whole Interaction static for ease of use messages
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Collections.Generic;

namespace Debugger;

/// <summary>
///     Entry for static Debugging
/// </summary>
public static class Debugs
{
    /// <summary>
    ///     Single instance of DebugLog
    /// </summary>
    private static readonly DebugLog DebugLog = new();

    /// <summary>
    ///     Initializes the <see cref="Debugs" /> class.
    /// </summary>
    static Debugs()
    {
        DebugLog.Start();
    }

    /// <summary>
    ///     Start debugging with a window.
    ///     Completely optional. Activate to see the Debug Window
    /// </summary>
    public static void StartWindow()
    {
        DebugLog.StartWindow();
    }

    public static void CloseWindow()
    {
        DebugLog.CloseWindow();
    }


    /// <summary>
    ///     Stops the debugging.
    /// </summary>
    public static void StopDebugging()
    {
        DebugLog.StopDebugging();
        DebugLog.CloseWindow();
    }

    /// <summary>
    ///     Creates the dump.
    ///     Writes all log entries of the current session into the Log no mather the Debug Lvl.
    /// </summary>
    public static void CreateDump()
    {
        DebugLog.CreateDump();
    }

    /// <summary>
    ///     Logs the message to the Debug file.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="lvl">The level.</param>
    public static void LogFile(string error, ErCode lvl)
    {
        DebugLog.LogFile(error, lvl, 2);
    }

    /// <summary>
    ///     Logs the message to the Debug file with an object
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="lvl">The level.</param>
    /// <param name="obj">The object.</param>
    public static void LogFile<T>(string error, ErCode lvl, T obj)
    {
        DebugLog.LogFile(error, lvl, obj, 2);
    }

    /// <summary>
    ///     Logs the message to the Debug file with a list of objects.
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="lvl">The level.</param>
    /// <param name="objLst">The object LST.</param>
    public static void LogFile<T>(string error, ErCode lvl, IEnumerable<T> objLst)
    {
        DebugLog.LogFile(error, lvl, objLst, 2);
    }

    /// <summary>
    ///     Logs the file with a dictionary of objects.
    /// </summary>
    /// <typeparam name="T">Generic Type</typeparam>
    /// <typeparam name="TU">The type of the u.</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="lvl">The level.</param>
    /// <param name="objectDictionary">The object dictionary.</param>
    public static void LogFile<T, TU>(string error, ErCode lvl, Dictionary<T, TU> objectDictionary)
    {
        DebugLog.LogFile(error, lvl, objectDictionary, 2);
    }

    /// <summary>
    ///     Deletes the log file.
    ///     Optional, but if you want to clean up behind you.
    /// </summary>
    public static void DeleteLogFile()
    {
        DebugLog.Delete();
    }
}
