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
        private readonly LogWindow _window;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfEventOutput"/> class.
        /// </summary>
        public WpfEventOutput()
        {
            _window = new LogWindow();
            _window.Show();
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Write(string message)
        {
            _window.Append(message);
        }
    }
}
