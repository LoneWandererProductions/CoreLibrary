/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/XmlTools.cs
 * PURPOSE:     Some Xml Tools
 *              Logging Via Events
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;

namespace Serializer
{
    /// <summary>
    ///     The XML tools class.
    ///     Important: Object we serialize must be public!
    /// </summary>
    public static class XmlTools
    {
        /// <summary>
        ///     Event triggered when an error occurs.
        /// </summary>
        public static event Action<string, Exception> OnError;

        /// <summary>
        ///     Event triggered for informational messages.
        /// </summary>
        public static event Action<string> OnInformation;

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        private static void LogError(string message, Exception ex = null)
        {
            OnError?.Invoke(message, ex);
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void LogInformation(string message)
        {
            OnInformation?.Invoke(message);
        }

        /// <summary>
        ///     Read a specific attribute value.
        /// </summary>
        /// <param name="path">Target file path with extension.</param>
        /// <param name="property">The property in the XML file.</param>
        /// <returns>First value as string. Can return null.</returns>
        public static string GetFirstAttributeFromXml(string path, string property)
        {
            // Check if file exists
            if (!File.Exists(path))
            {
                return null;
            }

            var doc = LoadXml(path);
            if (doc == null)
            {
                return string.Empty;
            }

            var elements = doc.GetElementsByTagName(property);
            if (elements.Count > 0)
            {
                return elements[0]?.InnerText;
            }

            LogError($"Property '{property}' not found in XML file: {path}");
            return null;
        }

        /// <summary>
        ///     Read all values of a specific property from the XML file.
        /// </summary>
        /// <param name="path">Target file path with extension.</param>
        /// <param name="property">The property in the XML file.</param>
        /// <returns>A list of all values found. Can return null.</returns>
        public static List<string> GetAttributesFromXml(string path, string property)
        {
            // Check if file exists
            if (!File.Exists(path))
            {
                return null;
            }

            var doc = LoadXml(path);
            if (doc == null)
            {
                return null;
            }

            var elements = doc.GetElementsByTagName(property);
            if (elements.Count == 0)
            {
                LogError($"Property '{property}' not found in XML file: {path}");
                return null;
            }

            // Add all element values to the list
            return (from XmlNode element in elements select element?.InnerText).ToList();
        }

        /// <summary>
        ///     Load an XML file.
        /// </summary>
        /// <param name="path">Target file path with extension.</param>
        /// <returns>XML document or null in case of error.</returns>
        [return: MaybeNull]
        public static XmlDocument LoadXml(string path)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(path);
                LogInformation($"Successfully loaded XML file: {path}");
            }
            catch (Exception ex) when (ex is FileNotFoundException or ArgumentException or XmlException or IOException or NotSupportedException or SecurityException)
            {
                LogError($"Error loading XML file: {path} - {ex.Message}", ex);
                return null;
            }

            return doc;
        }
    }
}
