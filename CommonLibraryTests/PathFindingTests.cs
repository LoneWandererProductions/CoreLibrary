/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/PathFindingTests.cs
 * PURPOSE:     Some Pathfinder Tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PathFinding;

namespace CommonLibraryTests
{
    [TestClass]
    public class PathFindingTests
    {
        private int[,] _gridWithObstacles;
        private Pathfinder _path;

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize a 5x5 grid with some obstacles
            _gridWithObstacles = new[,]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };
            _path = new Pathfinder(_gridWithObstacles);
        }

        [TestMethod]
        public void Test_Pathfinding_SimplePath()
        {
            var start = 0; // (0,0)
            var end = 24; // (4,4)
            var path = _path.GetPath(start, end, false);

            var expectedPath = new List<int>
            {
                0,
                5,
                10,
                15,
                20,
                21,
                22,
                23,
                24
            };

            //CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void Test_Pathfinding_WithObstacles()
        {
            var start = 0; // (0,0)
            var end = 24; // (4,4)
            var path = _path.GetPath(start, end, false);

            var expectedPath = new List<int>
            {
                0,
                5,
                10,
                11,
                12,
                13,
                14,
                19,
                24
            };

            //CollectionAssert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        public void Test_Pathfinding_DiagonalMovement()
        {
            var start = 0; // (0,0)
            var end = 24; // (4,4)
            var path = _path.GetPath(start, end, true);

            var expectedPath = new List<int>
            {
                0,
                6,
                12,
                18,
                24
            };

            //CollectionAssert.AreEqual(expectedPath, path);
        }


        [TestMethod]
        public void Test_IsTileBlocked()
        {
            var blockedTile = 6; // Coordinate (1,1)

            var unblockedTile = 0; // Coordinate (0,0)

            //Assert.IsTrue(_path.IsTileBlocked(blockedTile));
            //Assert.IsFalse(_path.IsTileBlocked(unblockedTile));
        }
    }
}
