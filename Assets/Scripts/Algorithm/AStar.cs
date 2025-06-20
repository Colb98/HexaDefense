using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class AStar
{
    public static readonly (int dx, int dy)[] EvenNeighbors = {
        (-1,  0), (1,  0),
        (-1, -1), (0, -1),
        (-1,  1), (0,  1)
    };

    public static readonly (int dx, int dy)[] OddNeighbors = {
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

    // Find path from multiple start points to a single end point
    public static List<Vector2Int> FindPath(int[,] map, Vector2Int[] starts, Vector2Int end)
    {
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);


        var openSet = new SortedSet<Node>(Comparer<Node>.Create((a, b) =>
        {
            int comp = a.FCost.CompareTo(b.FCost);
            if (comp == 0)
                comp = a.HCost.CompareTo(b.HCost); // Tie-breaker
            if (comp == 0)
                comp = a.X.CompareTo(b.X); // Prevent duplicate keys
            if (comp == 0)
                comp = a.Y.CompareTo(b.Y);
            return comp;
        }));
        var openDict = new Dictionary<(int, int), Node>();
        var closedSet = new HashSet<(int, int)>();

        foreach (var start in starts)
        {
            if (start.x < 0 || start.y < 0 || start.x >= rows || start.y >= cols)
                continue;
            var node = new Node(start.x, start.y)
            {
                GCost = 0,
                HCost = Heuristic(start.x, start.y, end.x, end.y)
            };
            openSet.Add(node);
            openDict[(start.x, start.y)] = node;
        }

        Node current = null;

        while (openSet.Count > 0)
        {
            current = openSet.Min;
            openSet.Remove(current);
            openDict.Remove((current.X, current.Y));

            if (current.X == end.x && current.Y == end.y)
                break;

            closedSet.Add((current.X, current.Y));
            if (map[current.X, current.Y] == (int)TileType.WALL || map[current.X, current.Y] == (int)TileType.TOWER)
                continue;

            var neighbors = (current.Y % 2 == 0) ? EvenNeighbors : OddNeighbors;
            foreach (var (dx, dy) in neighbors)
            {
                int nx = current.X + dx;
                int ny = current.Y + dy;

                if (nx < 0 || ny < 0 || nx >= rows || ny >= cols)
                    continue;
                if (map[nx, ny] == (int)TileType.WALL || map[nx, ny] == (int)TileType.TOWER)
                    continue;
                if (closedSet.Contains((nx, ny)))
                    continue;

                float newGCost = current.GCost + 1;

                if (openDict.TryGetValue((nx, ny), out var neighbor))
                {
                    if (newGCost < neighbor.GCost)
                    {
                        openSet.Remove(neighbor);
                        neighbor.GCost = newGCost;
                        neighbor.Parent = current;
                        openSet.Add(neighbor);
                    }
                }
                else
                {
                    neighbor = new Node(nx, ny)
                    {
                        GCost = newGCost,
                        HCost = Heuristic(nx, ny, end.x, end.y),
                        Parent = current
                    };
                    openSet.Add(neighbor);
                    openDict[(nx, ny)] = neighbor;
                }
            }
        }

        // Không tìm thấy đường
        if (current == null || (current.X != end.x || current.Y != end.y))
        {
            //Debug.Log($"Path not found from {starts[0]} to {end}. Current {current}");
            return null;
        }

        // Truy vết đường đi
        List<Vector2Int> path = new List<Vector2Int>();
        while (current != null)
        {
            path.Add(new Vector2Int(current.X, current.Y));
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    // Không cùng chuẩn với hàm convert sang Cube coordinate của Tile, nhưng vẫn hoạt động được, keep ở đây
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

    private static float Heuristic(Node node1, Node node2)
    {
        return Tile.GetHexManhattanDistance(new Vector2Int(node1.X, node1.Y), new Vector2Int(node2.X, node2.Y));
    }

    private static float Heuristic(int x1, int y1, int x2, int y2)
    {
        return Tile.GetHexManhattanDistance(new Vector2Int(x1, y1), new Vector2Int(x2, y2));
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
