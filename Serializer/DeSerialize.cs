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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CoreMemoryLog;
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
        ///     Logs the provided message. For demonstration purposes, this logs to the console.
        ///     In a real application, use a proper logging framework.
        /// </summary>
        /// <param name="level">The Log Level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">The exception.</param>
        private static void Log(LogLevel level, string message, Exception ex = null)
        {
            InMemoryLogger.Instance.Log(level, message, nameof(Serialize), ex);
            Trace.WriteLine($"[{DateTime.Now}] {message}");
        }

        /// <summary>
        ///     Validates that the file exists and is not empty.
        /// </summary>
        /// <param name="path">The file path.</param>
        private static void ValidateFilePath(string path)
        {
            if (!FileHandleSearch.FileExists(path))
            {
                Log(LogLevel.Error, "File already exists.",
                    new ArgumentException($"{SerialResources.ErrorPath} {path}"));
            }

            if (!FileContent(path))
            {
                Log(LogLevel.Error, "File was not empty.",
                    new ArgumentException($"{SerialResources.ErrorFileEmpty} {path}"));
            }
        }

        /// <summary>
        ///     Checks if the file has content.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>True if file has content, false otherwise.</returns>
        private static bool FileContent(string path)
        {
            return new FileInfo(path).Length != 0;
        }

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
                using var reader = new StreamReader(path, Encoding.UTF8);
                var result = new XmlSerializer(typeof(T)).Deserialize(reader) as T;
                Log(LogLevel.Information, $"Object of type {typeof(T)} successfully deserialized from {path}");
                return result;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }

            return null;
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
                var result = (List<T>)serializer.Deserialize(stream);
                Log(LogLevel.Information, $"List of type {typeof(T)} successfully deserialized from {path}");
                return result;
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }

            return null;
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
                using var reader = new StreamReader(path, Encoding.UTF8);
                var list = (List<Item>)new XmlSerializer(typeof(List<Item>)).Deserialize(reader);

                if (list != null)
                {
                    var result = list.ToDictionary(
                        item => Deserialize<TKey>(item.Key),
                        item => Deserialize<TValue>(item.Value));
                    Log(LogLevel.Information,
                        $"Dictionary with key type {typeof(TKey)} and value type {typeof(TValue)} successfully deserialized from {path}");
                    return result;
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }

            return null;
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
