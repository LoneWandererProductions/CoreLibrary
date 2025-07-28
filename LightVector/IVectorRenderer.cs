/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/IVectorRenderer.cs
 * PURPOSE:     Contract on how to display our vector image the implementation details are up to the user
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System.Windows.Media;

namespace LightVector;

/// <summary>
///     Interface for Vector Render
/// </summary>
public interface IVectorRenderer
{
    /// <summary>
    ///     Renders to container.
    /// </summary>
    /// <returns>Returns a framework-specific container</returns>
    object RenderToContainer();

    /// <summary>
    ///     Renders to image.
    /// </summary>
    /// <returns>Converts to a WPF Image</returns>
    ImageSource RenderToImage();
}
