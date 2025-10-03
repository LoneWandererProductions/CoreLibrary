/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonControls
* FILE:        CommonControls/Win32Api.cs
* PURPOSE:     Direct Invocations to the Operating System
* PROGRAMER:   Peter Geinitz (Wayfarer)
* SOURCES:     https://www.pinvoke.net/default.aspx
*              https://docs.microsoft.com/de-de/dotnet/framework/interop/specifying-a-character-set
*              http://maruf-dotnetdeveloper.blogspot.com/2012/08/c-refreshing-system-tray-icon.html
*/

using System;
using System.Runtime.InteropServices;

namespace InterOp;

/// <summary>
///     Win32 API imports.
///     Functions and Callback Functions
/// </summary>
internal static partial class Win32Api
{
    /// <summary>
    ///     Unhooks the windows hook.
    ///     Source: https://www.pinvoke.net/default.aspx/user32/UnhookWindowsHookEx.html
    ///     Used in Keystroke
    /// </summary>
    /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
    /// <returns>
    ///     True if successful, false otherwise
    /// </returns>
    [LibraryImport(InterOpResources.UserDll, EntryPoint = InterOpResources.FunctionUnhookWindowsHookEx, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnhookWindowsHookEx(IntPtr hInstance);

    /// <summary>
    ///     Loads the library
    ///     Used to Load the Methods in User 32
    ///     Used in Keystroke
    /// </summary>
    /// <param name="lpModuleName">Name of the library</param>
    /// <returns>
    ///     A handle to the library
    /// </returns>
    [LibraryImport(InterOpResources.KernelDll, EntryPoint = InterOpResources.FunctionGetModuleHandle, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    internal static partial IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>
    ///     Calls the next hook.
    ///     Used in Keystroke
    /// </summary>
    /// <param name="idHook">The hook id</param>
    /// <param name="nCode">The hook code</param>
    /// <param name="wParam">The wParam</param>
    /// <param name="lParam">The lParam</param>
    /// <returns>
    ///     A handle to the library
    /// </returns>
    [LibraryImport(InterOpResources.UserDll, EntryPoint = InterOpResources.FunctionCallNextHookEx, SetLastError = true)]
    internal static partial IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

    /// <summary>
    ///     Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
    ///     Used in Keystroke
    /// </summary>
    /// <param name="idHook">The id of the event you want to hook</param>
    /// <param name="callback">The callback.</param>
    /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
    /// <param name="threadId">The thread you want to attach the event to, can be null</param>
    /// <returns>
    ///     a handle to the desired hook
    /// </returns>
    [LibraryImport(InterOpResources.UserDll, EntryPoint = InterOpResources.FunctionSetWindowsHookEx, SetLastError = true)]
    internal static partial IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance,
        uint threadId);

    /// <summary>
    ///     Get Current Location of cursor on Screen
    ///     Used in Mouse Position
    /// </summary>
    /// <param name="pt">Pointer to </param>
    /// <returns>Mouse Position</returns>
    [LibraryImport(InterOpResources.UserDll, EntryPoint = InterOpResources.FunctionGetPhysicalCursorPos, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetPhysicalCursorPos(ref Win32Points pt);

    /// <summary>
    ///     Header: Winuser.h
    ///     Callback Function
    ///     Register Keystroke
    ///     ANSI A/UNICODE W Variants
    ///     Source: https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985%28v%3Dvs.85%29
    ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexa
    ///     Source: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowshookexw
    /// </summary>
    /// <param name="nCode">The hook code</param>
    /// <param name="wParam">The wParam</param>
    /// <param name="lParam">The lParam</param>
    /// <returns>
    ///     Key was stroked
    /// </returns>
    internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
}
