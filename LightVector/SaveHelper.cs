/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/SaveHelper.cs
 * PURPOSE:     Save our Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Xml.Serialization;
using Debugger;

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
                Directory.CreateDirectory(directory);
            }

            //check if file is empty, if empty return
            if (serializeObject.Equals(null))
            {
                DebugLog.CreateLogFile(WvgResources.ErrorSerializerEmpty + path, 0);
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
                DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorSerializer, error), 0);
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
                DebugLog.CreateLogFile(WvgResources.ErrorPath, 0);
                return new SaveContainer();
            }

            //check if file is empty, if empty return a new empty one
            if (new FileInfo(path).Length == 0)
            {
                DebugLog.CreateLogFile(WvgResources.ErrorFileEmpty + path, 0);
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
                DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorDeSerializer, error), 0);
                return new SaveContainer();
            }
        }
    }
}
