using System.Collections.Generic;
using System.Diagnostics;

namespace Mathematics
{
    public sealed class Rasterizer
    {
        private readonly int _width, _height;

        public IEnumerable<Triangle> Render(RenderObject rObject, bool IsOrthographic)
        {
            // Precompute all necessary matrices (MVP)
            // (((v * M) * V) * P) -> Row Major ('v' on the right)
            var modelMatrix = rObject.ModelMatrix;
            var viewMatrix = Projection3DCamera.ViewMatrix(new Vector3D(), 90); //TODO _camera.ViewMatrix;

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
                    updatedTri[i][j] = (rObject.Polygons[i][j] * modelMatrix);
                }

                Trace.WriteLine("2:" + updatedTri[i]);

                // For a triangle to be illuminated the DP between its normal and the light's direction must be > 0
                //var lightDp = (updatedTri[i].Normal * -_light.Direction);
                //ShadeTri(ref updatedTri[i], lightDp);

                for (var j = 0; j < updatedTri[i].VertexCount; j++)
                {
                    // World to View
                    var worldView = (updatedTri[i][j] * viewMatrix);

                    // View to Projection
                    //Convert 3D to 2D
                    var v1 = Projection3DCamera.ProjectionTo3D(worldView);
                    //worldView = worldView * projectionMatrix;

                    var v2 = Projection3DCamera.OrthographicProjectionTo3D(worldView);

                    // Perspective Division
                    //updatedTri[i][j] = _camera.IsOrthographic() ? worldView : worldView / worldView.W;
                    updatedTri[i][j] = IsOrthographic ? v2 : v1;
                }
            }

            return updatedTri;

            //PlotMesh(updatedTri);
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

        // Draws Mesh
        public void PlotMesh(Triangle[] projected)
        {
            for (var i = 0; i < projected.Length; i++)
            {
                if (BackFaceCulled(projected[i]))
                {
                    continue;
                }

                for (var j = 0; j < projected[i].VertexCount; j++)
                {
                    projected[i][j] = ConvertToRaster(projected[i][j]);
                }

                for (var j = 0;
                        j < projected[i].VertexCount;
                        j++)
                    // TODO Fills every possible spot between two given points to form a line
                {
                    LineVector.LinearLine(projected[i][j], projected[i][(j + 1) % projected[i].VertexCount]);
                }
            }
        }

        // Does not reverse 'Y' value (actual console Y)
        private Vector3D ConvertToRaster(Vector3D v)
        {
            /* Inverting the Y value 'fixes' a bug 
             * in which some triangles are not rendered;
             * Y gets inverted again at 'PlotPoint' */
            return new((int)((v.X + 1) * 0.5f * _width),
                -(int)((1 - ((v.Y + 1) * 0.5f)) * _height),
                -v.Z);
        }

        // Check if triangle is turning away from the camera
        private bool BackFaceCulled(Triangle polygon)
        {
            // Shoelace formula: https://en.wikipedia.org/wiki/Shoelace_formula#Statement
            // Division by 2 is not necessary, since all we care about is if the value is positive/negative

            double sum = 0.0f;

            for (var i = 0; i < polygon.VertexCount; i++)
            {
                sum += (polygon[i].X * polygon[(i + 1) % polygon.VertexCount].Y) -
                       (polygon[i].Y * polygon[(i + 1) % polygon.VertexCount].X);
            }

            return sum >= 0;
        }
    }
}
