/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ConvertObjectXml.cs
 * PURPOSE:     Convert Object to Xml
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Debugger
{
    /// <summary>
    ///     The convert object to XML string class.
    /// </summary>
    internal static class ConvertObjectXml
    {
        /// <summary>
        ///     Converts Enumeration into a XML String
        /// </summary>
        /// <typeparam name="T">Value Object</typeparam>
        /// <param name="serializeObject">Target Enumeration</param>
        /// <returns>Serialized Object</returns>
        internal static string ConvertListXml<T>(IEnumerable<T> serializeObject)
        {
            var convert = string.Empty;
            var bld = new StringBuilder(convert);

            foreach (var vector in serializeObject.Select(ConvertObjctXml))
            {
                _ = bld.AppendLine(vector);
            }

            return bld.ToString();
        }

        /// <summary>
        ///     Converts Dictionary into a XML String
        /// </summary>
        /// <typeparam name="T">Value Object</typeparam>
        /// <typeparam name="TU">Key Object</typeparam>
        /// <param name="serializeObject">Target Dictionary</param>
        /// <returns>Serialized Object</returns>
        internal static string ConvertDictionaryXml<T, TU>(Dictionary<T, TU> serializeObject)
        {
            var convert = string.Empty;
            var bld = new StringBuilder(convert);

            foreach (var element in serializeObject)
            {
                var vector = ConvertObjctXml(element.Value);
                bld.Append(element.Key).Append(DebuggerResources.Formating).AppendLine(vector);
            }

            return bld.ToString();
        }

        /// <summary>
        ///     Serialize an Object for debugging purposes
        /// </summary>
        /// <param name="element">The Object element.</param>
        /// <returns>The <see cref="string" />.</returns>
        /// <typeparam name="T">Generic Type</typeparam>
        internal static string ConvertObjctXml<T>(T element)
        {
            var serializer = new XmlSerializer(element.GetType());

            string str;

            var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            //not elegant, but still we can catch the exception and conclude something with the object was wrong
            try
            {
                using var stream = new StringWriter();
                using var writer = XmlWriter.Create(stream, settings);
                serializer.Serialize(writer, element, xns);
                return stream.ToString();
            }
            catch (ArgumentNullException ex)
            {
                str = ex.ToString();
                Trace.WriteLine(str);
            }

            return string.Concat(DebuggerResources.ErrorSerializing, str);
        }
    }
}
