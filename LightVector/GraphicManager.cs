/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/GraphicManager.cs
 * PURPOSE:     Implementation of our Contract for our Library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

#nullable enable
using System;
using System.Collections.Generic;
using System.Windows;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     Entry for our Vector Interface
    /// </summary>
    /// <seealso cref="IGraphicManager" />
    public class GraphicManager : IGraphicManager
    {
        /// <summary>
        ///     The objects
        /// </summary>
        private readonly Dictionary<int, SaveObject> _objects = new();

        /// <inheritdoc />
        /// <exception cref="T:System.InvalidOperationException">An object with ID {id} already exists.</exception>
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
                    // Assuming GraphicObject has a dictionary to store optional attributes
                    graphic.OptionalAttributes[key] = value;
                }
            }

            _objects.Add(id, saveObject);
        }

        /// <inheritdoc />
        /// <exception cref="KeyNotFoundException">No object found with ID {id}.</exception>
        public bool ApplyTransformation(int id, Transform transformation)
        {
            if (!_objects.TryGetValue(id, out var obj))
            {
                throw new KeyNotFoundException($"No object found with ID {id}.");
            }

            // Check if the object supports the transformation
            if (!obj.Graphic.SupportsTransformation(transformation))
            {
                return false;
            }

            obj.Graphic.ApplyTransformation(transformation);
            return true;
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
            if (!_objects.TryGetValue(id, out var obj))
            {
                return false;
            }

            obj.Layer = newLayer;
            return true;
        }

        /// <inheritdoc />
        public bool UpdateObjectAttributes(int id, Dictionary<string, object> newAttributes)
        {
            if (!_objects.TryGetValue(id, out var obj))
            {
                return false;
            }

            foreach (var (key, value) in newAttributes)
            {
                obj.Graphic.OptionalAttributes[key] = value;
            }

            return true;
        }

        /// <inheritdoc />
        public void SaveToFile(string filePath)
        {
            SaveHelper.XmlSerializerObject(new List<SaveObject>(_objects.Values), filePath);
        }

        /// <inheritdoc />
        public void LoadFromFile(string filePath)
        {
            var loadedObjects = SaveHelper.XmlDeSerializerObject<List<SaveObject>>(filePath);
            _objects.Clear();
            foreach (var obj in loadedObjects)
            {
                _objects[obj.Id] = obj;
            }
        }

        /// <summary>
        ///     Gets the type of the graphic.
        /// </summary>
        /// <param name="graphic">The graphic.</param>
        /// <returns>Type of graphic.</returns>
        private static GraphicTypes GetGraphicType(GraphicObject graphic)
        {
            return graphic switch
            {
                LineObject => GraphicTypes.Line,
                BezierCurve => GraphicTypes.BezierCurve,
                PolygonObject => GraphicTypes.Polygon,
                CircleObject => GraphicTypes.Circle,
                OvalObject => GraphicTypes.Oval,
                _ => throw new InvalidOperationException("Unknown graphic type.")
            };
        }
    }
}
