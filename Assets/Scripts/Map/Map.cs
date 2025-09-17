using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Map : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 10;
    public int height = 10;
    public GameObject tilePrefab;
    public float tileSize = 1f;
    public Transform mapTransform;

    [Header("Tile Pool (Read-Only)")]
    [SerializeField] private List<GameObject> tilePool = new List<GameObject>();

    private int[,] mapData;
    private Tile[,] tiles;
    [SerializeField] private TowerManager towerManager;

    [SerializeField] private UnitManager unitManager;

    [SerializeField] private AttackManager projectileManager;

    public TowerManager TowerManager => towerManager;   
    public UnitManager UnitManager => unitManager;
    public AttackManager ProjectileManager => projectileManager;

    private int[,] distanceToEnd;
    private List<Vector2Int> endTiles = new List<Vector2Int>();

    public TowerManager GetTowerManager()
    {
        return towerManager;
    }

    public void SetTowerManager(TowerManager value)
    {
        towerManager = value;
    }

    public UnitManager GetUnitManager()
    {
        return unitManager;
    }

    public void SetUnitManager(UnitManager value)
    {
        unitManager = value;
    }

    private void Start()
    {
        LoadMapData();
        InitDistanceToEnd();
        GenerateHexMapWithPooling();
        ComputeDistanceToEnd();
    }

    private void Update()
    {

    }

    private void InitDistanceToEnd()
    {
        distanceToEnd = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                distanceToEnd[x, y] = -1;
    }

    public TileType GetMapDataAt(int x, int y)
    {
        return (TileType) mapData[x, y];
    }

    private void LoadMapData()
    {
        TextAsset mapTextAsset = Resources.Load<TextAsset>("map"); // Looks for Resources/map.txt

        if (mapTextAsset != null)
        {
            string[] lines = mapTextAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            height = lines.Length;
            width = lines[0].Length;

            mapData = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                string line = lines[y];
                for (int x = 0; x < width; x++)
                {
                    mapData[x, y] = Mathf.Clamp(line[x] - '0', 0, 4); // Ensure value stays within [0, 4]
                    if (mapData[x, y] == (int)TileType.GOAL)
                    {
                        endTiles.Add(new Vector2Int(x, y));
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No map.txt found in Resources. Generating default blank map.");
            mapData = new int[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    mapData[x, y] = 0;
        }
    }

    private void HandleTileClicked(Tile tile)
    {
        //Debug.Log($"Map received tile click: {tile.name} at position {tile.transform.position}");
        bool mapChanged = false;
        bool needUpdatePaths = false;

        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if (TowerPlacementManager.Instance.PlaceTower(tile))
            {
                needUpdatePaths = true;
                var center = new Vector2Int(tile.x, tile.y);
                var coords = towerManager.GetNeighborCoordOfCenter(center, 2);
                foreach (var coord in coords)
                {
                    var curTile = tiles[coord.x, coord.y];
                    curTile.SetType(TileType.TOWER);
                    mapData[curTile.x, curTile.y] = (int)TileType.TOWER;
                }
            }
            else if (tile.type == TileType.TOWER)
            {
                var ui = FindFirstObjectByType<GameUI>();
                Tower tower = towerManager.GetTowerAt(tile.x, tile.y);
                if (tower)
                {
                    Debug.Log($"Tower clicked: {tower.name} at position {tower.transform.position}");
                    ui.ShowTowerHud(tower, tower.transform.position);

                    tower.ShowAttackRange();
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            mapChanged = true;
            needUpdatePaths = true;
            tile.SetType(TileType.GROUND);
            mapData[tile.x, tile.y] = (int)TileType.GROUND;
        }

        if (mapChanged) {
            ComputeDistanceToEnd();
        }
        if (needUpdatePaths)
        {
            GetUnitManager().UpdateUnitPaths();
        }
    }

    public void OnTowerDead(Tower tower)
    {
        towerManager.OnTowerDead(tower);
        //unitManager.UpdateUnitPaths();
    }

    public int[,] GetMapData() { return mapData; }

    public Entity[] GetAllEntities()
    {
        Unit[] units = GetUnitManager().GetEntities();
        Tower[] towers = GetTowerManager().GetTowers();
        Entity[] entities = new Entity[units.Length + towers.Length];
        Array.Copy(units, 0, entities, 0, units.Length);
        Array.Copy(towers, 0, entities, units.Length, towers.Length);
        return entities;
    }

    private void OnEnable()
    {
        Tile.OnTileClicked += HandleTileClicked;
    }

    private void OnDisable()
    {
        Tile.OnTileClicked -= HandleTileClicked;
    }

    private void GenerateHexMapWithPooling()
    {
        float xOffset = tileSize * Mathf.Sqrt(3);
        float yOffset = tileSize * 1.5f;
        int tileIndex = 0;
        tiles = new Tile[width, height];

        // Compute map's total size in world units
        float mapWidth = (width - 1) * xOffset + xOffset;
        float mapHeight = (height - 1) * yOffset + yOffset;
        Vector2 mapCenterOffset = new Vector2(mapWidth / 2f, mapHeight / 2f);

        for (int y = 0; y < height; y++)
        {
            float yPos = y * yOffset;
            for (int x = 0; x < width; x++)
            {
                float xPos = x * xOffset;
                if (y % 2 == 1)
                    xPos += xOffset / 2f;

                Vector3 tilePos = new Vector3(xPos, yPos, 0);
                tilePos -= (Vector3)mapCenterOffset; // Shift to center

                GameObject tile;
                if (tileIndex < tilePool.Count)
                {
                    tile = tilePool[tileIndex];
                    tile.SetActive(true);
                }
                else
                {
                    tile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
                    tilePool.Add(tile);
                }

                tile.transform.SetParent(mapTransform); // Ensure parent is Map even when pooled
                tile.transform.localPosition = tilePos;
                tile.transform.localScale = Vector3.one * tileSize * 2f;
                tile.name = $"Tile_{x}_{y}";

                var tileComponent = tile.GetComponent<Tile>();
                tileComponent.SetType((TileType)mapData[x, y]);
                tiles[x, y] = tileComponent;
                tileComponent.x = x;
                tileComponent.y = y;
                tileIndex++;
            }
        }

        for (int i = tileIndex; i < tilePool.Count; i++)
        {
            tilePool[i].SetActive(false);
        }
    }

    public Vector2Int GetStartCoord()
    {
        for (int i = 0; i < mapData.GetLength(0); i++)
        {
            for (int j = 0; j < mapData.GetLength(1); j++)
            {
                if ((TileType)mapData[i, j] == TileType.SPAWN)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void SetMapDataAt(int x, int y, TileType type)
    {
        mapData[x, y] = (int)type;
    }

    public Tile GetTileAt(int x, int y)
    {
        return tiles[x, y];
    }

    public int GetDistanceToEnd(Vector2Int coord)
    {
        return GetDistanceToEnd(coord.x, coord.y);
    }

    public int GetDistanceToEnd(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return -1; // Out of bounds

        return distanceToEnd[x, y];
    }

    private void ComputeDistanceToEnd()
    {
        Debug.Log($"Computing distance to end for {endTiles.Count} end tiles.");
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
   
        foreach (var end in endTiles)
        {
            queue.Enqueue(end);
            distanceToEnd[end.x, end.y] = 0;
        }
        var count = 0;

        while (queue.Count > 0)
        {
            if (count > 2000)
            {
                Debug.LogWarning("Distance computation exceeded 2000 iterations, stopping to prevent infinite loop.");
                break;
            }
            var tile = queue.Dequeue();
            int currentCost = distanceToEnd[tile.x, tile.y];
            visited.Add(tile);
            count++;

            // Get neighbors
            var directions = tile.y % 2 == 0 ? Tile.EvenNeighbors : Tile.OddNeighbors;
            List<Vector2Int> neighbors = new List<Vector2Int>();
            foreach (var dir in directions)
            {
                Vector2Int neighbor = new Vector2Int(tile.x + dir.x, tile.y + dir.y);
                neighbors.Add(neighbor);
            }

            foreach (var neighbor in neighbors)
            {

                if (visited.Contains(neighbor)) continue; // Skip already visited tiles
                if (neighbor.x < 0 || neighbor.x >= width || neighbor.y < 0 || neighbor.y >= height)
                    continue; // Out of bounds
                if (mapData[neighbor.x, neighbor.y] == (int)TileType.WALL) 
                    continue; // Skip walls

                if (distanceToEnd[neighbor.x, neighbor.y] < currentCost + 1)
                {
                    distanceToEnd[neighbor.x, neighbor.y] = currentCost + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!tilePrefab) return;

        Gizmos.color = Color.cyan;

        float xOffset = tileSize * Mathf.Sqrt(3);
        float yOffset = tileSize * 1.5f;

        // Compute map's total size in world units
        float mapWidth = (width - 1) * xOffset + xOffset;
        float mapHeight = (height - 1) * yOffset + yOffset;
        Vector2 mapCenterOffset = new Vector2(mapWidth / 2f, mapHeight / 2f);

        for (int y = 0; y < height; y++)
        {
            float yPos = y * yOffset;
            for (int x = 0; x < width; x++)
            {
                float xPos = x * xOffset;
                if (y % 2 == 1)
                    xPos += xOffset / 2f;

                Vector3 tilePos = new Vector3(xPos + transform.position.x, yPos + transform.position.y, 0);
                tilePos -= (Vector3)mapCenterOffset; // Shift to center
                Vector3 pos = tilePos;
                Gizmos.DrawWireSphere(pos, tileSize * 1f);
            }
        }
    }
#endif
}
