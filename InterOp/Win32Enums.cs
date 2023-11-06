/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Win32Enums.cs
 * PURPOSE:     General Purpose Enums Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.pinvoke.net/default.aspx
 *              https://docs.microsoft.com/de-de/dotnet/framework/interop/specifying-a-character-set
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal, might be of use for other Applications

namespace InterOp
{
    /// <summary>
    ///     The win32 Enums class.
    /// </summary>
    public static class Win32Enums
    {
        /// <summary>
        ///     Basic Mouse Parameters in Hexadecimal
        ///     https://wiki.winehq.org/List_Of_Windows_Messages
        /// </summary>
        public enum MouseEvents
        {
            /// <summary>
            ///     Mouse Move, 0x200
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove
            /// </summary>
            WmMousemove = 512,

            /// <summary>
            ///     Left mouse button down, 0x201
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondown
            /// </summary>
            WmLbuttondown = 513,

            /// <summary>
            ///     Left mouse button up, 0x202
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttonup
            /// </summary>
            WmLbuttonup = 514,

            /// <summary>
            ///     Left mouse button double click, 0x203
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-lbuttondblclk
            /// </summary>
            WmLbuttondblclk = 515,

            /// <summary>
            ///     Right mouse button down, 0x204
            /// </summary>
            WmRbuttondown = 516,

            /// <summary>
            ///     Right mouse button up, 0x205
            /// </summary>
            WmRbuttonup = 517,

            /// <summary>
            ///     Right mouse button double click, 0x206
            /// </summary>
            WmRbuttondblclk = 518,

            /// <summary>
            ///     Middle mouse button down, 0x207
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttondown
            /// </summary>
            WmMbuttondown = 519,

            /// <summary>
            ///     Middle mouse button up, 0x0208
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttonup
            /// </summary>
            WmMbuttonup = 520,

            /// <summary>
            ///     Middle mouse button double click, 0x209
            ///     https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mbuttondblclk
            /// </summary>
            WmLmuttondblclk = 521,

            /// <summary>
            ///     The balloon tool tip changed true, 0x402
            /// </summary>
            BalloonToolTipChanged = 1026,

            /// <summary>
            ///     The balloon tool tip changed true, 0x403
            /// </summary>
            BalloonToolTipChangedTrue = 1027,

            /// <summary>
            ///     The balloon tool tip changed false, 0x404
            /// </summary>
            BalloonToolTipChangedFalse = 1028,

            /// <summary>
            ///     The balloon tool tip clicked, 0x405
            /// </summary>
            BalloonToolTipClicked = 1029
        }
    }
}
