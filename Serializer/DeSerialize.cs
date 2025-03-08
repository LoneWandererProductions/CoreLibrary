/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/DeSerialize.cs
 * PURPOSE:     De Serialize Objects, Lists and Dictionaries
 *              Logging done Via Events.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Serializer
{
    /// <summary>
    /// Helper Class to Serialize and Deserialize Objects, Lists, and Dictionaries
    /// Important: Object we Serialize must be public!
    /// </summary>
    public static class DeSerialize
    {
        /// <summary>
        /// Event for logging messages
        /// </summary>
        public static event Action<string, Exception> OnError;

        /// <summary>
        /// Occurs when [on information].
        /// </summary>
        public static event Action<string> OnInformation;

        /// <summary>
        /// Logs the provided message.
        /// </summary>
        private static void Log(string message, Exception ex = null)
        {
            OnError?.Invoke(message, ex);
            Trace.WriteLine($"[{DateTime.Now}] {message}");
        }

        /// <summary>
        /// Validates that the file exists and is not empty.
        /// </summary>
        private static void ValidateFilePath(string path)
        {
            if (!File.Exists(path))
            {
                Log( $"File does not exist: {path}", new ArgumentException(path));
                throw new ArgumentException($"File does not exist: {path}");
            }

            if (!FileContent(path))
            {
                Log($"File is empty: {path}", new ArgumentException(path));
                throw new ArgumentException($"File is empty: {path}");
            }
        }

        /// <summary>
        /// Checks if the file has content.
        /// </summary>
        private static bool FileContent(string path) => new FileInfo(path).Length > 0;

        /// <summary>
        /// Deserializes an object from XML.
        /// </summary>
        public static T LoadObjectFromXml<T>(string path) where T : class, new()
        {
            ValidateFilePath(path);

            try
            {
                using var reader = new StreamReader(path, Encoding.UTF8);
                var result = new XmlSerializer(typeof(T)).Deserialize(reader) as T;
                OnInformation?.Invoke($"Object of type {typeof(T)} successfully deserialized from {path}");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Error deserializing object from {path}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Deserializes a list from XML.
        /// </summary>
        public static List<T> LoadListFromXml<T>(string path) where T : class, new()
        {
            ValidateFilePath(path);

            try
            {
                var serializer = new XmlSerializer(typeof(List<T>));
                using var stream = new FileStream(path, FileMode.Open);
                var result = (List<T>)serializer.Deserialize(stream);

                OnInformation?.Invoke($"List of type {typeof(T)} successfully deserialized from {path}");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Error deserializing list from {path}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Deserializes a dictionary from XML.
        /// </summary>
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
                    OnInformation?.Invoke($"Dictionary successfully deserialized from {path}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log( $"Error deserializing dictionary from {path}: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Generic deserializer for a string.
        /// </summary>
        private static T Deserialize<T>(string serialized)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var reader = new StringReader(serialized);
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Log($"Error deserializing string: {ex.Message}", ex);
                return default;
            }
        }
    }
}

