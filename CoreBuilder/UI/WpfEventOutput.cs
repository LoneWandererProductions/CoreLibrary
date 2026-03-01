/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.UI
 * FILE:        WpfEventOutput.cs
 * PURPOSE:     Sample wpf side channel for event output.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Interface;

namespace CoreBuilder.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Logging output that displays messages in a WPF window.
    /// </summary>
    /// <seealso cref="CoreBuilder.Interface.IEventOutput" />
    public sealed class WpfEventOutput : IEventOutput
    {
        /// <summary>
        /// The window
        /// </summary>
        private LogWindow? _window;

        /// <summary>
        /// The lock
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfEventOutput"/> class.
        /// </summary>
        public WpfEventOutput()
        {
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Write(string message)
        {
            EnsureWindow();
            _window!.Dispatcher.BeginInvoke(() => _window.Append(message));
        }

        /// <summary>
        /// Ensures the window.
        /// Needed in console Context.
        /// </summary>
        private void EnsureWindow()
        {
            if (_window != null) return;

            lock (_lock)
            {
                if (_window != null) return;

                var tcs = new System.Threading.Tasks.TaskCompletionSource<LogWindow>();

                var thread = new System.Threading.Thread(() =>
                {
                    var w = new LogWindow();
                    w.Show();
                    _window = w;
                    tcs.SetResult(w);
                    System.Windows.Threading.Dispatcher.Run(); // Start message loop
                });

                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();

                _window = tcs.Task.Result;
            }
        }
    }
}
