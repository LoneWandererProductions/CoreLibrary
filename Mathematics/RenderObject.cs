using System.Collections.Generic;

namespace Mathematics
{
    public sealed class RenderObject
    {
        public  Transform Transform { get; set; }
        public List<Triangle> Polygons { get; set; }

        public BaseMatrix ModelMatrix => Projection3DCamera.ModelMatrix(Transform);

        public RenderObject(List<Triangle> polygons, Transform transform)
        {
            Polygons = polygons;
            Transform = transform;
        }
    }
}
