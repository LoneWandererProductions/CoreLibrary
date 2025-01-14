/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Pathfinder
 * FILE:        Pathfinder/Pathfinding.cs
 * PURPOSE:     The actual Pathfinder algorithm
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    /// <summary>
    ///     Implementation of the A* pathfinder algorithm.
    /// </summary>
    public sealed class Pathfinding : IPathfinding
    {
        private readonly int _diagonalCost;

        private readonly int[,] _directions =
        {
            { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 }, // Right, Down, Left, Up
            { 1, 1 }, { 1, -1 }, { -1, -1 }, { -1, 1 } // Diagonals
        };

        private readonly int[,] _grid;
        private readonly int _straightCost;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Pathfinding" /> class.
        /// </summary>
        /// <param name="grid">The grid representing the environment.</param>
        /// <param name="straightCost">The cost of moving straight (optional).</param>
        /// <param name="diagonalCost">The cost of moving diagonally (optional).</param>
        public Pathfinding(int[,] grid, int straightCost = 1, int diagonalCost = 2)
        {
            _grid = grid;
            _straightCost = straightCost;
            _diagonalCost = diagonalCost;
        }


        /// <inheritdoc />
        /// <summary>
        ///     Finds the path from the start coordinates to the goal coordinates.
        /// </summary>
        /// <param name="startX">The start x-coordinate.</param>
        /// <param name="startY">The start y-coordinate.</param>
        /// <param name="goalX">The goal x-coordinate.</param>
        /// <param name="goalY">The goal y-coordinate.</param>
        /// <param name="rangeLimit">The maximum range limit for the path calculation.</param>
        /// <returns>
        ///     A <see cref="T:Pathfinder.PathfinderResults" /> object containing the path and other calculations.
        /// </returns>
        public PathfinderResults FindPath(int startX, int startY, int goalX, int goalY, int rangeLimit)
        {
            var result = new PathfinderResults();
            var openList = new SortedSet<Node>(new NodeComparer());
            var closedList = new HashSet<(int, int)>();
            var nodeIds = new Dictionary<Node, int>();
            var nodeIdCounter = 0;

            // Initialize start node with its movement cost
            var startNode = new Node(startX, startY, 0, Heuristic(startX, startY, goalX, goalY), _grid[startX, startY]);
            openList.Add(startNode);
            nodeIds[startNode] = nodeIdCounter++;

            while (openList.Count > 0)
            {
                var currentNode = openList.Min;
                openList.Remove(currentNode);

                if (currentNode.X == goalX && currentNode.Y == goalY)
                {
                    result.FullPath = BuildPath(currentNode);
                    result.LimitedRangePath = result.FullPath.Take(rangeLimit).ToList();
                    return result;
                }

                closedList.Add((currentNode.X, currentNode.Y));

                foreach (var neighbor in GetNeighbors(currentNode, _grid, goalX, goalY))
                {
                    if (closedList.Contains((neighbor.X, neighbor.Y)))
                    {
                        continue;
                    }

                    neighbor.H = Heuristic(neighbor.X, neighbor.Y, goalX, goalY);
                    neighbor.Parent = currentNode;

                    var openNode = openList.FirstOrDefault(n => n.X == neighbor.X && n.Y == neighbor.Y);
                    if (openNode == null)
                    {
                        openList.Add(neighbor);
                        nodeIds[neighbor] = nodeIdCounter++;
                    }
                    else if (neighbor.G < openNode.G)
                    {
                        openList.Remove(openNode);
                        openNode.G = neighbor.G;
                        openNode.Parent = currentNode;
                        openList.Add(openNode);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Builds the path from the goal node back to the start node.
        /// </summary>
        /// <param name="goalNode">The goal node.</param>
        /// <returns>A list of nodes representing the path from start to goal.</returns>
        private List<Node> BuildPath(Node goalNode)
        {
            var path = new List<Node>();
            var currentNode = goalNode;

            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            return path;
        }

        /// <summary>
        ///     Calculates the heuristic estimate of the cost from (x1, y1) to (x2, y2).
        /// </summary>
        /// <param name="x1">The x-coordinate of the first point.</param>
        /// <param name="y1">The y-coordinate of the first point.</param>
        /// <param name="x2">The x-coordinate of the second point.</param>
        /// <param name="y2">The y-coordinate of the second point.</param>
        /// <returns>The heuristic estimate of the cost.</returns>
        private static int Heuristic(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        /// <summary>
        ///     Gets the neighboring nodes for a given node based on the grid and movement costs.
        /// </summary>
        /// <param name="node">The node to get neighbors for.</param>
        /// <param name="grid">The grid representing the environment.</param>
        /// <returns>A list of neighboring nodes.</returns>
        private List<Node> GetNeighbors(Node node, int[,] grid, int goalX, int goalY)
        {
            var neighbors = new List<Node>();

            for (var i = 0; i < _directions.GetLength(0); i++)
            {
                var newX = node.X + _directions[i, 0];
                var newY = node.Y + _directions[i, 1];

                if (newX >= 0 && newX < grid.GetLength(0) && newY >= 0 && newY < grid.GetLength(1) &&
                    grid[newX, newY] != 0)
                {
                    var movementCost = grid[newX, newY];
                    var isDiagonal = Math.Abs(_directions[i, 0]) == 1 && Math.Abs(_directions[i, 1]) == 1;
                    var moveCost = isDiagonal ? _diagonalCost : _straightCost;

                    // Calculate new G cost for neighbor node
                    var newG = node.G + moveCost + movementCost;

                    // Create the neighbor node
                    var neighbor = new Node(newX, newY, newG, 0, movementCost, node)
                    {
                        // Set the H cost (heuristic) for the neighbor
                        H = Heuristic(newX, newY, goalX, goalY)
                    };

                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Compares two nodes for ordering in the open list.
        /// </summary>
        /// <seealso cref="T:System.Collections.Generic.IComparer`1" />
        public sealed class NodeComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                var result = (x.G + x.H).CompareTo(y.G + y.H);
                if (result == 0)
                {
                    result = x.G.CompareTo(y.G);
                    if (result == 0)
                    {
                        result = x.X.CompareTo(y.X);
                        if (result == 0)
                        {
                            result = x.Y.CompareTo(y.Y);
                        }
                    }
                }

                return result;
            }
        }
    }
}
