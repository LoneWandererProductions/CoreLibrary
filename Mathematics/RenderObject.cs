﻿using System.Collections.Generic;

namespace Mathematics
{
    public sealed class RenderObject
    {
        public Transform Transform { get; set; }

        public List<Triangle> Polygons { get; set; }

        public BaseMatrix ModelMatrix => Projection3DConstants.GetModelMatrix(Transform);

        public RenderObject Create(List<Triangle> polygons)
        {
            Polygons = polygons;
            return new RenderObject();
        }
    }
}