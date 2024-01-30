using System.Collections.Generic;

namespace Mathematics
{
    public sealed class RenderObject
    {
        public Transform Transform { get; set; }

        public static List<Triangle> Polygons { get; set; }

        public BaseMatrix ModelMatrix => Projection3DCamera.GetModelMatrix(Transform);

        public static RenderObject Create(List<Triangle> polygons)
        {
            Polygons = polygons;
            return new RenderObject();
        }
    }
}
