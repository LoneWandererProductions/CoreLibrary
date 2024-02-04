using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    public class Projection : IProjection
    {
        public List<Vector3D> GenerateMesh(ObjFile obj, Transform transform, int height, int width)
        {
            var poly = Triangle.CreateTri(obj.Vectors);
            var renderObj = new RenderObject(poly, transform);
            var raster = new Rasterize {Height = height, Width = width};

            var updatedTri = raster.Render(renderObj, false);
            var tri = raster.PlotMesh(updatedTri);
            return tri;
        }
    }
}
