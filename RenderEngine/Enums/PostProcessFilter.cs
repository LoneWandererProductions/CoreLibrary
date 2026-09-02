/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine.Enums
 * FILE:        PostProcessFilter.cs
 * PURPOSE:     Possible post-processing filter modes for rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace RenderEngine.Enums
{
    /// <summary>
    /// Available post-processing filter modes.
    /// </summary>
    public enum PostProcessFilter
    {
        /// <summary>
        /// No Filter.
        /// </summary>
        None = 0,

        /// <summary>
        /// The painterly Filter.
        /// </summary>
        Painterly = 1,

        /// <summary>
        /// The posterize edges Filter.
        /// </summary>
        PosterizeEdges = 2,

        /// <summary>
        /// The impasto canvas Filter.
        /// </summary>
        ImpastoCanvas = 3,

        /// <summary>
        /// The grayscale Filter.
        /// </summary>
        Grayscale = 4
    }
}
