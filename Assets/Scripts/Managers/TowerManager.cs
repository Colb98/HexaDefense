using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class TowerPrefabEntry
{
    public string towerName;
    public Tower towerPrefab;
}

public class TowerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Map _map;
    [SerializeField] private List<TowerPrefabEntry> _towerPrefabs;

    [Header("Pool Settings")]
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private bool _expandPoolWhenEmpty = true;
    private int towerCount = 0;

    #region Grid Offsets

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

    #endregion

    // Object pool for towers
    private Dictionary<string, Queue<Tower>> _towerPool;
    // Active towers for tracking
    private HashSet<Tower> _activeTowers;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeTowerPool();
    }

    #endregion

    #region Pool Management

    /// <summary>
    /// Initializes the tower object pool with the specified initial size.
    /// </summary>
    private void InitializeTowerPool()
    {
        _towerPool = new Dictionary<string, Queue<Tower>>();
        _activeTowers = new HashSet<Tower>();

        foreach(var entry in _towerPrefabs)
        {
            _towerPool[entry.towerName] = new Queue<Tower>(_initialPoolSize);
            // Pre-warm the pool
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateTowerInPool(entry.towerName);
            }
        }
    }

    /// <summary>
    /// Creates a new tower instance and adds it to the pool.
    /// </summary>
    private Tower CreateTowerInPool(string name)
    {
        var prefab = _towerPrefabs.FirstOrDefault(entry => entry.towerName == name);
        Tower tower = Instantiate(prefab.towerPrefab);
        tower.gameObject.SetActive(false);
        _towerPool[name].Enqueue(tower);
        tower.SetMap(_map);
        return tower;
    }

    /// <summary>
    /// Gets a tower from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <returns>A tower instance from the pool.</returns>
    private Tower GetTowerFromPool(string type)
    {
        if (!_towerPool.ContainsKey(type))
        {
            Debug.LogError($"Tower type {type} not found in pool!");
            return null;
        }
        if (_towerPool[type].Count == 0)
        {
            if (_expandPoolWhenEmpty)
            {
                return CreateTowerInPool(type);
            }
            else
            {
                Debug.LogWarning("Tower pool is empty and set not to expand!");
            }
        }

        Tower tower = _towerPool[type].Dequeue();
        _activeTowers.Add(tower);
        tower.gameObject.SetActive(true);
        tower.name = $"{type}_{towerCount++}";
        tower.OnSpawn();
        return tower;
    }

    /// <summary>
    /// Returns a tower to the pool for reuse.
    /// </summary>
    /// <param name="tower">The tower to return to the pool.</param>
    public void ReturnTowerToPool(Tower tower)
    {
        if (_activeTowers.Remove(tower))
        {
            tower.gameObject.SetActive(false);
            _towerPool[tower.entityType].Enqueue(tower);
        }
        else
        {
            Debug.LogWarning("Attempted to return a tower that isn't tracked as active!");
        }
    }

    public void OnTowerDead(Tower tower)
    {
        RemoveTowerOccupied(tower);
    }

    public void RemoveTowerOccupied (Tower tower)
    {
        // This part only need to handle logic, not the pool
        var coords = GetNeighborCoordOfCenter(tower.position, tower.GetSize());
        foreach (var coord in coords)
        {
            var curTile = _map.GetTileAt(coord.x, coord.y);
            curTile.SetType(TileType.GROUND);
            _map.SetMapDataAt(coord.x, coord.y, TileType.GROUND);
        }
    }

    public void SellTower(Tower tower)
    {
        tower.OnRemoved();
        RemoveTowerOccupied(tower);
        GameManager.Instance.AddGold(tower.GetSellPrice());
    }

    public void UpgradeTower(Tower tower)
    {
        tower.Upgrade();
    }

    #endregion

    #region Tower Placement

    /// <summary>
    /// Checks if a tower can be placed at the given position.
    /// </summary>
    /// <param name="center">The center position to check.</param>
    /// <param name="size">The size of the tower (2 or 3).</param>
    /// <returns>True if the tower can be placed, false otherwise.</returns>
    public bool CanTowerBePlaced(Vector2Int center, int size)
    {
        if (size != 2 && size != 3)
        {
            Debug.LogError("Cannot place tower with size different than 2 or 3!");
            return false;
        }

        var centerType = _map.GetMapDataAt(center.x, center.y);
        if (centerType != TileType.GROUND && centerType != TileType.WALL && centerType != TileType.PATH)
        {
            return false;
        }

        var neighbors = size == 2 ?
            (center.y % 2 == 0 ? EvenOccupied2 : OddOccupied2) :
            (center.y % 2 == 0 ? EvenOccupied3 : OddOccupied3);

        foreach (var neighbor in neighbors)
        {
            int nx = center.x + neighbor.dx;
            int ny = center.y + neighbor.dy;

            // Check if within map bounds
            if (nx < 0 || nx >= _map.width || ny < 0 || ny >= _map.height)
            {
                return false;
            }

            var curType = _map.GetMapDataAt(nx, ny);
            if (!IsSameType(curType, centerType))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Places a tower at the given position using the object pool.
    /// </summary>
    /// <param name="center">The center position to place the tower.</param>
    /// <param name="size">The size of the tower.</param>
    /// <returns>The placed tower instance.</returns>
    public Tower PlaceTower(Vector2Int center, int size, string type, int level)
    {
        Tower tower = GetTowerFromPool(type);
        if (tower == null)
        {
            return null;
        }
        tower.transform.localPosition = Tile.GetTilePosition(_map.tileSize, center, _map.width, _map.height);

        var towerConfig = GameConfigManager.Instance.Towers[type];
        var levelConfig = towerConfig.levels.FirstOrDefault(lvl => lvl.level == level);
        tower.Initialize(size, center, type, levelConfig, towerConfig);
        tower.SetLevel(level);
        tower.transform.SetParent(_map.transform);
        return tower;
    }

    /// <summary>
    /// Determines if two tile types are compatible for tower placement.
    /// </summary>
    private bool IsSameType(TileType curType, TileType otherType)
    {
        if (curType == TileType.PATH && otherType == TileType.GROUND) return true;
        if (otherType == TileType.PATH && curType == TileType.GROUND) return true;
        return curType == otherType;
    }

    /// <summary>
    /// Gets all neighboring coordinates for a tower of specified size.
    /// </summary>
    /// <param name="center">The center position of the tower.</param>
    /// <param name="size">The size of the tower (2 or 3).</param>
    /// <returns>Array of coordinates that the tower occupies.</returns>
    public Vector2Int[] GetNeighborCoordOfCenter(Vector2Int center, int size)
    {
        if (size != 2 && size != 3)
        {
            Debug.LogError("Cannot get neighbors for tower with size different than 2 or 3!");
            return Array.Empty<Vector2Int>();
        }

        var offsets = size == 2 ?
            (center.y % 2 == 0 ? EvenOccupied2Offsets : OddOccupied2Offsets) :
            (center.y % 2 == 0 ? EvenOccupied3Offsets : OddOccupied3Offsets);

        Vector2Int[] result = new Vector2Int[offsets.Length];

        for (int i = 0; i < offsets.Length; i++)
        {
            result[i] = offsets[i] + center;
        }
        return result;
    }

    #endregion

    #region Getters
    public Tower[] GetTowers()
    {
        return _activeTowers.ToArray();
    }

    public Tower GetTowerAt(int x, int y)
    {
        Debug.Log("Getting tower at coordinates: " + x + ", " + y);
        foreach (var tower in _activeTowers)
        {
            var coords = GetNeighborCoordOfCenter(tower.position, 2);
            Debug.Log("Checking tower at " + tower.position + " for coords: " + string.Join(", ", coords.Select(c => c.ToString())));
            if (coords.Contains(new Vector2Int(x, y)))
            {
                return tower;
            }
        }
        Debug.LogWarning($"No tower found at coordinates: {x}, {y}");
        return null;
    }
    #endregion
}