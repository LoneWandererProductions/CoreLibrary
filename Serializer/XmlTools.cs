/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/XmlTools.cs
 * PURPOSE:     Some Xml Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using CoreMemoryLog;

// ReSharper disable once UnusedMember.Global, it is used, or should be at least
// ReSharper disable once MemberCanBePrivate.Global, should be visible to the outside

namespace Serializer
{
    /// <summary>
    ///     The XML tools class.
    ///     Important: Object we serialize must be public!
    /// </summary>
    public static class XmlTools
    {
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

            InMemoryLogger.Instance.Log(LogLevel.Error, SerialResources.ErrorPropertyNotFound, "Serializer");
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
                InMemoryLogger.Instance.Log(LogLevel.Error, SerialResources.ErrorPropertyNotFound, "Serializer");
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
            }
            catch (Exception ex) when (ex is FileNotFoundException or ArgumentException or XmlException or IOException or NotSupportedException or SecurityException)
            {
                InMemoryLogger.Instance.Log(LogLevel.Error, ex.Message, "Serializer", ex);
                return null;
            }

            return doc;
        }
    }
}
