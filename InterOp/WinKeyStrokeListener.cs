/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/WinKeyStrokeListener.cs
 * PURPOSE:     General Listener for all Keystrokes.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Source:      http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/
 */

// ReSharper disable UnusedType.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace InterOp
{
    /// <inheritdoc />
    /// <summary>
    ///     Only works in window App
    ///     Manages a global low level keyboard hook
    /// </summary>
    public sealed class WinKeyStrokeListener : IDisposable
    {
        /// <summary>
        ///     Keyboard Process
        ///     Callback Function
        ///     https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644985%28v%3Dvs.85%29
        /// </summary>
        private readonly Win32Api.LowLevelKeyboardProc _proc;

        /// <summary>
        ///     Id of hook
        /// </summary>
        private IntPtr _hookId = IntPtr.Zero;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the
        ///     class and installs the keyboard hook.
        /// </summary>
        public WinKeyStrokeListener()
        {
            _proc = HookCallback;
        }

        /// <summary>
        ///     External Event, we listen to, to get the Key
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> OnKeyPressed;

        /// <summary>
        ///     Used by the Garbage Collector in case you missed it
        ///     Releases unmanaged resources and performs other cleanup operations before the
        ///     is reclaimed by garbage collection and uninstall the keyboard hook
        /// </summary>
        ~WinKeyStrokeListener()
        {
            Trace.WriteLine(InterOpResources.InformationListenerDisposed);
            _ = Win32Api.UnhookWindowsHookEx(_hookId);
        }

        /// <summary>
        ///     External Access, Start
        /// </summary>
        public void HookKeyboard()
        {
            _hookId = SetHook(_proc);
        }

        /// <summary>
        ///     External Access, Reset
        /// </summary>
        public void UnHookKeyboard()
        {
            _ = Win32Api.UnhookWindowsHookEx(_hookId);
        }

        /// <summary>
        ///     Get Process and Handle to User and Keyboard
        /// </summary>
        /// <param name="proc">Keyboard Process</param>
        /// <returns>Handle to User</returns>
        private static IntPtr SetHook(Win32Api.LowLevelKeyboardProc proc)
        {
            //Get our Process Name
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            //Pass it to our Hook
            if (curModule != null)
            {
                return Win32Api.SetWindowsHookEx(InterOpResources.WhKeyboardLl, proc,
                    Win32Api.GetModuleHandle(curModule.ModuleName),
                    0);
            }

            return new IntPtr();
        }

        /// <summary>
        ///     Check Key Stroke
        /// </summary>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam</param>
        /// <param name="lParam">The lparam</param>
        /// <returns>Handle to Process</returns>
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //allowed Key strokes checks
            if ((nCode < 0 || wParam != (IntPtr)InterOpResources.WmKeydown) &&
                wParam != (IntPtr)InterOpResources.WmSysKeyDown)
            {
                return Win32Api.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            var vkCode = Marshal.ReadInt32(lParam);

            //Keys we are looking for Invoke Event
            OnKeyPressed?.Invoke(this, new KeyPressedEventArgs(KeyInterop.KeyFromVirtualKey(vkCode)));

            //not interested, next
            return Win32Api.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <inheritdoc />
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed resources (if any)
            }

            UnHookKeyboard();
            _disposed = true;
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Key Object
    /// </summary>
    public sealed class KeyPressedEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:InterOp.KeyPressedEventArgs" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        internal KeyPressedEventArgs(Key key)
        {
            KeyPressed = key;
        }

        /// <summary>
        ///     Gets the key pressed.
        /// </summary>
        public Key KeyPressed { get; }
    }
}
