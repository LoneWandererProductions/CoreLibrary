using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        public void BasicStructure()
        {
            var objFile = ResourceObjects.GetCube();
            var poly = Triangle.CreateTri(objFile);
            var transform = new Transform();
            var renderObj = new RenderObject(poly, transform);
            var raster = new Rasterizer();
            var render = raster.Render(renderObj, false);

            var lst = Triangle.GetCoordinates(render);
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
