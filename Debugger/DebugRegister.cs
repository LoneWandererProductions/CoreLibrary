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
        ///     Gets or sets the color of the found item in the line
        /// </summary>
        /// <value>
        ///     The color of the found.
        /// </value>
        internal static string FoundColor { get; set; } = "Yellow";

        /// <summary>
        ///     Gets or sets the config.
        ///     This object will be saved in the file
        /// </summary>
        internal static ConfigExtended Config { get; private set; }

        /// <summary>
        /// Gets the color options.
        /// </summary>
        /// <value>
        /// The color options.
        /// </value>
        public static List<ColorOption> ColorOptions { get; private set; }

        /// <summary>
        ///     Read the config file and prepare the file, if it does not exist, create a new config and prepare it for saving.
        /// </summary>
        internal static void ReadConfigFile()
        {
            if (!File.Exists(DebuggerResources.ConfigFile))
            {
                Config = new ConfigExtended
                {
                    DebugPath = DebugPath,
                    SecondsTick = SecondsTick,
                    MinutesTick = MinutesTick,
                    HourTick = HourTick,
                    ErrorColor = DebuggerResources.ErrorColor,
                    InformationColor = DebuggerResources.InformationColor,
                    ExternalColor = DebuggerResources.ExternalColor,
                    StandardColor = DebuggerResources.StandardColor,
                    WarningColor = DebuggerResources.WarningColor,
                    IsDumpActive = IsDumpActive,
                    ColorOptions = ColorOptions
                };
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
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (XmlException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (NullReferenceException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex);
            }

            return null;
        }

        /// <summary>
        /// Generic Serializer Of Objects
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
            data.ErrorColor = DebuggerResources.ErrorColor;
            data.WarningColor = DebuggerResources.WarningColor;
            data.InformationColor = DebuggerResources.InformationColor;
            data.ExternalColor = DebuggerResources.ExternalColor;
            data.StandardColor = DebuggerResources.StandardColor;
            data.ColorOptions = ColorOptions;

            try
            {
                var serializer = new XmlSerializer(data.GetType());
                using var tr =
                    new StreamWriter(ConfigPath);
                serializer.Serialize(tr, data);
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (XmlException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (NullReferenceException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
