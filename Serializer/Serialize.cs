/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/Serialize.cs
 * PURPOSE:     Serialize Objects, Lists, and Dictionaries
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Serializer
{
    /// <summary>
    /// Helper class to serialize and deserialize objects for persistent data saving.
    /// Important: Objects we serialize must be public!
    /// </summary>
    public static class Serialize
    {
        /// <summary>
        /// Occurs when [on error].
        /// </summary>
        public static event Action<string, Exception> OnError;

        /// <summary>
        /// Occurs when [on information].
        /// </summary>
        public static event Action<string> OnInformation;

        /// <summary>
        /// Logs an error message and invokes the OnError event.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="ex">Optional exception to log</param>
        private static void LogError(string message, Exception ex = null)
        {
            OnError?.Invoke(message, ex); // Notify any listeners of the error
            Trace.WriteLine($"[{DateTime.Now}] ERROR: {message}"); // Log error to trace
        }

        /// <summary>
        /// Logs an informational message and invokes the OnInformation event.
        /// </summary>
        /// <param name="message">The information message</param>
        private static void LogInformation(string message)
        {
            OnInformation?.Invoke(message); // Notify any listeners of the information
            Trace.WriteLine($"[{DateTime.Now}] INFO: {message}"); // Log information to trace
        }

        /// <summary>
        /// Ensures that the directory for the specified path exists; creates it if not.
        /// </summary>
        /// <param name="path">The file path</param>
        private static void EnsureDirectoryExists(string path)
        {
            var folder = Path.GetDirectoryName(path); // Get directory part of the path
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder); // Create the directory if it doesn't exist
            }
        }

        /// <summary>
        /// Serializes an object to XML and saves it to the specified file path.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <param name="path">The file path where the object will be saved</param>
        public static void SaveObjectToXml<T>(T obj, string path)
        {
            if (obj == null) throw new ArgumentException("Object cannot be null.");

            EnsureDirectoryExists(path); // Ensure the directory exists before writing

            try
            {
                // Create a StreamWriter to write to the file and serialize the object to XML
                using var writer = new StreamWriter(path, false, Encoding.UTF8);
                new XmlSerializer(typeof(T)).Serialize(writer, obj);
                LogInformation($"Object of type {typeof(T)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                // Handle serialization errors and log them
                LogError($"Error during serialization: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Serializes a list of objects to XML and saves it to the specified file path.
        /// </summary>
        /// <typeparam name="T">The type of objects in the list</typeparam>
        /// <param name="obj">The list of objects to serialize</param>
        /// <param name="path">The file path where the list will be saved</param>
        public static void SaveLstObjectToXml<T>(List<T> obj, string path)
        {
            if (obj == null) throw new ArgumentException("Object cannot be null.");

            EnsureDirectoryExists(path); // Ensure the directory exists before writing

            try
            {
                // Serialize the list of objects and save it to the file
                using var fileStream = new FileStream(path, FileMode.Create);
                new XmlSerializer(typeof(List<T>)).Serialize(fileStream, obj);
                LogInformation($"List of type {typeof(T)} successfully serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                // Handle serialization errors and log them
                LogError($"Error during serialization: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Serializes a dictionary to XML and saves it to the specified file path.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key</typeparam>
        /// <typeparam name="TValue">The type of the dictionary value</typeparam>
        /// <param name="dct">The dictionary to serialize</param>
        /// <param name="path">The file path where the dictionary will be saved</param>
        public static void SaveDctObjectToXml<TKey, TValue>(Dictionary<TKey, TValue> dct, string path)
        {
            if (dct == null) throw new ArgumentException("Dictionary cannot be null.");

            EnsureDirectoryExists(path); // Ensure the directory exists before writing

            try
            {
                // Convert the dictionary to a serialized version and save it
                var myDictionary = dct.ToDictionary(
                    pair => Handle(pair.Key), // Serialize the key
                    pair => Handle(pair.Value) // Serialize the value
                );
                SerializeDictionary(myDictionary, path); // Serialize the dictionary
                LogInformation($"Dictionary with key type {typeof(TKey)} and value type {typeof(TValue)} serialized to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                // Handle serialization errors and log them
                LogError($"Error during serialization: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Serializes an object of any type to a string.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="obj">The object to serialize</param>
        /// <returns>A string representation of the serialized object</returns>
        private static string Handle<T>(T obj)
        {
            using var stringWriter = new StringWriter();
            new XmlSerializer(typeof(T)).Serialize(stringWriter, obj);
            return stringWriter.ToString(); // Return the serialized object as a string
        }

        /// <summary>
        /// Serializes a dictionary (with string keys and values) to XML and saves it to the specified file path.
        /// </summary>
        /// <param name="dct">The dictionary to serialize</param>
        /// <param name="path">The file path where the dictionary will be saved</param>
        private static void SerializeDictionary(Dictionary<string, string> dct, string path)
        {
            if (dct == null) throw new ArgumentException("Dictionary cannot be null.");

            EnsureDirectoryExists(path); // Ensure the directory exists before writing

            try
            {
                // Convert the dictionary to a list of Item objects
                var tempDataItems = dct.Select(pair => new Item(pair.Key, pair.Value)).ToList();
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty); // Remove XML namespaces

                // Serialize the list of items and save it to the file
                using var stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                new XmlSerializer(typeof(List<Item>)).Serialize(stringWriter, tempDataItems, namespaces);

                using var streamWriter = new StreamWriter(path);
                streamWriter.Write(stringWriter.ToString()); // Write serialized data to file
                LogInformation($"Dictionary serialized and saved to {path}");
            }
            catch (Exception ex) when (ex is InvalidOperationException or XmlException or NullReferenceException or UnauthorizedAccessException or ArgumentException or IOException)
            {
                // Handle serialization errors and log them
                LogError($"Error during serialization: {ex.Message}", ex);
                throw;
            }
        }
    }
}
