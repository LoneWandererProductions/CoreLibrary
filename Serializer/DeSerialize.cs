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
            ValidateFilePath(path);

            try
            {
                using var reader = new StreamReader(path);
                return new XmlSerializer(typeof(T)).Deserialize(reader) as T;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
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
            ValidateFilePath(path);

            try
            {
                var serializer = new XmlSerializer(typeof(List<T>));
                using var stream = new FileStream(path, FileMode.Open);
                return (List<T>)serializer.Deserialize(stream);
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
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
            ValidateFilePath(path);

            try
            {
                using var reader = new StreamReader(path);
                var list = (List<Item>)new XmlSerializer(typeof(List<Item>)).Deserialize(reader);

                return list.ToDictionary(
                    item => Deserialize<TKey>(item.Key),
                    item => Deserialize<TValue>(item.Value));
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                throw new Exception($"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
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
        ///     Validates the file path.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        private static void ValidateFilePath(string path)
        {
            if (!FileHandleSearch.FileExists(path))
            {
                throw new ArgumentException($"{SerialResources.ErrorPath} {path}");
            }

            if (!FileContent(path))
            {
                throw new ArgumentException($"{SerialResources.ErrorFileEmpty} {path}");
            }
        }

        /// <summary>
        ///     Generic deserializer.
        /// </summary>
        /// <typeparam name="T">Generic Type of Object</typeparam>
        /// <param name="serialized">Object Serialized</param>
        /// <returns>The deserialized Object<see cref="T" />.</returns>
        private static T Deserialize<T>(string serialized)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(serialized);
            return (T)serializer.Deserialize(reader);
        }
    }
}
