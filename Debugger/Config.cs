/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/Config.cs
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
    public class Config : ObservableObject
    {
        /// <summary>
        ///     Gets or sets the debug path.
        /// </summary>
        public string DebugPath
        {
            get => DebugRegister.DebugPath;
            set
            {
                DebugRegister.DebugPath = value;
                RaisePropertyChangedEvent(nameof(DebugPath));
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
                DebugRegister.SecondsTick = value;
                RaisePropertyChangedEvent(nameof(SecondsTick));
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
                DebugRegister.MinutesTick = value;
                RaisePropertyChangedEvent(nameof(MinutesTick));
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
                DebugRegister.HourTick = value;
                RaisePropertyChangedEvent(nameof(HourTick));
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
                DebugRegister.IsDumpActive = value;
                RaisePropertyChangedEvent(nameof(IsDumpActive));
            }
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
                DebugRegister.SuppressWindow = value;
                RaisePropertyChangedEvent(nameof(SuppressWindow));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether Debugger Is Running
        /// </summary>
        public bool IsRunning
        {
            get => DebugRegister.IsRunning;
            init
            {
                DebugRegister.IsRunning = value;
                RaisePropertyChangedEvent(nameof(IsRunning));
            }
        }

        /// <summary>
        ///     Gets or sets the error color.
        /// </summary>
        public string ErrorColor
        {
            get => DebugRegister.ErrorColor;
            set => DebugRegister.ErrorColor = value;
        }

        /// <summary>
        ///     Gets or sets the warning color.
        /// </summary>
        public string WarningColor
        {
            get => DebugRegister.WarningColor;
            set
            {
                DebugRegister.WarningColor = value;
                RaisePropertyChangedEvent(nameof(WarningColor));
            }
        }

        /// <summary>
        ///     Gets or sets the information color.
        /// </summary>
        public string InformationColor
        {
            get => DebugRegister.InformationColor;
            set => DebugRegister.InformationColor = value;
        }

        /// <summary>
        ///     Gets or sets the external color.
        /// </summary>
        public string ExternalColor
        {
            get => DebugRegister.ExternalColor;
            set => DebugRegister.ExternalColor = value;
        }

        /// <summary>
        ///     Gets or sets the standard color.
        /// </summary>
        public string StandardColor
        {
            get => DebugRegister.StandardColor;
            set => DebugRegister.StandardColor = value;
        }

        /// <summary>
        ///     Gets or sets the color options.
        /// </summary>
        /// <value>
        ///     The color options.
        /// </value>
        public List<ColorOption> ColorOptions
        {
            get => DebugRegister.ColorOptions;
            set => DebugRegister.ColorOptions = value;
        }

        /// <summary>
        ///     Maximum size for each log file in bytes.
        /// </summary>
        public long MaxFileSize { get; set; } = 5 * 1024 * 1024; // Default: 5 MB

        /// <summary>
        ///     Maximum number of log files to retain.
        /// </summary>
        public int MaxFileCount { get; set; } = 10; // Default: 10
    }
}
