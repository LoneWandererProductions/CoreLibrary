/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/Serialize.cs
 * PURPOSE:     Serialize Objects, Lists and Dictionaries
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using FileHandler;

namespace Serializer
{
    /// <summary>
    ///     Helper Class to Serialize and DeSerialize Objects for persistent Saving of Data
    ///     Important: Object we Serialize must be public!
    /// </summary>
    public static class Serialize
    {
        /// <summary>
        ///     Generic Serializer Of Objects
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="obj">Target Object</param>
        /// <param name="path">Target Path with extension</param>
        public static void SaveObjectToXml<T>(T obj, string path)
        {
            if (obj == null)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StreamWriter(path);
                serializer.Serialize(writer, obj);
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Generic Serializer Of List Objects
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="path">The path.</param>
        public static void SaveLstObjectToXml<T>(List<T> obj, string path)
        {
            if (obj == null || obj.Count == 0)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                new XmlSerializer(typeof(List<T>)).Serialize(fileStream, obj);
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Serializes Dictionary Type of: Tile Dictionary
        /// </summary>
        /// <param name="dct">Dictionary Tile</param>
        /// <param name="path">Target Path</param>
        public static void SaveDctObjectToXml<TKey, TValue>(Dictionary<TKey, TValue> dct, string path)
        {
            if (dct == null || dct.Count == 0)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            try
            {
                var myDictionary = dct.ToDictionary(
                    pair => Handle(pair.Key),
                    pair => Handle(pair.Value)
                );

                SerializeDictionary(myDictionary, path);
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Converts generic Object into a XML string
        /// </summary>
        /// <typeparam name="T">Generic Type of Object</typeparam>
        /// <param name="obj">Object to Serialize</param>
        /// <returns>Object as XML string</returns>
        private static string Handle<T>(T obj)
        {
            using var stringWriter = new StringWriter();
            new XmlSerializer(typeof(T)).Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        /// <summary>
        ///     Helper class for Dictionary Object also used for external use
        /// </summary>
        /// <param name="dct">Type Id String</param>
        /// <param name="path">File Destination</param>
        private static void SerializeDictionary(Dictionary<string, string> dct, string path)
        {
            if (dct == null || dct.Count == 0)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            try
            {
                var tempDataItems = dct.Select(pair => new Item(pair.Key, pair.Value)).ToList();
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                new XmlSerializer(typeof(List<Item>)).Serialize(stringWriter, tempDataItems, namespaces);

                using var streamWriter = new StreamWriter(path);
                streamWriter.Write(stringWriter.ToString());
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }
        }
    }
}
