/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/PathProcessing.cs
 * PURPOSE:     Helper Class to operate and convert Results of our Path calculations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;

namespace PathFinding
{
    /// <summary>
    ///     The path processing class.
    /// </summary>
    internal static class PathProcessing
    {
        /// <summary>
        ///     Transforms coordinates to Nodes
        ///     Necessary since One Cell has a height and length of 3 but is handled as  1
        /// </summary>
        /// <param name="newPoint">Coordinate Like Point</param>
        /// <returns>Coordinates to Nodes</returns>
        internal static MovePoint GetPoint(MovePoint newPoint)
        {
            var point = new MovePoint();
            var playerYColumn = 1;

            if (newPoint.Ycolumn != 0)
            {
                playerYColumn = (newPoint.Ycolumn * 3) + 1;
            }

            var playerXRow = 1;

            if (newPoint.Xrow != 0)
            {
                playerXRow = (newPoint.Xrow * 3) + 1;
            }

            point.Xrow = playerXRow;
            point.Ycolumn = playerYColumn;
            return point;
        }

        /// <summary>
        ///     Generates a List of all Coordinates based on height and length
        ///     Simple Container to convert Nodes into Coordinates
        ///     First is 0 next is 4, 7, 10, etc ... Stands for the Coordinate in the middle
        ///     The list is used and compared to the Nodes in the List that describes the possible movement path
        /// </summary>
        /// <param name="length">length of map</param>
        /// <param name="height">height of map</param>
        /// <returns>Coordinate Ids</returns>
        internal static List<int> GetCoordinateKeys(int length, int height)
        {
            var x = height >= length
                ? height
                : length;

            var coordinateKeys = new List<int>();
            for (var i = 0; i < x; i++)
            {
                coordinateKeys.Add((i * 3) + 1);
            }

            return coordinateKeys;
        }

        /// <summary>
        ///     Loads the Border List as the Map
        ///     0 is a possible path anything else is a Border and impassable
        /// </summary>
        /// <param name="borderMap">Map of Borders</param>
        /// <returns>Node for the PathFinding Engine</returns>
        internal static PathNode[,] GetMap(int[,] borderMap)
        {
            var grid = new PathNode[borderMap.GetUpperBound(0), borderMap.GetUpperBound(1)];

            for (var x = 0; x < borderMap.GetUpperBound(0); x++)
            {
                for (var y = 0; y < borderMap.GetUpperBound(1); y++)
                {
                    grid[x, y] = new PathNode
                    {
                        Parent = null,
                        XNodeRow = x,
                        YNodeColumn = y,
                        Cost = 0,
                        Heuristic = 0,
                        IsWall = borderMap[x, y] != 0
                    };
                }
            }

            return grid;
        }

        /// <summary>
        ///     Cycles though the Path from Nodes and Compares it to a List of Coordinate
        ///     List of Coordinates gets checked against the path of Nodes
        /// </summary>
        /// <param name="path">List of Coordinates</param>
        /// <param name="imageKeys">Simple Container to convert Nodes into Coordinates</param>
        /// <param name="length">length of map</param>
        /// <returns>Returns the Path in Coordinates</returns>
        internal static List<int> GetPath(IEnumerable<PathNode> path, List<int> imageKeys, int length)
        {
            return (from nodes in path
                    where imageKeys.Contains(nodes.XNodeRow)
                    where imageKeys.Contains(nodes.YNodeColumn)
                    select CalculateId(imageKeys.IndexOf(nodes.XNodeRow), imageKeys.IndexOf(nodes.YNodeColumn), length))
                .ToList();
        }

        /// <summary>
        ///     Example:
        ///     0,1,2,3
        ///     0,  0,1,2,3
        ///     1,  4,5,6,7
        ///     2,  8,9,10,11
        /// </summary>
        /// <param name="xRow">X Coordinate on Map</param>
        /// <param name="yColumn">Y Coordinate on Map</param>
        /// <param name="length">length of the Map</param>
        /// <returns>Fitting id of the Coordinate</returns>
        private static int CalculateId(int xRow, int yColumn, int length)
        {
            return (yColumn * length) + xRow;
        }

        /// <summary>
        ///     Convert to Coordinate
        /// </summary>
        /// <param name="masterId">Id of Master</param>
        /// <param name="length">length of Map</param>
        /// <returns>Id to Coordinate Like format</returns>
        public static MovePoint IdToCoordinate(int masterId, int length)
        {
            var modulo = masterId % length;
            var yColumn = masterId / length;

            return new MovePoint { Xrow = modulo, Ycolumn = yColumn };
        }
    }
}
