/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/XmlTools.cs
 * PURPOSE:     Some Xml Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Xml;
using Debugger;
using FileHandler;

// ReSharper disable once UnusedMember.Global, it is used, or should be at least
// ReSharper disable once MemberCanBePrivate.Global, should be visible to the outside

namespace Serializer
{
    /// <summary>
    ///     The XML tools class.
    ///     Important: Object we Serialize must be public!
    /// </summary>
    public static class XmlTools
    {
        /// <summary>
        ///     Read a specific Attributes Value
        /// </summary>
        /// <param name="path">Target Path with extension</param>
        /// <param name="property">Property of the XML File</param>
        /// <returns>First Value as string.Can return null.</returns>
        [return: MaybeNull]
        public static string GetFirstAttributeFromXml(string path, string property)
        {
            //if File exists
            if (!FileHandleSearch.FileExists(path))
            {
                return null;
            }

            var doc = LoadXml(path);
            if (doc == null)
            {
                return string.Empty;
            }

            var elements = doc.GetElementsByTagName(property);

            if (elements.Count != 0)
            {
                return elements[0]?.InnerText;
            }

            DebugLog.CreateLogFile(SerialResources.ErrorPropertyNotFound, ErCode.Error);

            return string.Empty;
        }

        /// <summary>
        ///     Read a specific Attributes Value
        /// </summary>
        /// <param name="path">Target Path with extension</param>
        /// <param name="property">Property of the XML File</param>
        /// <returns>All Values in a string list. Can return null.</returns>
        [return: MaybeNull]
        public static List<string> GetAttributesFromXml(string path, string property)
        {
            //if File exists
            if (!FileHandleSearch.FileExists(path))
            {
                return null;
            }

            var lst = new List<string>();
            var doc = LoadXml(path);
            if (doc == null)
            {
                return null;
            }

            var elements = doc.GetElementsByTagName(property);

            if (elements.Count == 0)
            {
                DebugLog.CreateLogFile(SerialResources.ErrorPropertyNotFound, ErCode.Error);
                return null;
            }

            for (var i = 0; i < elements.Count; i++)
            {
                var id = elements[i]?.InnerText;
                lst.Add(id);
            }

            //return all elements
            return lst;
        }

        /// <summary>
        ///     Load a XML File
        /// </summary>
        /// <param name="path">Target Path with extension</param>
        /// <returns>XML File or Error</returns>
        [return: MaybeNull]
        public static XmlDocument LoadXml(string path)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch (FileNotFoundException ex)
            {
                DebugLog.CreateLogFile(ex.ToString(), ErCode.Error);
                return null;
            }
            catch (ArgumentException ex)
            {
                DebugLog.CreateLogFile(ex.ToString(), ErCode.Error);
                return null;
            }
            catch (XmlException ex)
            {
                DebugLog.CreateLogFile(ex.ToString(), ErCode.Error);
                return null;
            }
            catch (IOException ex)
            {
                DebugLog.CreateLogFile(ex.ToString(), ErCode.Error);
                return null;
            }
            catch (NotSupportedException ex)
            {
                DebugLog.CreateLogFile(ex.ToString(), ErCode.Error);
                return null;
            }
            catch (SecurityException ex)
            {
                DebugLog.CreateLogFile(ex.ToString(), ErCode.Error);
                return null;
            }

            return doc;
        }
    }
}
