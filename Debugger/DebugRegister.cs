/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebugRegister.cs
 * PURPOSE:     Handle the internal Config files
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
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
        ///     Gets or sets the error color.
        /// </summary>
        internal static string ErrorColor { get; set; } = "Red";

        /// <summary>
        ///     Gets or sets the warning color.
        /// </summary>
        internal static string WarningColor { get; set; } = "Orange";

        /// <summary>
        ///     Gets or sets the information color.
        /// </summary>
        internal static string InformationColor { get; set; } = "Blue";

        /// <summary>
        ///     Gets or sets the external color.
        /// </summary>
        internal static string ExternalColor { get; set; } = "Green";

        /// <summary>
        ///     Gets or sets the standard color.
        /// </summary>
        internal static string StandardColor { get; set; } = "Black";

        /// <summary>
        ///     Gets or sets the config.
        /// </summary>
        internal static ConfigExtended Config { get; private set; }

        /// <summary>
        ///     Read the config file.
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
                    ErrorColor = ErrorColor,
                    InformationColor = InformationColor,
                    ExternalColor = ExternalColor,
                    StandardColor = StandardColor,
                    WarningColor = WarningColor,
                    IsDumpActive = IsDumpActive
                };
            }
            else
            {
                var conf = SerializeConfig();
                if (conf == null)
                {
                    return;
                }

                //set file we want to read
                DebugPath = conf.DebugPath;
                //generic data in config
                DebugPath = conf.DebugPath;
                SecondsTick = conf.SecondsTick;
                MinutesTick = conf.MinutesTick;
                HourTick = conf.HourTick;
                IsDumpActive = IsDumpActive;

                Config = new ConfigExtended
                {
                    DebugPath = conf.DebugPath,
                    IsRunning = IsRunning,
                    SecondsTick = conf.SecondsTick,
                    MinutesTick = conf.MinutesTick,
                    HourTick = conf.HourTick,
                    ErrorColor = conf.ErrorColor,
                    InformationColor = conf.InformationColor,
                    ExternalColor = conf.ExternalColor,
                    StandardColor = conf.StandardColor,
                    WarningColor = conf.WarningColor
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
                Debug.WriteLine(ex);
            }
            catch (XmlException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        /// <summary>
        ///     Generic Serializer Of Objects
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="serializeObject">Target Object</param>
        internal static void XmlSerializerObject<T>(T serializeObject)
        {
            if (serializeObject is not ConfigExtended data)
            {
                return;
            }

            //Add our Colors a bit of a hack but works for now
            data.ErrorColor = ErrorColor;
            data.WarningColor = WarningColor;
            data.InformationColor = InformationColor;
            data.ExternalColor = ExternalColor;
            data.StandardColor = StandardColor;

            try
            {
                var serializer = new XmlSerializer(data.GetType());
                using var tr =
                    new StreamWriter(ConfigPath);
                serializer.Serialize(tr, data);
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (XmlException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
