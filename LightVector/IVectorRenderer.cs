/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/IVectorRenderer.cs
 * PURPOSE:     Contract on how to display our vector image the implementation details are up to the user
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace LightVector
{
    public interface IVectorRenderer
    {
        /// <summary>
        /// Renders to container.
        /// </summary>
        /// <returns>Returns a framework-specific container</returns>
        object RenderToContainer();

        /// <summary>
        /// Renders to image.
        /// </summary>
        /// <returns>Converts to a WPF Image</returns>
        System.Windows.Media.ImageSource RenderToImage();
    }

}
