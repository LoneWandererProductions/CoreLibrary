/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        GraphicManager.cs
 * PURPOSE:     Implementation of our Contract for our Library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using LightVector.Enums;
using LightVector.Interfaces;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     Entry for our Vector Interface
    /// </summary>
    public class GraphicManager : IGraphicManager
    {
        /// <summary>
        /// The objects
        /// </summary>
        private readonly Dictionary<int, SaveObject> _objects = new();

        /// <inheritdoc />
        public void AddObject(int id, GraphicObject graphic, int layer, Point startCoordinates,
            Dictionary<string, object>? attributes = null)
        {
            if (_objects.ContainsKey(id))
            {
                throw new InvalidOperationException($"An object with ID {id} already exists.");
            }

            var saveObject = new SaveObject
            {
                Id = id,
                Layer = layer,
                StartCoordinates = startCoordinates,
                Graphic = graphic,
                Type = GetGraphicType(graphic)
            };

            // Assign optional attributes if provided
            if (attributes != null)
            {
                foreach (var (key, value) in attributes)
                {
                    graphic.OptionalAttributes[key] = value;
                }
            }

            _objects.Add(id, saveObject);
        }

        /// <inheritdoc />
        public bool ApplyTransformation(int id, Transform transformation)
        {
            if (!_objects.TryGetValue(id, out var obj))
            {
                // Optional: throw exception or return false depending on contract
                throw new KeyNotFoundException($"No object found with ID {id}.");
            }

            // Case 1: Translation (Move the Container)
            if (transformation is TranslateTransform translate)
            {
                // We modify the SaveObject's World Position, NOT the GraphicObject
                obj.StartCoordinates = new Point(
                    obj.StartCoordinates.X + translate.X,
                    obj.StartCoordinates.Y + translate.Y
                );
                return true;
            }

            // Case 2: Geometry Transform (Scale/Rotate the Shape)
            if (obj.Graphic.SupportsTransformation(transformation))
            {
                obj.Graphic.ApplyTransformation(transformation);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public SaveObject? GetObjectById(int id)
        {
            return _objects.TryGetValue(id, out var obj) ? obj : null;
        }

        /// <inheritdoc />
        public bool RemoveObject(int id)
        {
            return _objects.Remove(id);
        }

        /// <inheritdoc />
        public bool UpdateObjectLayer(int id, int newLayer)
        {
            if (!_objects.TryGetValue(id, out var obj)) return false;

            obj.Layer = newLayer;
            return true;
        }

        /// <inheritdoc />
        public bool UpdateObjectAttributes(int id, Dictionary<string, object> newAttributes)
        {
            if (!_objects.TryGetValue(id, out var obj)) return false;

            foreach (var (key, value) in newAttributes)
            {
                obj.Graphic.OptionalAttributes[key] = value;
            }

            return true;
        }

        /// <inheritdoc />
        public void SaveToFile(string filePath)
        {
            // Convert Dictionary Values to List for clean XML structure
            var exportList = _objects.Values.OrderBy(x => x.Id).ToList();
            SaveHelper.XmlSerializerObject(exportList, filePath);
        }

        /// <inheritdoc />
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var loadedObjects = SaveHelper.XmlDeSerializerObject<List<SaveObject>>(filePath);

            _objects.Clear();
            if (loadedObjects == null) return;

            foreach (var obj in loadedObjects)
            {
                _objects[obj.Id] = obj;
            }
        }

        /// <summary>
        /// Gets the type of the graphic.
        /// </summary>
        /// <param name="graphic">The graphic.</param>
        /// <returns></returns>
        private static GraphicTypes GetGraphicType(GraphicObject graphic)
        {
            return graphic switch
            {
                LineObject => GraphicTypes.Line,
                BezierCurve => GraphicTypes.BezierCurve,
                PolygonObject => GraphicTypes.Polygon,
                CircleObject => GraphicTypes.Circle,
                OvalObject => GraphicTypes.Oval,
                _ => throw new InvalidOperationException($"Unknown graphic type: {graphic.GetType().Name}")
            };
        }
    }
}
