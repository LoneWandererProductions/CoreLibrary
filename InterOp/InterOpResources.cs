/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterOp
 * FILE:        InterOp/InterOpResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace InterOp
{
    /// <summary>
    ///     The com Control resources class.
    /// </summary>
    internal static class InterOpResources
    {
        /// <summary>
        ///     The user dll (const). Value: "user32.dll".
        /// </summary>
        internal const string UserDll = "user32.dll";

        /// <summary>
        ///     The kernel dll (const). Value: "kernel32.dll".
        /// </summary>
        internal const string KernelDll = "kernel32.dll";

        /// <summary>
        ///     Header: Winuser.h
        ///     The function SetWindowsHookEx (const). Value: "SetWindowsHookEx".
        ///     ANSI A/UNICODE W Variants
        ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
        ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw
        /// </summary>
        internal const string FunctionSetWindowsHookEx = "SetWindowsHookEx";

        /// <summary>
        ///     Header: Winuser.h
        ///     The function UnhookWindowsHookEx (const). Value: "UnhookWindowsHookEx".
        ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unhookwindowshookex
        /// </summary>
        internal const string FunctionUnhookWindowsHookEx = "UnhookWindowsHookEx";

        /// <summary>
        ///     Header: Libloaderapi.h
        ///     The function GetModuleHandle (const). Value: "GetModuleHandle".
        ///     ANSI A
        ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-getmodulehandlea
        /// </summary>
        internal const string FunctionGetModuleHandle = "GetModuleHandle";

        /// <summary>
        ///     Header: Winuser.h
        ///     The function FunctionGetPhysicalCursorPos (const). Value: "GetPhysicalCursorPos".
        ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getphysicalcursorpos
        /// </summary>
        internal const string FunctionGetPhysicalCursorPos = "GetPhysicalCursorPos";

        /// <summary>
        ///     Header: Winuser.h
        ///     The function CallNextHookEx (const). Value: "CallNextHookEx".
        ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-callnexthookex
        /// </summary>
        internal const string FunctionCallNextHookEx = "CallNextHookEx";

        /// <summary>
        ///     The information listener disposed (const). Value: "KeyStrokeListener's finalize is called.".
        /// </summary>
        internal const string InformationListenerDisposed = "KeyStrokeListener's finalize is called.";

        /// <summary>
        ///     magic Number
        ///     The wh keyboard ll (const). Value: 13.
        /// </summary>
        internal const int WhKeyboardLl = 13;

        /// <summary>
        ///     magic Number
        ///     The wm keydown (const). Value: 0x0100.
        /// </summary>
        internal const int WmKeydown = 0x0100;

        /// <summary>
        ///     magic Number
        ///     The wm sys key down (const). Value: 0x0104.
        /// </summary>
        internal const int WmSysKeyDown = 0x0104;

        /// <summary>
        ///     Registry Key error (const). Value: "Error: Empty Registry Key Exception".
        /// </summary>
        internal const string KeyError = "Error: Empty Registry Key Exception";
    }
}
