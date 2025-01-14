/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Debugger
* FILE:        Debugger/DebugRegister.cs
* PURPOSE:     Handle the internal Config files
* PROGRAMMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
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
        private static readonly string ConfigPath =
            Path.Combine(Directory.GetCurrentDirectory(), DebuggerResources.ConfigFile);

        /// <summary>
        ///     Static constructor to initialize the config.
        /// </summary>
        static DebugRegister()
        {
            LoadConfig();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the Debugger is Running
        /// </summary>
        public static bool IsRunning { get; internal set; }

        /// <summary>
        ///     Get the Path to the Debug File
        /// </summary>
        internal static string DebugPath { get; set; }

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
        ///     Gets or sets the hour tick.
        /// </summary>
        internal static int HourTick { get; set; }

        /// <summary>
        ///     Gets or sets the minutes tick.
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
        ///     Gets the maximum size of the file.
        /// </summary>
        /// <value>
        ///     The maximum size of the file.
        /// </value>
        public static long MaxFileSize { get; private set; } = 5 * 1024 * 1024;

        /// <summary>
        ///     Gets the maximum file count.
        /// </summary>
        /// <value>
        ///     The maximum file count.
        /// </value>
        public static int MaxFileCount { get; private set; } = 10;

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
        ///     Gets or sets a value indicating whether this instance is verbose.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is verbose; otherwise, <c>false</c>.
        /// </value>
        public static bool IsVerbose { get; set; }

        /// <summary>
        ///     The base options
        /// </summary>
        private static ConfigExtended CreateBaseOptions()
        {
            return new ConfigExtended
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
                ColorOptions = DebuggerResources.InitialOptions,
                MaxFileSize = 5 * 1024 * 1024, // Default: 5 MB
                MaxFileCount = 10 // Default: 10 files
            };
        }

        /// <summary>
        ///     Load the configuration settings.
        /// </summary>
        private static void LoadConfig()
        {
            Config = File.Exists(DebuggerResources.ConfigFile)
                ? DeserializeConfig() ?? CreateBaseOptions()
                : CreateBaseOptions();
            ApplyConfig(Config);
        }

        /// <summary>
        ///     Resets this instance.
        /// </summary>
        internal static void Reset()
        {
            Config = CreateBaseOptions();
        }

        /// <summary>
        ///     Apply the configuration values from the loaded config.
        /// </summary>
        /// <param name="config">The config object containing the settings.</param>
        private static void ApplyConfig(ConfigExtended config)
        {
            DebugPath = config.DebugPath;
            SecondsTick = config.SecondsTick;
            MinutesTick = config.MinutesTick;
            HourTick = config.HourTick;
            IsDumpActive = config.IsDumpActive;
            MaxFileSize = config.MaxFileSize;
            MaxFileCount = config.MaxFileCount;
        }

        /// <summary>
        ///     Deserialize the config file.
        /// </summary>
        /// <returns>The <see cref="ConfigExtended" />.</returns>
        [return: MaybeNull]
        private static ConfigExtended DeserializeConfig()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ConfigExtended));
                using Stream tr = File.OpenRead(ConfigPath);
                return serializer.Deserialize(tr) as ConfigExtended;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
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
            if (serializeObject is not ConfigExtended data || options == null)
            {
                return;
            }

            ColorOptions = options;

            // Add our colors here
            data.ErrorColor = ErrorColor;
            data.WarningColor = WarningColor;
            data.InformationColor = InformationColor;
            data.ExternalColor = ExternalColor;
            data.StandardColor = StandardColor;
            data.ColorOptions = ColorOptions;
            data.MaxFileSize = MaxFileSize;
            data.MaxFileCount = MaxFileCount;

            try
            {
                var serializer = new XmlSerializer(data.GetType());
                using var tr = new StreamWriter(ConfigPath);
                serializer.Serialize(tr, data);
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
