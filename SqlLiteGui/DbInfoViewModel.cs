/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        DbInfoViewModel.cs
 * PURPOSE:     DbInfo View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using ViewModel;

namespace SQLiteGui
{
    /// <inheritdoc />
    /// <summary>
    ///     View Model for DbInfo
    /// </summary>
    /// <seealso cref="T:ViewModel.ViewModelBase" />
    public sealed class DbInfoViewModel : ViewModelBase
    {
        /// <summary>
        /// The information text
        /// </summary>
        private string? _infoText;

        /// <summary>
        ///     Gets or sets the information text.
        /// </summary>
        /// <value>
        ///     The information text.
        /// </value>
        public string? InfoText
        {
            get => _infoText;
            set
            {
                if (_infoText == value)
                {
                    return;
                }

                _infoText = value;
                OnPropertyChanged(nameof(InfoText));
            }
        }

        /// <summary>
        ///     Appends the information.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void AppendInfo(string message)
        {
            InfoText += message + Environment.NewLine;
        }
    }
}
