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


        public List<Triangle> Generate(IEnumerable<Triangle> triangles, Transform transform, Vector3D vCamera,
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
