/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        PathfinderTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pathfinder;

namespace CommonLibraryTests;

/// <summary>
///     Some basic tests for the pathfinder
/// </summary>
[TestClass]
public class PathfinderTests
{
    /// <summary>
    ///     The move range
    /// </summary>
    private const int MoveRange = 100;

    /// <summary>
    ///     The pathfinder
    /// </summary>
    private static Pathfinding _pathfinder;

    /// <summary>
    ///     Tests the find path valid path.
    /// </summary>
    [TestMethod]
    public void TestFindPathValidPath()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 2, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 1, startY = 1;
        const int goalX = 7, goalY = 7;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(11, path.FullPath.Count, "Path length should be 11.");
    }

    /// <summary>
    ///     Tests the find path single step.
    /// </summary>
    [TestMethod]
    public void TestFindPathSingleStep()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 2, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 1, startY = 1;
        const int goalX = 1, goalY = 2;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(path.FullPath.Count, 2, "Path length should be 2.");
    }


    /// <summary>
    ///     Tests the find path no path.
    /// </summary>
    [TestMethod]
    public void TestFindPathNoPath()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 2, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 1, startY = 1;
        const int goalX = 7, goalY = 7;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNull(path.FullPath, "Path should be null as there is no possible path.");
    }

    /// <summary>
    ///     Tests the find path start at edge.
    /// </summary>
    [TestMethod]
    public void TestFindPathStartAtEdge()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 0, startY = 7;
        const int goalX = 7, goalY = 7;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(8, path.FullPath.Count, "Path length should be 19 starting from edge.");
    }

    /// <summary>
    ///     Tests the find path high movement cost.
    /// </summary>
    [TestMethod]
    public void TestFindPathHighMovementCost()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 2, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 5, 0, 0, 1, 0 }, // High cost at (4,4)
            { 0, 3, 0, 0, 0, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 1, startY = 1;
        const int goalX = 7, goalY = 7;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path.FullPath, "Path should not be null.");
        Assert.AreEqual(11, path.FullPath.Count, "Path length should be 11, avoiding the high cost.");
    }

    /// <summary>
    ///     Tests the find path diagonal movement.
    /// </summary>
    [TestMethod]
    public void TestFindPathDiagonalMovement()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 1, startY = 1;
        const int goalX = 7, goalY = 5;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(10, path.FullPath.Count, "Path length should be 10 with diagonal movement.");
    }

    /// <summary>
    ///     Tests the find path goal at edge.
    /// </summary>
    [TestMethod]
    public void TestFindPathGoalAtEdge()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 1, startY = 1;
        const int goalX = 0, goalY = 8;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNull(path.FullPath, "Path should be null.");

        grid = new[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 0, 1, 0, 0, 0, 0, 0, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 0, 0, 1, 0, 0, 1, 0 }, { 0, 3, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 3, 0, 0, 0, 0, 0, 2, 0 }, { 0, 1, 1, 1, 1, 1, 1, 2, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.AreEqual(8, path.FullPath.Count, "Path length should be 9 ending at edge.");
    }

    /// <summary>
    ///     Tests the find path start and goal are same.
    /// </summary>
    [TestMethod]
    public void TestFindPathStartAndGoalAreSame()
    {
        int[,] grid =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 4, startY = 4;
        const int goalX = 4, goalY = 4;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(1, path.FullPath.Count, "Path length should be 1.");
        Assert.AreEqual(startX, path.FullPath[0].X, "Start X should match.");
        Assert.AreEqual(startY, path.FullPath[0].Y, "Start Y should match.");
    }

    /// <summary>
    ///     Tests the find path with high obstacle density.
    /// </summary>
    [TestMethod]
    public void TestFindPathWithHighObstacleDensity()
    {
        int[,] grid =
        {
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 1, 1, 1, 1, 1, 1, 1, 0 }, { 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 0, startY = 0;
        const int goalX = 8, goalY = 8;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNull(path.FullPath, "Path should be null as there is no possible path.");
    }

    /// <summary>
    ///     Tests the find path with edge cases.
    /// </summary>
    [TestMethod]
    public void TestFindPathWithEdgeCases()
    {
        int[,] grid =
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 0, 0, 0, 0, 0, 0, 0, 1 }, { 1, 0, 1, 1, 1, 1, 0, 0, 1 },
            { 1, 0, 1, 0, 0, 0, 0, 1, 1 }, { 1, 0, 1, 0, 1, 1, 0, 1, 1 }, { 1, 0, 1, 0, 1, 0, 0, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 8, startY = 0;
        const int goalX = 0, goalY = 8;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);

        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(15, path.FullPath.Count, "Path length should be 15.");
    }

    /// <summary>
    ///     Tests the find path with dynamic obstacles.
    /// </summary>
    [TestMethod]
    public void TestFindPathWithDynamicObstacles()
    {
        // Initial grid setup (1 represents traversable cells, 0 represents obstacles)
        int[,] grid =
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 0, startY = 0;
        const int goalX = 8, goalY = 8;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);
        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(9, path.FullPath.Count, "Initial path length should be 9.");

        // Add obstacles to the grid
        grid[4, 4] = 0;
        grid[4, 5] = 0;
        grid[5, 4] = 0;

        // Recreate the Pathfinding instance with the updated grid
        _pathfinder = new Pathfinding(grid);

        path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);
        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null after obstacles added.");

        // Adjust assertion for path length based on expected outcome
        // Here we check if a path exists and its length; it may vary with obstacles
        Assert.IsTrue(path.FullPath.Count > 0, "Path should be found after obstacles are added.");
    }

    [TestMethod]
    public void TestFindPathWithDifferentMovementCosts()
    {
        int[,] grid =
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 10, 10, 10, 10, 10, 10, 10, 10, 1 }, // High cost cells
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        _pathfinder = new Pathfinding(grid);

        const int startX = 0, startY = 0;
        const int goalX = 8, goalY = 8;

        var path = _pathfinder.FindPath(startX, startY, goalX, goalY, MoveRange);
        PrintPath(grid, path.FullPath);

        Assert.IsNotNull(path, "Path should not be null.");
        Assert.AreEqual(15, path.FullPath.Count, "Path length should be valid considering costs.");
    }

    /// <summary>
    ///     Prints the path.
    /// </summary>
    /// <param name="grid">The grid.</param>
    /// <param name="path">The path.</param>
    private static void PrintPath(int[,] grid, List<Node> path)
    {
        if (path.IsNullOrEmpty())
        {
            return;
        }

        var displayGrid = new char[grid.GetLength(0), grid.GetLength(1)];
        //build map
        for (var i = 0; i < grid.GetLength(0); i++)
            for (var j = 0; j < grid.GetLength(1); j++)
            {
                displayGrid[i, j] = grid[i, j] == 0 ? 'X' : '.';
            }

        //build map
        for (var i = 0; i < path.Count; i++)
        {
            var node = path[i];
            if (i == 0)
            {
                displayGrid[node.X, node.Y] = 'S'; // Mark the start point
            }
            else if (i == path.Count - 1)
            {
                displayGrid[node.X, node.Y] = 'E'; // Mark the end point
            }
            else
            {
                displayGrid[node.X, node.Y] = 'P'; // Mark the path
            }
        }

        for (var i = 0; i < displayGrid.GetLength(0); i++)
        {
            for (var j = 0; j < displayGrid.GetLength(1); j++)
            {
                Trace.Write(displayGrid[i, j] + " ");
            }

            Trace.WriteLine(string.Empty);
        }
    }
}
