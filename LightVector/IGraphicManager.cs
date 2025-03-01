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
        void AddObject(int id, GraphicObject graphic, int layer, Point startCoordinates,
            Dictionary<string, object>? attributes = null);

        bool ApplyTransformation(int id, Transform transformation);

        SaveObject? GetObjectById(int id);

        bool RemoveObject(int id);
        bool UpdateObjectLayer(int id, int newLayer);
        bool UpdateObjectAttributes(int id, Dictionary<string, object> newAttributes);

        void SaveToFile(string filePath);
        void LoadFromFile(string filePath);
    }
}
