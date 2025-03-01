/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/SaveHelper.cs
 * PURPOSE:     Save our Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace LightVector
{
    /// <summary>
    ///     Helper class for saving and loading XML files
    /// </summary>
    internal static class SaveHelper
    {
        /// <summary>
        /// Serializes an object to XML and saves it to a file.
        /// </summary>
        public static void XmlSerializerObject<T>(T serializeObject, string path)
        {
            if (serializeObject is null)
            {
                Trace.WriteLine(WvgResources.ErrorSerializerEmpty + path);
                File.Delete(path);
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StreamWriter(path);
                serializer.Serialize(writer, serializeObject);
            }
            catch (Exception error)
            {
                Trace.WriteLine(WvgResources.ErrorSerializer + error);
            }
        }

        /// <summary>
        /// Deserializes an XML file into an object of type T.
        /// </summary>
        public static T XmlDeSerializerObject<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {
                Trace.WriteLine(WvgResources.ErrorPath);
                return new T();
            }

            if (new FileInfo(path).Length == 0)
            {
                Trace.WriteLine(WvgResources.ErrorFileEmpty + path);
                return new T();
            }

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using Stream reader = File.OpenRead(path);
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception error)
            {
                Trace.WriteLine(WvgResources.ErrorDeSerializer + error);
                return new T();
            }
        }
    }
}

