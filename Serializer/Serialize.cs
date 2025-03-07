/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/Serialize.cs
 * PURPOSE:     Serialize Objects, Lists, and Dictionaries
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
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
    ///     Helper class to serialize and deserialize objects for persistent data saving.
    ///     Important: Objects we serialize must be public!
    /// </summary>
    public static class Serialize
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
        ///     Logs messages using the InMemoryLogger.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">The exception (optional).</param>
        private static void Log(LogLevel level, string message, Exception ex = null)
        {
            InMemoryLogger.Instance.Log(level, message, "Serializer", ex);
            Trace.WriteLine($"[{DateTime.Now}] {message}");
        }

        /// <summary>
        ///     Serializes an object to an XML file.
        /// </summary>
        public static void SaveObjectToXml<T>(T obj, string path)
        {
            if (obj == null) throw new ArgumentException(SerialResources.ErrorSerializerEmpty);

            EnsureDirectoryExists(path);

            try
            {
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                new XmlSerializer(typeof(T)).Serialize(writer, obj);
                Log(LogLevel.Information, $"Object of type {typeof(T)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or
                                       UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        ///     Serializes a list of objects to an XML file.
        /// </summary>
        public static void SaveLstObjectToXml<T>(List<T> obj, string path)
        {
            if (obj == null) throw new ArgumentException(SerialResources.ErrorSerializerEmpty);

            EnsureDirectoryExists(path);

            try
            {
                using var fileStream = new FileStream(path, FileMode.Create);
                new XmlSerializer(typeof(List<T>)).Serialize(fileStream, obj);
                Log(LogLevel.Information, $"List of type {typeof(T)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or
                                       UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        ///     Serializes a dictionary to an XML file.
        /// </summary>
        public static void SaveDctObjectToXml<TKey, TValue>(Dictionary<TKey, TValue> dct, string path)
        {
            if (dct == null) throw new ArgumentException(SerialResources.ErrorSerializerEmpty);

            EnsureDirectoryExists(path);

            try
            {
                var myDictionary = dct.ToDictionary(
                    pair => Handle(pair.Key),
                    pair => Handle(pair.Value)
                );

                SerializeDictionary(myDictionary, path);
                Log(LogLevel.Information, $"Dictionary with key type {typeof(TKey)} and value type {typeof(TValue)} serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or
                                       UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        ///     Converts an object into an XML string.
        /// </summary>
        private static string Handle<T>(T obj)
        {
            using var stringWriter = new StringWriter();
            new XmlSerializer(typeof(T)).Serialize(stringWriter, obj);
            return stringWriter.ToString();
        }

        /// <summary>
        ///     Serializes a dictionary into an XML file.
        /// </summary>
        private static void SerializeDictionary(Dictionary<string, string> dct, string path)
        {
            if (dct == null) throw new ArgumentException(SerialResources.ErrorSerializerEmpty);

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
                Log(LogLevel.Information, $"Dictionary serialized and saved to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or
                                       UnauthorizedAccessException or ArgumentException or IOException)
            {
                Log(LogLevel.Error, $"{SerialResources.ErrorSerializerXml} {ex.Message}", ex);
                throw;
            }
        }
    }
}
