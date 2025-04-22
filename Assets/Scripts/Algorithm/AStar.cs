using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class AStar
{
    private static readonly (int dx, int dy)[] EvenNeighbors = {
        (-1,  0), (1,  0),
        (-1, -1), (0, -1),
        (-1,  1), (0,  1)
    };

    private static readonly (int dx, int dy)[] OddNeighbors = {
        (-1,  0), (1,  0),
        (0, -1), (1, -1),
        (0,  1), (1,  1)
    };


    public class Node
    {
        public int X, Y;
        public float GCost, HCost;
        public Node Parent;

        public float FCost => GCost + HCost;

        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Node node && X == node.X && Y == node.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        public override string ToString()
        {
            return $"Node({X}, {Y})";
        }
    }

    public static List<(int x, int y)> FindPath(int[,] map, (int x, int y) spawnPos)
    {
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);

        Node startNode = new Node(spawnPos.x, spawnPos.y);
        var goals = new List<Node>();
        var openSet = new List<Node> { startNode };
        var closedSet = new HashSet<Node>();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                if ((TileType)map[x, y] == TileType.GOAL)
                {
                    goals.Add(new Node(x, y));
                }
            }
        }

        while (openSet.Count > 0)
        {
            // Get node with lowest F cost
            Node current = openSet[0];
            foreach (var node in openSet)
            {
                if (node.FCost < current.FCost || (node.FCost == current.FCost && node.HCost < current.HCost))
                {
                    current = node;
                }
            }

            // If current is a GOAL
            if ((TileType)map[current.X, current.Y] == TileType.GOAL)
                return ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current);

            var directions = (current.Y % 2 == 0) ? EvenNeighbors : OddNeighbors;
            foreach (var (dx, dy) in directions)
            {
                int nx = current.X + dx;
                int ny = current.Y + dy;

                if (nx < 0 || ny < 0 || nx >= rows || ny >= cols)
                    continue;

                if ((TileType)map[nx, ny] == TileType.WALL || (TileType)map[nx, ny] == TileType.TOWER)
                    continue;

                Node neighbor = new Node(nx, ny);
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = current.GCost + 1;

                var openNode = openSet.Find(n => n.Equals(neighbor));
                if (openNode == null)
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = Heuristic(neighbor, goals);
                    neighbor.Parent = current;
                    openSet.Add(neighbor);
                }
                else if (tentativeGCost < openNode.GCost)
                {
                    openNode.GCost = tentativeGCost;
                    openNode.Parent = current;
                }
            }
        }

        // No path found
        return null;
    }

    private static float Heuristic(Node node, List<Node> goals)
    {
        // Use Manhattan distance to the nearest GOAL tile
        float minDist = float.MaxValue;

        int nodeX = node.X + node.Y;
        int nodeY = node.Y;
        int nodeZ = node.X;

        foreach (var goal in goals)
        {
            int goalX = goal.X + goal.Y;
            int goalY = goal.Y;
            int goalZ = goal.X;

            float dist = Math.Max(Math.Abs(goalX - nodeX), Math.Max(Math.Abs(goalY - nodeY), Math.Abs(goalZ - nodeZ)));
            if (dist < minDist)
                minDist = dist;
        }

        return minDist;
    }

    private static List<(int x, int y)> ReconstructPath(Node node)
    {
        var path = new List<(int x, int y)>();
        while (node != null)
        {
            path.Add((node.X, node.Y));
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }
}
