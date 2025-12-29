/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/WinScreenCoordinate.cs
 * PURPOSE:     Get Window Position on Screen
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global, for fucks Sake it is a Dll
// ReSharper disable UnusedType.Global

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace InterOp
{
    /// <summary>
    ///     The screen coordinate class.
    /// </summary>
    public static class WinScreenCoordinate
    {
        /// <summary>
        ///     Get internal System Point to c# Struct
        /// </summary>
        /// <value>
        ///     The mouse position.
        /// </value>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static Point MousePosition
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new PlatformNotSupportedException();
                }

                var w32Mouse = new Win32Points();
                _ = Win32Api.GetPhysicalCursorPos(ref w32Mouse);
                return new Point(w32Mouse.X, w32Mouse.Y);
            }
        }
    }
}
