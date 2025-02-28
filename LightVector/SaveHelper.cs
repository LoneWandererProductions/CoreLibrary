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
    ///     Basic Serializer
    /// </summary>
    internal static class SaveHelper
    {
        /// <summary>
        ///     Generic Serializer Of Objects
        /// </summary>
        /// <param name="serializeObject">Target Object</param>
        /// <param name="path">Target Path with extension</param>
        internal static void XmlSerializerObject<T>(T serializeObject, string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory) && directory != null)
            {
                _ = Directory.CreateDirectory(directory);
            }

            //check if file is empty, if empty return
            if (serializeObject.Equals(null))
            {
                Trace.WriteLine(WvgResources.ErrorSerializerEmpty + path);
                File.Delete(path);
                return;
            }

            try
            {
                var serializer = new XmlSerializer(serializeObject.GetType(),
                    new[] { typeof(LineObject), typeof(CurveObject) });
                using var tr = new StreamWriter(path);
                serializer.Serialize(tr, serializeObject);
            }
            catch (Exception error)
            {
                Trace.WriteLine(string.Concat(WvgResources.ErrorSerializer, error));
            }
        }

        /// <summary>
        ///     DeSerializes Object Type of: MapObject
        /// </summary>
        /// <param name="path">Target Path</param>
        internal static SaveContainer XmlDeSerializerSaveContainer(string path)
        {
            if (!File.Exists(path))
            {
                Trace.WriteLine(WvgResources.ErrorPath);
                return new SaveContainer();
            }

            //check if file is empty, if empty return a new empty one
            if (new FileInfo(path).Length == 0)
            {
                Trace.WriteLine(WvgResources.ErrorFileEmpty + path);
                return new SaveContainer();
            }

            try
            {
                var serializer = new XmlSerializer(typeof(SaveContainer));
                using Stream tr = File.OpenRead(path);
                return (SaveContainer)serializer.Deserialize(tr);
            }
            catch (Exception error)
            {
                Trace.WriteLine(string.Concat(WvgResources.ErrorDeSerializer, error));
                return new SaveContainer();
            }
        }
    }
}
