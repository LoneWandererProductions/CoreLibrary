﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/ProjectionCamera.cs
 * PURPOSE:     Tests for the Camera and the whole set
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class ProjectionCamera
    {
        /// <summary>
        ///     Cameras this instance.
        /// </summary>
        [TestMethod]
        public void Camera()
        {
            var objFile = ResourceObjects.GetCube();
            var triangles = PolyTriangle.CreateTri(objFile);

            var translation = new Vector3D { X = 0, Y = 0, Z = 5 };

            var transform = Transform.GetInstance(translation, Vector3D.UnitVector, Vector3D.ZeroVector);
            transform.CameraType = Cameras.Orbit;
            transform.DisplayType = Display.Normal;

            var projection = new Projection();

            _ = projection.Generate(triangles, transform);

            transform.LeftCamera(0.5);
        }
    }
}
