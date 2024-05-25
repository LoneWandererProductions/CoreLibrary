/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Debugger
* FILE:        Debugger/DebugRegister.cs
* PURPOSE:     Handle the internal Config files
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Debugger
{
    /// <summary>
    ///     The debug register class.
    /// </summary>
    public static class DebugRegister
    {
        /// <summary>
        ///     Get the Path to the Debug File
        /// </summary>
        private static readonly string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(),
            DebuggerResources.ConfigFile);

        /// <summary>
        ///     The base options
        /// </summary>
        private static readonly ConfigExtended BaseOptions = new()
        {
            DebugPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.FileName),
            SecondsTick = 1,
            MinutesTick = 0,
            HourTick = 0,
            ErrorColor = DebuggerResources.ErrorColor,
            InformationColor = DebuggerResources.InformationColor,
            ExternalColor = DebuggerResources.ExternalColor,
            StandardColor = DebuggerResources.StandardColor,
            WarningColor = DebuggerResources.WarningColor,
            IsDumpActive = false,
            ColorOptions = DebuggerResources.InitialOptions
        };

        /// <summary>
        ///     Gets or sets a value indicating whether the Debugger is Running
        /// </summary>
        public static bool IsRunning { get; internal set; }

        /// <summary>
        ///     Get the Path to the Debug File
        /// </summary>
        internal static string DebugPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            DebuggerResources.FileName);

        /// <summary>
        ///     Is dump active.
        /// </summary>
        internal static bool IsDumpActive { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we want to show the window
        /// </summary>
        internal static bool SuppressWindow { get; set; }

        /// <summary>
        ///     Gets or sets the seconds tick.
        /// </summary>
        internal static int SecondsTick { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the seconds tick.
        /// </summary>
        internal static int HourTick { get; set; }

        /// <summary>
        ///     Gets or sets the seconds tick.
        /// </summary>
        internal static int MinutesTick { get; set; }

        /// <summary>
        ///     Gets or sets the error color.
        /// </summary>
        internal static string ErrorColor { get; set; } = DebuggerResources.ErrorColor;

        /// <summary>
        ///     Gets or sets the warning color.
        /// </summary>
        internal static string WarningColor { get; set; } = DebuggerResources.WarningColor;

        /// <summary>
        ///     Gets or sets the information color.
        /// </summary>
        internal static string InformationColor { get; set; } = DebuggerResources.InformationColor;

        /// <summary>
        ///     Gets or sets the external color.
        /// </summary>
        internal static string ExternalColor { get; set; } = DebuggerResources.ExternalColor;

        /// <summary>
        ///     Gets or sets the standard color.
        /// </summary>
        internal static string StandardColor { get; set; } = DebuggerResources.StandardColor;

        /// <summary>
        ///     Gets or sets the color of the found item in the line
        /// </summary>
        /// <value>
        ///     The color of the found.
        /// </value>
        internal static string FoundColor { get; set; } = DebuggerResources.FoundColor;

        /// <summary>
        ///     Gets or sets the config.
        ///     This object will be saved in the file
        /// </summary>
        internal static ConfigExtended Config { get; private set; }

        /// <summary>
        ///     Gets the color options.
        /// </summary>
        /// <value>
        ///     The color options.
        /// </value>
        internal static List<ColorOption> ColorOptions { get; set; } = DebuggerResources.InitialOptions;

        /// <summary>
        ///     Resets this instance.
        /// </summary>
        internal static void Reset()
        {
            Config = BaseOptions;
        }

        /// <summary>
        ///     Read the config file and prepare the file, if it does not exist, create a new config and prepare it for saving.
        /// </summary>
        internal static void ReadConfigFile()
        {
            if (!File.Exists(DebuggerResources.ConfigFile))
            {
                Config = BaseOptions;
            }
            else
            {
                var config = SerializeConfig();
                if (config == null)
                {
                    return;
                }

                //set file we want to read
                //generic data in config
                DebugPath = config.DebugPath;
                DebugPath = config.DebugPath;
                SecondsTick = config.SecondsTick;
                MinutesTick = config.MinutesTick;
                HourTick = config.HourTick;
                IsDumpActive = config.IsDumpActive;

                Config = new ConfigExtended
                {
                    DebugPath = config.DebugPath,
                    //not saved in config
                    IsRunning = IsRunning,
                    SecondsTick = config.SecondsTick,
                    MinutesTick = config.MinutesTick,
                    HourTick = config.HourTick,
                    ErrorColor = config.ErrorColor,
                    InformationColor = config.InformationColor,
                    ExternalColor = config.ExternalColor,
                    StandardColor = config.StandardColor,
                    WarningColor = config.WarningColor,
                    ColorOptions = ColorOptions
                };
            }
        }

        /// <summary>
        ///     The serialize config.
        /// </summary>
        /// <returns>The <see cref="Config" />.</returns>
        [return: MaybeNull]
        private static ConfigExtended SerializeConfig()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ConfigExtended));
                using Stream tr = File.OpenRead(ConfigPath);
                return serializer.Deserialize(tr) as ConfigExtended;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Trace.WriteLine(ex);
            }

            return null;
        }

        /// <summary>
        ///     Generic Serializer Of Objects
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="serializeObject">Target Object</param>
        /// <param name="options">The options.</param>
        internal static void XmlSerializerObject<T>(T serializeObject, List<ColorOption> options)
        {
            //check if everything is in place
            if (serializeObject is not ConfigExtended data || options == null)
            {
                return;
            }

            ColorOptions = options;

            //Add our Colors a bit of a hack but works for now
            data.ErrorColor = ErrorColor;
            data.WarningColor = WarningColor;
            data.InformationColor = InformationColor;
            data.ExternalColor = ExternalColor;
            data.StandardColor = StandardColor;
            data.ColorOptions = ColorOptions;

            try
            {
                var serializer = new XmlSerializer(data.GetType());
                using var tr =
                    new StreamWriter(ConfigPath);
                serializer.Serialize(tr, data);
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
