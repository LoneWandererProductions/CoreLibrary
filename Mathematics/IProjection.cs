using System.Collections.Generic;
using DataFormatter;

namespace Mathematics
{
    public interface IProjection
    {
        List<Vector3D> GenerateMesh(ObjFile obj, Transform transform, int height, int width);
    }
}
