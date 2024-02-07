using System.Collections.Generic;
using System.Diagnostics;

namespace Mathematics
{
    public sealed class Rasterize
    {
        public int Width { get; set; }
        public int Height { get; set; }

        internal IList<Triangle> Render(RenderObject rObject, bool isOrthographic)
        {
            // Precompute all necessary matrices (MVP)
            // (((v * M) * V) * P) -> Row Major ('v' on the right)
            var modelMatrix = rObject.ModelMatrix;
            var viewMatrix = Projection3DCamera.ViewMatrix(rObject.Transform);

            // Create array to store transformed triangles
            var updatedTri = new Triangle[rObject.Polygons.Count];

            for (var i = 0; i < rObject.Polygons.Count; i++)
            {
                //TODO error here
                updatedTri[i] = new Triangle(new Vector3D[rObject.Polygons[i].VertexCount]);
                //Trace.WriteLine("1:" + updatedTri[i]);

                for (var j = 0;
                     j < rObject.Polygons[i].VertexCount;
                     j++) // Object to World space (Local to World coords)
                {
                    updatedTri[i][j] = rObject.Polygons[i][j] * modelMatrix;
                }

                Trace.WriteLine("2:" + updatedTri[i]);

                // For a triangle to be illuminated the DP between its normal and the light's direction must be > 0
                //var lightDp = (updatedTri[i].Normal * -_light.Direction);
                //ShadeTri(ref updatedTri[i], lightDp);

                for (var j = 0; j < updatedTri[i].VertexCount; j++)
                {
                    // World to View
                    var worldView = updatedTri[i][j] * viewMatrix;

                    // View to Projection
                    //Convert 3D to 2D
                    var v1 = Projection3DCamera.ProjectionTo3D(worldView);
                    //worldView = worldView * projectionMatrix;

                    var v2 = Projection3DCamera.OrthographicProjectionTo3D(worldView);

                    // Perspective Division
                    //updatedTri[i][j] = _camera.IsOrthographic() ? worldView : worldView / worldView.W;
                    updatedTri[i][j] = isOrthographic ? v2 : v1;
                }
            }

            return updatedTri;
        }

        private void ShadeTri(ref Triangle tri, double dotProduct)
        {
            //if (dotProduct < 0.0f) return ShadeChar.Null;

            if (dotProduct < 0.1f)
            {
            }
            else if (dotProduct < 0.5f)
            {
            }
            else if (dotProduct < 0.7f)
            {
            }
        }

        /// <summary>
        ///     Backs the face culled.
        ///     Shoelace formula: https://en.wikipedia.org/wiki/Shoelace_formula#Statement
        ///     Division by 2 is not necessary, since all we care about is if the value is positive/negative
        /// </summary>
        /// <param name="projected">The projected.</param>
        /// <returns>List of visible Triangles</returns>
        public List<Triangle> BackFaceCulled(IEnumerable<Triangle> projected)
        {
            var culled = new List<Triangle>();

            foreach (var triangle in projected)
            {
                double sum = 0.0f;

                for (var i = 0; i < triangle.VertexCount; i++)
                {
                    sum += (triangle[i].X * triangle[(i + 1) % triangle.VertexCount].Y) -
                           (triangle[i].Y * triangle[(i + 1) % triangle.VertexCount].X);
                }

                if (sum < 0)
                {
                    culled.Add(triangle);
                }
            }

            return culled;
        }

        /// <summary>
        ///     Converts the into view.
        /// </summary>
        /// <param name="projected">The projected.</param>
        /// <returns></returns>
        public List<Vector3D> ConvertIntoView(IEnumerable<Triangle> projected)
        {
            var lst = new List<Vector3D>();

            foreach (var tri in projected)
            {
                for (var j = 0; j < tri.VertexCount; j++)
                {
                    tri[j] = ConvertToRaster(tri[j]);
                }

                for (var j = 0; j < tri.VertexCount; j++)
                {
                    lst.Add(tri[j]);

                    lst.Add(tri[(j + 1) % tri.VertexCount]);
                }
            }

            return lst;
        }


        /// <summary>
        ///     Converts to raster.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>New Coordinates to center the View into the Image</returns>
        public Vector3D ConvertToRaster(Vector3D v)
        {
            return new Vector3D((int)((v.X + 1) * 0.5d * Width),
                (int)((1 - ((v.Y + 1) * 0.5d)) * Height),
                -v.Z);
        }
    }
}
