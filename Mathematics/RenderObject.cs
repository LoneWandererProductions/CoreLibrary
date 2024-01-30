using System.Collections.Generic;

namespace Mathematics
{
    public sealed class RenderObjects
    {
        public Transform Transform { get; set; }

        public static List<Triangle> Polygons { get; set; }

        public BaseMatrix ModelMatrix => Projection3DConstants.GetModelMatrix(Transform);

        public static RenderObjects Create(List<Triangle> polygons)
        {
            Polygons = polygons;
            return new RenderObjects();
        }
    }
}
