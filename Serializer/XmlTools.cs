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

            throw new XmlException(SerialResources.ErrorPropertyNotFound);
        }

        /// <summary>
        ///     Read a specific Attributes Value
        /// </summary>
        /// <param name="path">Target Path with extension</param>
        /// <param name="property">Property of the XML File</param>
        /// <returns>All Values in a string list. Can return null.</returns>
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
                throw new XmlException(SerialResources.ErrorPropertyNotFound);
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
                throw new FileNotFoundException(ex.ToString());
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.ToString());
            }
            catch (XmlException ex)
            {
                throw new XmlException(ex.ToString());
            }
            catch (IOException ex)
            {
                throw new IOException(ex.ToString());
            }
            catch (NotSupportedException ex)
            {
                throw new NotSupportedException(ex.ToString());
            }
            catch (SecurityException ex)
            {
                throw new SecurityException(ex.ToString());
            }

            return doc;
        }
    }
}
