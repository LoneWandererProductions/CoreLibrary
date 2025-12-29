/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Projections.cs
 * PURPOSE:     Test single parts of the 3D Engine
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    /// Test for Projections.
    /// </summary>
    [TestClass]
    public class Projections
    {
        /// <summary>
        ///     Test the Obj Loader
        /// </summary>
        [TestMethod]
        public void MeshLoader()
        {
            var obj = ResourceObjects.GetCube();
            var poly = PolyTriangle.CreateTri(obj);

            Assert.AreEqual(12, poly.Count, "Not the correct Count");
        }

        /// <summary>
        ///     Projections from 3d to 2d.
        /// </summary>
        [TestMethod]
        public void ProjectionTo2D()
        {
            var vector = new Vector3D(8, 4, 5);

            const int screenHeight = 240;
            const int screenWidth = 256;

            // Projection Matrix
            const float fNear = 0.1f;
            const float fFar = 1000.0f;
            const float fFov = 90.0f;
            //since a error was fixed this one will be switched just for this
            const double fAspectRatio = (double)screenWidth / screenHeight;
            var fFovRad = 1.0f / Math.Tan(fFov * 0.5f / 180.0f * 3.14159f);

            Projection3DRegister.Width = screenWidth;
            Projection3DRegister.Height = screenHeight;
            Projection3DRegister.Angle = fFov;

            var matProj = new BaseMatrix();

            var m1 = new double[4, 4];

            m1[0, 0] = fAspectRatio * fFovRad;
            m1[1, 1] = fFovRad;
            m1[2, 2] = fFar / (fFar - fNear);
            m1[3, 2] = -fFar * fNear / (fFar - fNear);
            m1[2, 3] = 1.0f;
            m1[3, 3] = 0.0f;
            matProj.Matrix = m1;

            var vec = MultiplyMatrixVector(vector, matProj);

            var comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");

            //known values: 1,1,3 expected: 1/3, 1/3, 1

            vector = new Vector3D(1, 1, 3);

            vec = MultiplyMatrixVector(vector, matProj);

            comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");

            vector = new Vector3D(1, 1, 7);

            vec = MultiplyMatrixVector(vector, matProj);

            comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");

            //known values: 1,1,3 expected: 1/3, 1/3, 1

            vector = new Vector3D(1, 1, 3);

            vec = MultiplyMatrixVector(vector, matProj);

            comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");

            vector = new Vector3D(1, 1, 7);

            vec = MultiplyMatrixVector(vector, matProj);

            comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");

            //known values: 1,1,3 expected: 1/3, 1/3, 1

            vector = new Vector3D(-1, 1, 3);

            vec = MultiplyMatrixVector(vector, matProj);

            comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");
        }

        /// <summary>
        ///     Projections from 3d to 2d.
        /// </summary>
        [TestMethod]
        public void PointAt()
        {
            var transform = Transform.GetInstance();

            var expected = new BaseMatrix(4, 4) { [0, 0] = 1, [1, 1] = 1, [2, 2] = 1, [3, 3] = 1 };

            var matrix = Projection3DCamera.PointAt(transform);

            var check = expected == matrix;
            Trace.WriteLine(matrix.ToString());
            Trace.WriteLine(transform.Position.ToString());
            Assert.IsTrue(check, "Wrong Point At Matrix");

            var m = new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { -2, -2, -2, 1 } };

            transform = Transform.GetInstance();
            transform.Position = new Vector3D(2, 2, 2);

            expected = new BaseMatrix { Matrix = m };

            matrix = Projection3DCamera.PointAt(transform);

            check = expected == matrix;
            var cache = expected - matrix;

            Assert.IsTrue(check, $"Wrong Point At Matrix: {cache}");
        }

        /// <summary>
        ///     The Model matrix.
        ///     sample
        ///     transform. Scale 1,1,1, Rotation, 0,0,0, Position, 0,0,3
        ///     1 0 0 0
        ///     0 1 -0 0
        ///     -0 0 1 0
        ///     -0 -0 -0 1
        ///     before:
        ///     [0] = {1: (-1; -1; -1) 2: (-1; 1; -1) 3: (1; 1; -1)}
        ///     [1] = {1: (-1; -1; -1) 2: (1; 1; -1) 3: (1; -1; -1)}
        ///     after:
        ///     [0] = {1: (-1; -1; 2) 2: (-1; 1; 2) 3: (1; 1; 2)}
        ///     [1] = {1: (-1; -1; 2) 2: (1; 1; 2) 3: (1; -1; 2)}
        /// </summary>
        [TestMethod]
        public void ModelMatrix()
        {
            var transform = Transform.GetInstance();

            var matrix = new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 3, 1 } };

            var model = new BaseMatrix { Matrix = matrix };
            transform.Position = new Vector3D(0, 0, 0);
            transform.Translation.Z = 3;
            var cache = Projection3DCamera.ModelMatrix(transform);

            var check = model.Equals(cache);
            Trace.WriteLine(cache.ToString());
            Trace.WriteLine(transform.Position.ToString());

            Assert.IsTrue(check, "Not the Correct Model Matrix");

            check = model == cache;
            Assert.IsTrue(check, "Equal check failed");

            //before
            //  [0] = {1: (-1; -1; -1) 2: (-1; 1; -1) 3: (1; 1; -1)}
            //after
            //  [0] = {1: (-1; -1; 2) 2: (-1; 1; 2) 3: (1; 1; 2)}
            var v1 = new Vector3D(-1, -1, -1);
            var tv1 = new Vector3D(-1, -1, 2);
            var v2 = new Vector3D(-1, 1, -1);
            var tv2 = new Vector3D(-1, 1, 2);
            var v3 = new Vector3D(1, 1, -1);
            var tv3 = new Vector3D(1, 1, 2);

            var target1 = v1 * model;
            var target2 = v2 * model;
            var target3 = v3 * model;

            check = target1 == tv1;
            Assert.IsTrue(check, "Vector not converted");
            check = target2 == tv2;
            Assert.IsTrue(check, "Vector not converted");
            check = target3 == tv3;
            Assert.IsTrue(check, "Vector not converted");

            //before
            //  [1] = {1: (-1; -1; -1) 2: (1; 1; -1) 3: (1; -1; -1)}
            //after
            //  [1] = {1: (-1; -1; 2) 2: (1; 1; 2) 3: (1; -1; 2)}
            v1 = new Vector3D(-1, -1, -1);
            tv1 = new Vector3D(-1, -1, 2);
            v2 = new Vector3D(1, 1, -1);
            tv2 = new Vector3D(1, 1, 2);
            v3 = new Vector3D(1, -1, -1);
            tv3 = new Vector3D(1, -1, 2);

            target1 = v1 * model;
            target2 = v2 * model;
            target3 = v3 * model;

            check = target1 == tv1;
            Assert.IsTrue(check, "Vector not converted");
            check = target2 == tv2;
            Assert.IsTrue(check, "Vector not converted");
            check = target3 == tv3;
            Assert.IsTrue(check, "Vector not converted");
        }

        /// <summary>
        ///     Multiplies the matrix vector.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="matProj">The mat proj.</param>
        /// <returns>The Projection Vector</returns>
        private static Vector3D MultiplyMatrixVector(Vector3D i, BaseMatrix matProj)
        {
            var o = new Vector3D
            {
                X = (i.X * matProj.Matrix[0, 0]) + (i.Y * matProj.Matrix[1, 0]) + (i.Z * matProj.Matrix[2, 0]) +
                    matProj.Matrix[3, 0],
                Y = (i.X * matProj.Matrix[0, 1]) + (i.Y * matProj.Matrix[1, 1]) + (i.Z * matProj.Matrix[2, 1]) +
                    matProj.Matrix[3, 1],
                Z = (i.X * matProj.Matrix[0, 2]) + (i.Y * matProj.Matrix[1, 2]) + (i.Z * matProj.Matrix[2, 2]) +
                    matProj.Matrix[3, 2]
            };

            var w = (i.X * matProj.Matrix[0, 3]) + (i.Y * matProj.Matrix[1, 3]) + (i.Z * matProj.Matrix[2, 3]) +
                    matProj.Matrix[3, 3];

            w = Math.Round(w, 2);

            if (w == 0.0f)
            {
                return o;
            }

            o.X /= w;
            o.Y /= w;
            o.Z /= w;

            return o;
        }
    }
}
