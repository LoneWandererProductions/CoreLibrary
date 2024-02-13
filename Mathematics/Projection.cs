using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    public class Projection : IProjection
    {
        public List<Vector3D> GenerateMesh(ObjFile obj, Transform transform, int height, int width)
        {
            return null;
        }


        /// <summary>
        ///     Generates the specified triangles.
        /// </summary>
        /// <param name="triangles">The triangles.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="vCamera">The v camera.</param>
        /// <param name="vTarget">The v target.</param>
        /// <param name="vUp">The v up.</param>
        /// <param name="orthogonal">The orthogonal.</param>
        /// <returns>Converted 3d View</returns>
        public List<Triangle> Generate(List<Triangle> triangles, Transform transform, Vector3D vCamera, Vector3D vUp,
            bool? orthogonal)
        {
            var cache = Rasterize.WorldMatrix(triangles, transform);
            cache = Rasterize.ViewPort(cache, vCamera);

            cache = orthogonal == true ? Rasterize.Convert2DTo3D(cache) : Rasterize.Convert2DTo3DOrthographic(cache);

            return cache;
        }


        public List<Triangle> MoveIntoView(List<Triangle> triangles, int width, int height)
        {
            return Rasterize.MoveIntoView(triangles, width, height);
        }
    }
}
