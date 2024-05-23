/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/PathFindingTests.cs
 * PURPOSE:     Some Pathfinder Tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlayGround;

namespace CommonLibraryTests
{
    [TestClass]
    public class PathFindingTests
    {
        private int[,] _gridWithObstacles = new[,]
                                        {
                                            { 0, 0, 0, 0, 0, 0 },
                                            { 0, 1, 1, 1, 1, 0 },
                                            { 0, 1, 0, 0, 0, 0 },
                                            { 0, 1, 0, 0, 0, 0 },
                                            { 0, 2, 1, 1, 2, 0 },
                                            { 0, 0, 0, 0, 0, 0 }
                                        };

        [TestMethod]
        public void Test_Pathfinding_SimplePath()
        {

            PathFindingNew.SetData(_gridWithObstacles);
            var (path, cost) = PathFindingNew.GetPath(1, 1, 4, 4, false);

            Trace.WriteLine("Path: " + string.Join(", ", path));
            Trace.WriteLine("Total Cost: " + cost);
        }
    }
}
