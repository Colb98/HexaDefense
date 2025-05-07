using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using static UnityEngine.UI.Image;

public class TowerManager : MonoBehaviour
{
    [SerializeField] private Map map;

    private static readonly (int dx, int dy)[] EvenOccupied2 = {
        (0,  0), (-1,  0), (1,  0),
        (-1, -1), (0, -1),
        (-1,  1), (0,  1)
    };

    private static readonly (int dx, int dy)[] OddOccupied2 = {
        (0,  0), (-1,  0), (1,  0),
        (0, -1), (1, -1),
        (0,  1), (1,  1)
    };

    private static readonly (int dx, int dy)[] EvenOccupied3 = {
        (-2,  0), (-1,  0), (0,  0), (1,  0), (2,  0),
        (-2, -1), (-1, -1), (0, -1), (1, -1),
        (-2,  1), (-1,  1), (0,  1), (1,  1),
        (-1,  2), ( 0,  2), (1,  2),
        (-1, -2), ( 0, -2), (1, -2)
    };

    private static readonly (int dx, int dy)[] OddOccupied3 = {
        (-2,  0), (-1,  0), (0,  0), (1,  0), (2,  0),
        (-1, -1), ( 0, -1), (1, -1), (2, -1),
        (-1,  1), ( 0,  1), (1,  1), (2,  1),
        (-1,  2), ( 0,  2), (1,  2),
        (-1, -2), ( 0, -2), (1, -2)
    };

    private static readonly Vector2Int[] EvenOccupied2Offsets =
    EvenOccupied2.Select(p => new Vector2Int(p.dx, p.dy)).ToArray();

    private static readonly Vector2Int[] OddOccupied2Offsets =
    OddOccupied2.Select(p => new Vector2Int(p.dx, p.dy)).ToArray();

    private static readonly Vector2Int[] EvenOccupied3Offsets =
    EvenOccupied3.Select(p => new Vector2Int(p.dx, p.dy)).ToArray();

    private static readonly Vector2Int[] OddOccupied3Offsets =
    OddOccupied3.Select(p => new Vector2Int(p.dx, p.dy)).ToArray();

    /**
     * Check if tower can be placed at given position
     */
    public bool CanTowerBePlaced(Vector2Int center, int size)
    {
        if (size != 2 && size != 3)
        {
            Debug.LogError("Cannot Place Tower with size different than 2 or 3!");
            return false;
        }

        var centerType = map.GetMapDataAt(center.x, center.y);
        if (centerType != TileType.GROUND && centerType != TileType.WALL && centerType != TileType.PATH)
        {
            return false;
        }

        var neighbors = size == 2 ? (center.y % 2 == 0 ? EvenOccupied2 : OddOccupied2) : (center.y % 2 == 0 ? EvenOccupied3 : OddOccupied3);
        foreach (var neighbor in neighbors)
        {
            Debug.Log($"Neighbor {neighbor}");
            if (center.x + neighbor.dx < 0 || center.x + neighbor.dx >= map.width || center.y + neighbor.dy < 0 || center.y + neighbor.dy >= map.height)
            {
                Debug.Log("1");
                return false;
            }
            var curType = map.GetMapDataAt(center.x + neighbor.dx, center.y + neighbor.dy);
            if (!IsSameType(curType, centerType))
            {
                Debug.Log("2");
                return false; 
            }
        }
        return true;
    }

    bool IsSameType(TileType curType, TileType otherType) {
        if (curType == TileType.PATH && otherType == TileType.GROUND) return true;
        if (otherType == TileType.PATH && curType == TileType.GROUND) return true;
        return curType == otherType; 
    }

    public Vector2Int[] GetNeighborCoordOfCenter(Vector2Int center, int size)
    {

        if (size != 2 && size != 3)
        {
            Debug.LogError("Cannot Place Tower with size different than 2 or 3!");
            return Array.Empty<Vector2Int>();
        }
        var offsets = size == 2 ? (center.y % 2 == 0 ? EvenOccupied2Offsets : OddOccupied2Offsets) : (center.y % 2 == 0 ? EvenOccupied3Offsets : OddOccupied3Offsets);
        Vector2Int[] result = new Vector2Int[offsets.Length];

        for (int i = 0; i < offsets.Length; i++)
        {
            result[i] = offsets[i] + center;
        }
        return result;
    }
}
