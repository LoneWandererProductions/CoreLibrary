/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Win32Structs.cs
 * PURPOSE:     General Purpose Structs Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.pinvoke.net/default.aspx
 *              https://docs.microsoft.com/de-de/dotnet/framework/interop/specifying-a-character-set
 *              http://maruf-dotnetdeveloper.blogspot.com/2012/08/c-refreshing-system-tray-icon.html
 */

using System.Runtime.InteropServices;

namespace InterOp
{
    /// <summary>
    ///     Coordinate Struct
    ///     Window Format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct Win32Points
    {
        /// <summary>
        ///     The x Point (readonly).
        /// </summary>
        public readonly int X;

        /// <summary>
        ///     The y Point(readonly).
        /// </summary>
        public readonly int Y;
    }
}
