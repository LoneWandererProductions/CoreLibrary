using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayGround
{
    public static class PathFindingNew
    {
        private static int[,] _gridWithObstacles;

        public static void SetData(int[,] gridWithObstacles)
        {
            _gridWithObstacles = gridWithObstacles;
        }

        public static (List<int> path, int cost) GetPath(int startX, int startY, int endX, int endY, bool allowDiagonal)
        {
            var start = new MovePoint(startX, startY);
            var end = new MovePoint(endX, endY);

            var pathNodes = FindPath(start, end, allowDiagonal);

            if (pathNodes == null)
            {
                return (new List<int>(), int.MaxValue); // No path found
            }

            var path = pathNodes.Select(node => GetTileId(node.X, node.Y)).ToList();
            var totalCost = pathNodes.Sum(node => _gridWithObstacles[node.X, node.Y] == 2 ? 2 : 1);
            return (path, totalCost);
        }

        private static List<PathNode> FindPath(MovePoint start, MovePoint end, bool allowDiagonal)
        {
            var openList = new List<PathNode>();
            var closedList = new HashSet<PathNode>();

            var startNode = new PathNode(start.X, start.Y);
            var endNode = new PathNode(end.X, end.Y);
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                var currentNode = openList.OrderBy(node => node.F).First();
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.Equals(endNode))
                {
                    return RetracePath(startNode, currentNode);
                }

                foreach (var neighbor in GetNeighbors(currentNode, allowDiagonal))
                {
                    if (closedList.Contains(neighbor) || _gridWithObstacles[neighbor.X, neighbor.Y] == 0)
                    {
                        continue;
                    }

                    var newMovementCostToNeighbor = currentNode.G + GetDistance(currentNode, neighbor);
                    if (newMovementCostToNeighbor < neighbor.G || !openList.Contains(neighbor))
                    {
                        neighbor.G = newMovementCostToNeighbor;
                        neighbor.H = GetDistance(neighbor, endNode);
                        neighbor.Parent = currentNode;

                        if (!openList.Contains(neighbor))
                        {
                            openList.Add(neighbor);
                        }
                    }
                }
            }

            return null; // No path found
        }

        private static List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
        {
            var path = new List<PathNode>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            path.Reverse();
            return path;
        }

        private static int GetTileId(int x, int y)
        {
            return x * _gridWithObstacles.GetLength(1) + y;
        }

        private static IEnumerable<PathNode> GetNeighbors(PathNode node, bool allowDiagonal)
        {
            var neighbors = new List<PathNode>();
            var directions = new List<(int, int)>
            {
                (-1, 0), (1, 0), (0, -1), (0, 1)
            };

            if (allowDiagonal)
            {
                directions.AddRange(new List<(int, int)>
                {
                    (-1, -1), (-1, 1), (1, -1), (1, 1)
                });
            }

            foreach (var (dx, dy) in directions)
            {
                var x = node.X + dx;
                var y = node.Y + dy;

                if (x >= 0 && x < _gridWithObstacles.GetLength(0) && y >= 0 && y < _gridWithObstacles.GetLength(1))
                {
                    neighbors.Add(new PathNode(x, y));
                }
            }

            return neighbors;
        }

        private static int GetDistance(PathNode a, PathNode b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return dx + dy;
        }
    }
}
