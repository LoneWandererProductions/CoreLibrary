using System.Collections.Generic;

namespace Mathematics
{
    public sealed class RenderObject
    {
        public RenderObject(List<Triangle> polygons, Transform transform)
        {
            Polygons = polygons;
            Transform = transform;
        }

        public Transform Transform { get; set; }
        public List<Triangle> Polygons { get; set; }

        public BaseMatrix ModelMatrix => Projection3DCamera.ModelMatrix(Transform);
    }
}
