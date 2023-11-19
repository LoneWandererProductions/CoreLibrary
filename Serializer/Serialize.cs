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
using ExtendedSystemObjects;
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
            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            //check if file is empty, if empty return
            if (obj == null)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            try
            {
                var serializer = new XmlSerializer(obj.GetType());

                using var tr = new StreamWriter(path);
                serializer.Serialize(tr, obj);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (XmlException ex)
            {
                throw new XmlException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (IOException ex)
            {
                throw new IOException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
        }

        /// <summary>
        ///     Generic Serializer Of List Objects
        ///     Serializer Works but not De Serializer
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="path">The path.</param>
        /// <typeparam name="T"></typeparam>
        public static void SaveLstObjectToXml<T>(List<T> obj, string path)
        {
            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            //check if file is empty, if empty return
            if (obj == null)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                var ser = new XmlSerializer(typeof(List<T>));
                ser.Serialize(fileStream, obj);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (XmlException ex)
            {
                throw new XmlException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (IOException ex)
            {
                throw new IOException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
        }

        /// <summary>
        ///     Serializes Dictionary Type of: Tile Dictionary
        ///     Uses SerializeDictionary
        /// </summary>
        /// <param name="dct">Dictionary Tile</param>
        /// <param name="path">Target Path</param>
        public static void SaveDctObjectToXml<TKey, TValue>(Dictionary<TKey, TValue> dct, string path)
        {
            var folder = Path.GetDirectoryName(path);
            FileHandleCreate.CreateFolder(folder);

            //check if file is empty, if empty return
            if (dct.IsNullOrEmpty())
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            var myDictionary = new Dictionary<string, string>();

            foreach (var (key, value) in dct)
            {
                var itemValue = Handle(value);
                var itemKey = Handle(key);

                if (string.IsNullOrEmpty(itemValue) || string.IsNullOrEmpty(itemKey))
                {
                    continue;
                }

                myDictionary.Add(itemKey, itemValue);
            }

            SerializeDictionary(myDictionary, path);
        }

        /// <summary>
        ///     Converts generic Object into a XML string
        /// </summary>
        /// <typeparam name="T">Generic Type of Object</typeparam>
        /// <param name="obj">Object to Serialize</param>
        /// <returns>Object as XML string</returns>
        private static string Handle<T>(this T obj)
        {
            using var stringWriter = new StringWriter();
            var serializer = new XmlSerializer(obj.GetType());
            serializer.Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        /// <summary>
        ///     helper class for Dictionary Object also used for external use
        /// </summary>
        /// <param name="dct">Type Id String</param>
        /// <param name="path">File Destination</param>
        private static void SerializeDictionary(Dictionary<string, string> dct, string path)
        {
            //check if file is empty, if empty return
            if (dct.IsNullOrEmpty())
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            var sw = new StringWriter(CultureInfo.InvariantCulture);
            try
            {
                var tempDataItems = new List<Item>(dct.Count);
                tempDataItems.AddRange(dct.Keys.Select(key => new Item(key, dct[key])));

                var serializer = new XmlSerializer(typeof(List<Item>));

                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);

                serializer.Serialize(sw, tempDataItems, ns);

                using var tr = new StreamWriter(path);
                tr.Write(sw.ToString());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (XmlException ex)
            {
                throw new XmlException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            catch (IOException ex)
            {
                throw new IOException(string.Concat(SerialResources.ErrorSerializerXml, ex));
            }
            finally
            {
                if (true)
                {
                    sw.Dispose();
                }
            }
        }
    }
}
