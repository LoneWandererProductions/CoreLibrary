/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/DeSerialize.cs
 * PURPOSE:     De Serialize Objects, Lists and Dictionaries
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FileHandler;

namespace Serializer
{
    /// <summary>
    ///     Helper Class to Serialize and DeSerialize Objects for persistent Saving of Data
    ///     Important: Object we Serialize must be public!
    /// </summary>
    public static class DeSerialize
    {
        /// <summary>
        ///     Load the object from XML.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The Generic Object<see cref="T" />Can return null.</returns>
        /// <typeparam name="T">Type of the Object.</typeparam>
        public static T LoadObjectFromXml<T>(string path) where T : class, new()
        {
            //if File exists
            if (!FileHandleSearch.FileExists(path))
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorPath, path));
            }

            //check if file is empty, if empty return a new empty one
            if (!FileContent(path))
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorFileEmpty, path));
            }

            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                using TextReader reader = new StreamReader(path);
                //can return null but unlikely
                return deserializer.Deserialize(reader) as T;
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
        ///     Load the list object from XML.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="T" />Can return null.</returns>
        /// <typeparam name="T"></typeparam>
        public static List<T> LoadListFromXml<T>(string path) where T : class, new()
        {
            //if File exists
            if (!FileHandleSearch.FileExists(path))
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorPath, path));
            }

            //check if file is empty, if empty return a new empty one
            if (!FileContent(path))
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorFileEmpty, path));
            }

            try
            {
                var data = typeof(List<T>);
                var ser = new XmlSerializer(data);
                using Stream tr = File.OpenRead(path);
                return (List<T>)ser.Deserialize(tr);
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
        ///     Load the dictionary from XML.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The <see cref="T:Dictionary{TKey, TValue}" />Can return null.</returns>
        /// <typeparam name="TKey">Dictionary Key</typeparam>
        /// <typeparam name="TValue">Dictionary Value</typeparam>
        public static Dictionary<TKey, TValue> LoadDictionaryFromXml<TKey, TValue>(string path)
        {
            //if File exists
            if (!FileHandleSearch.FileExists(path))
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorPath, path));
            }

            //check if file is empty, if empty return a new empty one
            if (!FileContent(path))
            {
                throw new ArgumentException(string.Concat(SerialResources.ErrorFileEmpty, path));
            }

            var sr = new StreamReader(path);

            try
            {
                var xs = new XmlSerializer(typeof(List<Item>));
                var lst = (List<Item>)xs.Deserialize(sr);

                var dct = new Dictionary<TKey, TValue>();

                if (lst == null)
                {
                    return null;
                }

                foreach (var node in lst)
                {
                    var value = Deserialize<TValue>(node.Value);
                    var key = Deserialize<TKey>(node.Key);
                    dct.Add(key, value);
                }

                return dct;
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
        ///     Basic check if File is empty
        /// </summary>
        /// <param name="path">Name of the File</param>
        /// <returns>Status if File has actual content and some Debug Messages for easier Debugging</returns>
        private static bool FileContent(string path)
        {
            return new FileInfo(path).Length != 0;
        }

        /// <summary>
        ///     Generic deserializer.
        /// </summary>
        /// <typeparam name="T">>Generic Type of Object</typeparam>
        /// <param name="serialized">Object Serialized</param>
        /// <returns>The deserialized Object<see cref="T" />.</returns>
        private static T Deserialize<T>(this string serialized)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var reader = new StringReader(serialized);
            using var stm = new XmlTextReader(reader);
            return (T)serializer.Deserialize(stm);
        }
    }
}
