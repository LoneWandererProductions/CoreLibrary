﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DbInfoViewModel.cs
 * PURPOSE:     DbInfo View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.ComponentModel;

namespace SQLiteGui
{
    /// <summary>
    /// View Model for DbInfo
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DbInfoViewModel : INotifyPropertyChanged
    {
        private string _infoText;

        /// <summary>
        /// Gets or sets the information text.
        /// </summary>
        /// <value>
        /// The information text.
        /// </value>
        public string InfoText
        {
            get => _infoText;
            set
            {
                if (_infoText != value)
                {
                    _infoText = value;
                    OnPropertyChanged(nameof(InfoText));
                }
            }
        }

        /// <summary>
        /// Appends the information.
        /// </summary>
        /// <param name="message">The message.</param>
        public void AppendInfo(string message)
        {
            InfoText += message + Environment.NewLine;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
