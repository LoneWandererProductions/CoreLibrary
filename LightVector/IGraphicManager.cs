/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/IGraphicManager.cs
 * PURPOSE:     Interface Contract for our Library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/IGraphicManager.cs
 * PURPOSE:     Interface Contract for our Vector Library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


#nullable enable
using System.Collections.Generic;
using System.Windows;

namespace LightVector
{
    /// <summary>
    /// Interface for our Vector Library
    /// </summary>
    public interface IGraphicManager
    {
        /// <summary>
        /// Adds the object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="graphic">The graphic.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <param name="attributes">The attributes.</param>
        void AddObject(int id, GraphicObject graphic, int layer, Point startCoordinates,
            Dictionary<string, object>? attributes = null);

        /// <summary>
        /// Applies the transformation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="transformation">The transformation.</param>
        /// <returns>Status of the transformations</returns>
        bool ApplyTransformation(int id, Transform transformation);

        /// <summary>
        /// Gets the object by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        SaveObject? GetObjectById(int id);

        bool RemoveObject(int id);

        bool UpdateObjectLayer(int id, int newLayer);

        bool UpdateObjectAttributes(int id, Dictionary<string, object> newAttributes);

        void SaveToFile(string filePath);

        void LoadFromFile(string filePath);
    }
}
