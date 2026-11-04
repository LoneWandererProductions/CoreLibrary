/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageResource
 * FILE:        RenderResource.cs
 * PURPOSE:     String Resources for rendering operations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects
{
    /// <summary>
    ///     Mostly static string constants.
    /// </summary>
    internal static class ImageResource
    {
        /// <summary>
        /// The error input buffer
        /// </summary>
        internal const string ErrorInputBuffer = "Input buffer size does not match.";

        /// <summary>
        /// The error layer size
        /// </summary>
        internal const string ErrorLayerSize = "Layer size does not match container size.";

        /// <summary>
        /// The error invalid layer index
        /// </summary>
        internal const string ErrorInvalidLayerIndex = "Invalid layer index {0}";

        /// <summary>
        /// The error no layers
        /// </summary>
        internal const string ErrorNoLayers = "No layers to composite.";

        /// <summary>
        /// The error layer size mismatch
        /// </summary>
        internal const string ErrorLayerSizeMismatch = "Layer size mismatch.";

        /// <summary>
        /// The error invalid document size
        /// </summary>
        internal const string ErrorInvalidDocumentSize = "Invalid document size {0}x{1}.";

        /// <summary>
        /// The error layer not found
        /// </summary>
        internal const string ErrorLayerNotFound = "Layer {0} not found.";

        /// <summary>
        /// The error layer exists
        /// </summary>
        internal const string ErrorLayerExists = "A layer with id {0} already exists.";

        /// <summary>
        /// The error invalid layer type
        /// </summary>
        internal const string ErrorInvalidLayerType = "Layer {0} is not a {1}.";

        /// <summary>
        /// The error shape exists
        /// </summary>
        internal const string ErrorShapeExists = "A shape with id {0} already exists on this layer.";

        /// <summary>
        /// The error shape not found
        /// </summary>
        internal const string ErrorShapeNotFound = "Shape {0} not found.";

        /// <summary>
        /// The error shape identifier mismatch
        /// </summary>
        internal const string ErrorShapeIdMismatch = "A replacement shape must keep the id of the shape it replaces.";

        /// <summary>
        /// The error no shape rasterizer
        /// </summary>
        internal const string ErrorNoShapeRasterizer =
            "The document contains shape layers but no IShapeRasterizer was supplied.";
    }
}
