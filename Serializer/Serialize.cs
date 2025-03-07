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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CoreMemoryLog;

namespace Serializer
{
    /// <summary>
    ///     Helper Class to Serialize and DeSerialize Objects for persistent Saving of Data
    ///     Important: Object we Serialize must be public!
    /// </summary>
    public static partial class Serialize
    {
        /// <summary>
        ///     Ensures that the directory for the given path exists.
        /// </summary>
        /// <param name="path">The file path.</param>
        private static void EnsureDirectoryExists(string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        /// <summary>
        /// Logs the provided message. For demonstration purposes, this logs to the console.
        /// In a real application, use a proper logging framework.
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

            EnsureDirectoryExists(path);

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                serializer.Serialize(writer, obj);
                Log(LogLevel.Information, $"Object of type {typeof(T)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
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
            if (obj == null)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            EnsureDirectoryExists(path);

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                new XmlSerializer(typeof(List<T>)).Serialize(fileStream, obj);
                Log(LogLevel.Information, $"List of type {typeof(T)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Serializes Dictionary Type of: Tile Dictionary
        /// </summary>
        /// <param name="dct">Dictionary Tile</param>
        /// <param name="path">Target Path</param>
        public static void SaveDctObjectToXml<TKey, TValue>(Dictionary<TKey, TValue> dct, string path)
        {
            if (dct == null)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            EnsureDirectoryExists(path);

            try
            {
                var myDictionary = dct.ToDictionary(
                    pair => Handle(pair.Key),
                    pair => Handle(pair.Value)
                );

                SerializeDictionary(myDictionary, path);
                Log(LogLevel.Information,
                    $"Dictionary with key type {typeof(TKey)} and value type {typeof(TValue)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
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
            if (dct == null)
            {
                throw new ArgumentException(SerialResources.ErrorSerializerEmpty);
            }

            EnsureDirectoryExists(path);

            try
            {
                var tempDataItems = dct.Select(pair => new Item(pair.Key, pair.Value)).ToList();
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);

                using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                new XmlSerializer(typeof(List<Item>)).Serialize(stringWriter, tempDataItems, namespaces);

                using var streamWriter = new StreamWriter(path);
                streamWriter.Write(stringWriter.ToString());
                Log(LogLevel.Information,$"Dictionary serialized and saved to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException
                                           or UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Custom exception for serialization errors.
        /// </summary>
        public sealed partial class SerializationException : Exception
        {
        }
    }
}
