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
    ///     Interface for our Vector Library
    /// </summary>
    public interface IGraphicManager
    {
        /// <summary>
        ///     Adds the object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="graphic">The graphic.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <param name="attributes">The attributes.</param>
        void AddObject(int id, GraphicObject graphic, int layer, Point startCoordinates,
            Dictionary<string, object>? attributes = null);

        /// <summary>
        ///     Applies the transformation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="transformation">The transformation.</param>
        /// <returns>Status of the transformations</returns>
        bool ApplyTransformation(int id, Transform transformation);

        /// <summary>
        ///     Gets the object by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        SaveObject? GetObjectById(int id);

        /// <summary>
        /// Removes the object.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Status of operation. if failed false, if success, true.</returns>
        bool RemoveObject(int id);

        /// <summary>
        /// Updates the object layer.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newLayer">The new layer.</param>
        /// <returns>Status of operation. if failed false, if success, true.</returns>
        bool UpdateObjectLayer(int id, int newLayer);

        /// <summary>
        /// Updates the object attributes.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newAttributes">The new attributes.</param>
        /// <returns>Status of operation. if failed false, if success, true.</returns>
        bool UpdateObjectAttributes(int id, Dictionary<string, object> newAttributes);

        /// <summary>
        /// Saves to file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void SaveToFile(string filePath);

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        void LoadFromFile(string filePath);
    }
}
