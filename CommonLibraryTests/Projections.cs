using System;
using System.Collections.Generic;
using DataFormatter;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class Projections
    {
        /// <summary>
        /// Test the Obj Loader
        /// </summary>
        [TestMethod]
        public void MeshLoader()
        {
            var obj = ResourceObjects.GetCube();
            var poly = Triangle.CreateTri(obj);

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
            const double fAspectRatio = (double)screenHeight / screenWidth;
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
        }

        [TestMethod]
        public void BasicStructure()
        {
            const int screenHeight = 480;
            const int screenWidth = 640;


            Projection3DRegister.Width = screenWidth;
            Projection3DRegister.Height = screenHeight;

            var objFile = ResourceObjects.GetCube();
            var poly = Triangle.CreateTri(objFile);
            var transform = new Transform();
            var renderObj = new RenderObject(poly, transform);
            var raster = new Rasterize();
            var render = raster.Render(renderObj, false);

            var first = render[0];
            var second = render[11];

            Assert.AreEqual(0, Math.Round(first[0].X,2), "Failed");
            Assert.AreEqual(0, Math.Round(first[0].Y, 2), "Failed");
            Assert.AreEqual(-0.10, Math.Round(first[0].Z, 2), "Failed");

            Assert.AreEqual(0, Math.Round(first[1].X, 2), "Failed");
            Assert.AreEqual(19.08, Math.Round(first[1].Y, 2), "Failed");
            Assert.AreEqual(-0.91, Math.Round(first[1].Z, 2), "Failed");

            Assert.AreEqual(14.33, Math.Round(first[2].X, 2), "Failed");
            Assert.AreEqual(19.08, Math.Round(first[2].Y, 2), "Failed");
            Assert.AreEqual(-0.91, Math.Round(first[2].Z, 2), "Failed");

            Assert.AreEqual(0.75, Math.Round(second[0].X, 2), "Failed");
            Assert.AreEqual(-0.05, Math.Round(second[0].Y, 2), "Failed");
            Assert.AreEqual(0.9, Math.Round(second[0].Z, 2), "Failed");

            Assert.AreEqual(0, Math.Round(second[1].X, 2), "Failed");
            Assert.AreEqual(0, Math.Round(second[1].Y, 2), "Failed");
            Assert.AreEqual(-0.10, Math.Round(second[1].Z, 2), "Failed");

            Assert.AreEqual(0.75, Math.Round(second[2].X, 2), "Failed");
            Assert.AreEqual(0, Math.Round(second[2].Y, 2), "Failed");
            Assert.AreEqual(-0.1, Math.Round(second[2].Z, 2), "Failed");

            var lst = Triangle.GetCoordinates(render);
        }

        /// <summary>
        ///     Multiplies the matrix vector.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="matProj">The mat proj.</param>
        /// <returns>The Projection Vecctor</returns>
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


    /// <summary>
    /// Load a cube
    /// </summary>
    internal static class ResourceObjects
    {
        /// <summary>
        ///     Gets the cube.
        /// </summary>
        /// <returns>Cube Object</returns>
        internal static List<TertiaryVector> GetCube()
        {
            //south two Triangles
            var southOne = new TertiaryVector { X = 0, Y = 0, Z = 0 };
            var southTwo = new TertiaryVector { X = 0, Y = 1, Z = 0 };
            var southThree = new TertiaryVector { X = 1, Y = 1, Z = 0 };

            //south two Triangles
            var southFour = new TertiaryVector { X = 0, Y = 0, Z = 0 };
            var southFive = new TertiaryVector { X = 1, Y = 1, Z = 0 };

            var southSix = new TertiaryVector { X = 1, Y = 0, Z = 0 };

            //east two Triangles
            var eastOne = new TertiaryVector { X = 1, Y = 0, Z = 0 };
            var eastTwo = new TertiaryVector { X = 1, Y = 1, Z = 0 };

            var eastThree = new TertiaryVector { X = 1, Y = 1, Z = 1 };

            //east two Triangles
            var eastFour = new TertiaryVector { X = 1, Y = 0, Z = 0 };
            var eastFive = new TertiaryVector { X = 1, Y = 1, Z = 1 };

            var eastSix = new TertiaryVector { X = 1, Y = 0, Z = 1 };

            //north two Triangles
            var northOne = new TertiaryVector { X = 1, Y = 0, Z = 1 };
            var northTwo = new TertiaryVector { X = 1, Y = 1, Z = 1 };

            var northThree = new TertiaryVector { X = 0, Y = 1, Z = 1 };

            var northFour = new TertiaryVector { X = 1, Y = 0, Z = 1 };
            var northFive = new TertiaryVector { X = 0, Y = 1, Z = 1 };

            var northSix = new TertiaryVector { X = 0, Y = 0, Z = 1 };


            //west two Triangles
            var westOne = new TertiaryVector { X = 0, Y = 0, Z = 1 };
            var westTwo = new TertiaryVector { X = 0, Y = 1, Z = 1 };

            var westThree = new TertiaryVector { X = 0, Y = 1, Z = 0 };

            var westFour = new TertiaryVector { X = 0, Y = 0, Z = 1 };
            var westFive = new TertiaryVector { X = 0, Y = 1, Z = 0 };

            var westSix = new TertiaryVector { X = 0, Y = 0, Z = 0 };

            //top two Triangles
            var topOne = new TertiaryVector { X = 0, Y = 1, Z = 0 };
            var topTwo = new TertiaryVector { X = 0, Y = 1, Z = 1 };

            var topThree = new TertiaryVector { X = 1, Y = 1, Z = 1 };

            var topFour = new TertiaryVector { X = 0, Y = 1, Z = 0 };
            var topFive = new TertiaryVector { X = 1, Y = 1, Z = 1 };

            var topSix = new TertiaryVector { X = 1, Y = 1, Z = 0 };

            //bottom two Triangles
            var bottomOne = new TertiaryVector { X = 1, Y = 0, Z = 1 };
            var bottomTwo = new TertiaryVector { X = 0, Y = 0, Z = 1 };

            var bottomThree = new TertiaryVector { X = 0, Y = 0, Z = 0 };

            var bottomFour = new TertiaryVector { X = 1, Y = 0, Z = 1 };
            var bottomFive = new TertiaryVector { X = 0, Y = 0, Z = 0 };

            var bottomSix = new TertiaryVector { X = 1, Y = 0, Z = 0 };

            return new List<TertiaryVector>
            {
                southOne,
                southTwo,
                southThree,
                southFour,
                southFive,
                southSix,
                eastOne,
                eastTwo,
                eastThree,
                eastFour,
                eastFive,
                eastSix,
                northOne,
                northTwo,
                northThree,
                northFour,
                northFive,
                northSix,
                westOne,
                westTwo,
                westThree,
                westFour,
                westFive,
                westSix,
                topOne,
                topTwo,
                topThree,
                topFour,
                topFive,
                topSix,
                bottomOne,
                bottomTwo,
                bottomThree,
                bottomFour,
                bottomFive,
                bottomSix
            };
        }
    }
}
