using System;
using System.Collections.Generic;
using System.Numerics;

namespace LightVector
{
    /// <inheritdoc />
    /// <summary>
    ///     The polygon object class.
    /// </summary>
    [Serializable]
    public sealed class PolygonObject : GraphicObject
    {
        /// <summary>
        /// The list of vertices for the polygon.
        /// </summary>
        public List<Vector2> Vertices { get; set; } = new();

        /// <inheritdoc />
        /// <summary>
        /// Apply transformation method (scaling, rotation, etc.) to each vertex of the polygon.
        /// </summary>
        /// <param name="transformation">The transformation.</param>
        public override void ApplyTransformation(Transform transformation)
        {
            for (var i = 0; i < Vertices.Count; i++)
            {
                switch (transformation)
                {
                    case ScaleTransform scale:
                        // Apply scaling transformation to each vertex
                        Vertices[i] = new Vector2(Vertices[i].X * scale.ScaleX, Vertices[i].Y * scale.ScaleY);
                        break;
                    case RotateTransform rotate:
                        // Apply rotation transformation to each vertex
                        Vertices[i] = RotateVector(Vertices[i], rotate.Angle);
                        break;
                    case TranslateTransform translate:
                        // Apply translation transformation to each vertex
                        Vertices[i] = new Vector2(Vertices[i].X + (float)translate.X, Vertices[i].Y + (float)translate.Y);
                        break;
                }
            }
        }

        private Vector2 RotateVector(Vector2 vector, double angle)
        {
            // Convert angle to radians
            var angleRad = angle * (Math.PI / 180);

            // Rotation matrix
            var cosTheta = (float)Math.Cos(angleRad);
            var sinTheta = (float)Math.Sin(angleRad);

            // Rotate the vector
            var xNew = vector.X * cosTheta - vector.Y * sinTheta;
            var yNew = vector.X * sinTheta + vector.Y * cosTheta;

            return new Vector2(xNew, yNew);
        }
    }
}
