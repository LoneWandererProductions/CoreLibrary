/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Config.cs
 * PURPOSE:     Config Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global, Config Class
// ReSharper disable MemberCanBeMadeStatic.Global, Config Class
// ReSharper disable MemberCanBeInternal, no just no, else we get problems with the Serializer

using System.Collections.Generic;
using ViewModel;

namespace Debugger
{
    /// <inheritdoc />
    /// <summary>
    ///     The config class.
    ///     Only used for Data that is permanent and saved
    /// </summary>
    public class Config : ViewModelBase
    {
        private long _maxFileSize = 5 * 1024 * 1024; // Default: 5 MB
        private int _maxFileCount = 10; // Default: 10

        /// <summary>
        ///     Gets or sets the debug path.
        /// </summary>
        public string DebugPath
        {
            get => DebugRegister.DebugPath;
            set
            {
                if (DebugRegister.DebugPath == value) return;

                DebugRegister.DebugPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the seconds tick.
        /// </summary>
        public int SecondsTick
        {
            get => DebugRegister.SecondsTick;
            set
            {
                if (DebugRegister.SecondsTick == value) return;

                DebugRegister.SecondsTick = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the minutes tick.
        /// </summary>
        public int MinutesTick
        {
            get => DebugRegister.MinutesTick;
            set
            {
                if (DebugRegister.MinutesTick == value) return;

                DebugRegister.MinutesTick = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the hour tick.
        /// </summary>
        public int HourTick
        {
            get => DebugRegister.HourTick;
            set
            {
                if (DebugRegister.HourTick == value) return;

                DebugRegister.HourTick = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Dump is Active
        /// </summary>
        public bool IsDumpActive
        {
            get => DebugRegister.IsDumpActive;
            set
            {
                if (DebugRegister.IsDumpActive == value) return;

                DebugRegister.IsDumpActive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is verbose.
        /// </summary>
        public bool IsVerbose
        {
            get => DebugRegister.IsVerbose;
            set
            {
                if (DebugRegister.IsVerbose == value) return;

                DebugRegister.IsVerbose = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Maximum size for each log file in bytes.
        /// </summary>
        public long MaxFileSize
        {
            get => _maxFileSize;
            set => SetProperty(ref _maxFileSize, value);
        }

        /// <summary>
        ///     Maximum number of log files to retain.
        /// </summary>
        public int MaxFileCount
        {
            get => _maxFileCount;
            set => SetProperty(ref _maxFileCount, value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     The config extended class.
    ///     Only shows and saves Data that is changed at Runtime
    /// </summary>
    public sealed class ConfigExtended : Config
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the Window is displayed or not
        /// </summary>
        public bool SuppressWindow
        {
            get => DebugRegister.SuppressWindow;
            set
            {
                if (DebugRegister.SuppressWindow == value) return;

                DebugRegister.SuppressWindow = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Debugger Is Running
        /// </summary>
        public bool IsRunning
        {
            get => DebugRegister.IsRunning;
            set
            {
                if (DebugRegister.IsRunning == value) return;

                DebugRegister.IsRunning = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the error color.
        /// </summary>
        public string ErrorColor
        {
            get => DebugRegister.ErrorColor;
            set
            {
                if (DebugRegister.ErrorColor == value) return;

                DebugRegister.ErrorColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the warning color.
        /// </summary>
        public string WarningColor
        {
            get => DebugRegister.WarningColor;
            set
            {
                if (DebugRegister.WarningColor == value) return;

                DebugRegister.WarningColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the information color.
        /// </summary>
        public string InformationColor
        {
            get => DebugRegister.InformationColor;
            set
            {
                if (DebugRegister.InformationColor == value) return;

                DebugRegister.InformationColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the external color.
        /// </summary>
        public string ExternalColor
        {
            get => DebugRegister.ExternalColor;
            set
            {
                if (DebugRegister.ExternalColor == value) return;

                DebugRegister.ExternalColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the standard color.
        /// </summary>
        public string StandardColor
        {
            get => DebugRegister.StandardColor;
            set
            {
                if (DebugRegister.StandardColor == value) return;

                DebugRegister.StandardColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets the color options.
        /// </summary>
        public List<ColorOption> ColorOptions
        {
            get => DebugRegister.ColorOptions;
            set
            {
                if (DebugRegister.ColorOptions == value) return;

                DebugRegister.ColorOptions = value;
                OnPropertyChanged();
            }
        }
    }
}
