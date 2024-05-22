/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/PathFinding.cs
 * PURPOSE:     Path-finding class with the A* Algorithm
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * Source:      https://www.dotnetperls.com/maze
 * Source:      https://github.com/roy-t/AStar/blob/master/Roy-T.AStar/PathFinder.cs
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PathFinding
{
    /// <summary>
    /// Path-finding with the A* Algorithm
    /// Only used by EventEngine
    /// TODO Implement Pathfinder add Diagonal move points, this must be implemented in the TileBorders
    /// TODO Implement Pathfinder add weighted Terrain
    /// </summary>
    internal static class PathFinding
    {
        /// <summary>
        /// The border array.
        /// </summary>
        private static int[,] _borderArray;

        /// <summary>
        /// The grid, aka PathFinding Nodes.
        /// </summary>
        private static PathNode[,] _gridArray;

        /// <summary>
        /// The height of the Map.
        /// </summary>
        private static int _height;

        /// <summary>
        /// The length of the Map.
        /// </summary>
        private static int _length;

        /// <summary>
        /// Initiate the Engine
        /// </summary>
        /// <param name="borders">Border Map</param>
        /// <param name="height">Height of the Map</param>
        /// <param name="length">Length of the Map</param>
        internal static void SetData(int[,] borders, int height, int length)
        {
            //start Data
            _borderArray = borders;
            _height = height;
            _length = length;
        }

        /// <summary>
        /// Checks if Node is accessible
        /// </summary>
        /// <param name="targetCoordinateId">Target Node</param>
        /// <remarks>Boolean</remarks>
        /// <returns>false or true</returns>
        internal static bool IsTileBlocked(int targetCoordinateId)
        {
            var checkTile = PathProcessing.IdToCoordinate(targetCoordinateId, _length);
            var point = PathProcessing.GetPoint(checkTile);
            return _borderArray[point.XRow, point.YColumn] != 0;
        }

        /// <summary>
        /// Return's clean
        /// </summary>
        /// <param name="start">Start Coordinates(transformed)</param>
        /// ///
        /// <param name="end">End Coordinates(transformed)</param>
        /// <param name="diagonal"></param>
        /// <remarks>List of Coordinates gets checked against the path of Nodes</remarks>
        /// <returns>Returns the Path in Coordinates(transformed)</returns>
        internal static List<int> GetPath(int start, int end, bool diagonal)
        {
            //Initiate Internal Heuristic List
            _gridArray = PathProcessing.GetMap(_borderArray);

            //Internal Use to communicate
            var imageKeys = PathProcessing.GetCoordinateKeys(_height, _length);

            var cStart = PathProcessing.IdToCoordinate(start, _length);
            var cEnd = PathProcessing.IdToCoordinate(end, _length);

            cStart = PathProcessing.GetPoint(cStart);
            cEnd = PathProcessing.GetPoint(cEnd);
            //untransformed
            var pathway = FindPath(cStart, cEnd, diagonal);
            //(transformed)
            return PathProcessing.GetPath(pathway, imageKeys, _length);
        }

        /// <summary>
        /// A* Algorithm finds path from starting point to target
        /// </summary>
        /// <remarks>Nodes have to transformed in Coordinates to display the movement</remarks>
        /// <returns>Path as Nodes</returns>
        [return: MaybeNull]
        private static IEnumerable<PathNode> FindPath(MovePoint start, MovePoint end, bool diagonal)
        {
            //Clean from our Last Run
            CleanPath();

            var possibleList = new List<PathNode>(); //Nodes to be considered, ones that may be on the path
            var exploredList = new List<PathNode>(); //Explored nodes

            var currentPoint = new MovePoint(0, 0);
            PathNode current;

            //Add the starting point to OpenList
            possibleList.Add(_gridArray[start.XRow, start.YColumn]);

            while (possibleList.Count > 0)
            {
                //Explores for the expected optimal choice in the OpenList
                current = possibleList[0];

                currentPoint.XRow = current.XNodeRow;
                currentPoint.YColumn = current.YNodeColumn;

                if (currentPoint.Equals(end))
                {
                    break; //If we stand on the target node return. No need to continue
                }

                possibleList.RemoveAt(0); //Removes the starting point from the possible Node Queue
                exploredList.Add(current); //Add Start-point, since we stand on it.

                //Checks all the squares adjacent to the current point
                //Skips explored nodes
                //Skips the node if it is blocked, thanks to Linq all in the loop
                foreach (var neighbor in GetValidNeighbors(currentPoint, diagonal))
                {
                    if (exploredList.Contains(neighbor) || neighbor.IsWall)
                    {
                        continue;
                    }

                    //Decision tree how we progress
                    //Check new possible Node
                    int stepCost = (neighbor.XNodeRow != current.XNodeRow && neighbor.YNodeColumn != current.YNodeColumn) ? 14 : 10; // 14 for diagonal, 10 for straight
                    if (neighbor.Parent == null)
                    {
                        neighbor.Parent = current; //Where it came from, final path can be found by linking parents
                        neighbor.Cost = current.Cost + stepCost;
                        //10 is the cost for each horizontal or vertical node moved, for now, optional -> " * option.Cost". if we want to add specific weight to the tiles, diagonal costs should considered as well

                        //Calculating the Heuristic value, ignoring any obstacles
                        neighbor.Heuristic = ManhattanDistance(neighbor, end);

                        //Calculates total cost by combining the X distance by the Y
                        neighbor.Heuristic *= 10; //Then multiply H by 10 (The cost movement for each square)
                        possibleList.Add(neighbor);
                    }
                    //Node was already visited
                    else
                    {
                        //Is this a more efficient route than last time?
                        if (current.Cost + stepCost >= neighbor.Cost)
                        {
                            continue;
                        }

                        //else replace with new Node
                        neighbor.Parent = current; //Where it came from, final path can be found by linking parents
                        neighbor.Cost = current.Cost + stepCost;
                        //10 is the cost for each horizontal or vertical node moved, for now, optional -> " * option.Cost". if we want to add specific weight to the tiles, diagonal costs should considered as well
                    }

                    //This uses the IComparable interface and CompareWith() method to sort
                    possibleList.Sort();
                }
            }

            //Convert Result
            var pathway = new List<PathNode>();

            ////bail should not happen if files are loaded correct, if we load a wrong map, this might actual be possible, but we should still catch it
            if (end.XRow > (_length * 3) - 2)
            {
                throw new Exception(string.Concat(PathResources.ErrorLength, _length));
            }

            if (end.YColumn > (_height * 3) - 2)
            {
                throw new Exception(string.Concat(PathResources.ErrorHeight, _height));
            }

            current = _gridArray[end.XRow, end.YColumn]; //Current = end designation node

            pathway.Add(current);

            while (current.Parent != null)
            {
                pathway.Add(current.Parent);
                current = current.Parent;
            }

            //We added from the end, so reverse it
            pathway.Reverse();

            return pathway;
        }

        /// <summary>
        /// Manhattan method
        /// ignores any obstacles
        /// Calculating the Heuristic value
        /// </summary>
        /// <param name="neighbor">Neighbor Tile</param>
        /// <param name="end">Target Tile</param>
        /// <returns>Calculated Distance</returns>
        private static int ManhattanDistance(PathNode neighbor, MovePoint end)
        {
            return Math.Abs(neighbor.XNodeRow - end.XRow) + Math.Abs(neighbor.YNodeColumn - end.YColumn);
        }

        /// <summary>
        /// Cleans the Elements of the Node Array [,] Grid
        /// </summary>
        /// <remarks>Nulls every single Parent element of Grid</remarks>
        private static void CleanPath()
        {
            //Resets all the parent variables to clear the paths created last time
            for (var x = 0; x <= _gridArray.GetUpperBound(0); x++)
            {
                for (var y = 0; y <= _gridArray.GetUpperBound(1); y++)
                {
                    _gridArray[x, y].Parent = null;
                }
            }
        }

        /// <summary>
        /// Here we can add the other Directions
        /// Get's all Neighbors of a point
        /// Now we just use 4 directions
        /// If we want edged add 4 extra Checks,see description.
        /// </summary>
        /// <param name="point">Point Coordinates(untransformed)</param>
        /// <param name="diagonal">Do we use diagonal directions</param>
        /// <remarks>Point and all Neighbors</remarks>
        /// <returns>Returns List of Nodes</returns>
        private static IEnumerable<PathNode> GetValidNeighbors(MovePoint point, bool diagonal)
        {
            var cache = new List<PathNode>();

            //     NW,N,NE,
            //      W,X,E
            //     SW,S,SE
            var maxLength = _gridArray.GetUpperBound(0);
            var maxHeight = _gridArray.GetUpperBound(1);

            //W
            if (point.XRow - 1 >= 0)
            {
                cache.Add(_gridArray[point.XRow - 1, point.YColumn]);
            }

            //E
            if (point.XRow < maxLength)
            {
                cache.Add(_gridArray[point.XRow + 1, point.YColumn]);
            }

            //S
            if (point.YColumn - 1 >= 0)
            {
                cache.Add(_gridArray[point.XRow, point.YColumn - 1]);
            }

            //N
            if (point.YColumn < maxHeight)
            {
                cache.Add(_gridArray[point.XRow, point.YColumn + 1]);
            }

            //Conditional diagonal steps
            if (!diagonal)
            {
                return cache;
            }

            //NE
            if (point.XRow < maxLength && point.YColumn < maxHeight)
            {
                cache.Add(_gridArray[point.XRow + 1, point.YColumn + 1]);
            }

            //NW
            if (point.XRow - 1 >= 0 && point.YColumn < maxHeight)
            {
                cache.Add(_gridArray[point.XRow - 1, point.YColumn + 1]);
            }

            //SE
            if (point.XRow < maxLength && point.YColumn - 1 >= 0)
            {
                cache.Add(_gridArray[point.XRow + 1, point.YColumn - 1]);
            }

            //SW
            if (point.XRow - 1 >= 0 && point.YColumn - 1 >= 0)
            {
                cache.Add(_gridArray[point.XRow - 1, point.YColumn - 1]);
            }

            return cache;
        }
    }
}
