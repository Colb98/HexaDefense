using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 10;
    public int height = 10;
    public GameObject tilePrefab;
    public float tileSize = 1f;

    [Header("Tile Pool (Read-Only)")]
    [SerializeField] private List<GameObject> tilePool = new List<GameObject>();

    private int[,] mapData;
    private Tile[,] tiles;
    [SerializeField] private TowerManager towerManager;

    [SerializeField] private UnitManager unitManager;

    [SerializeField] private ProjectileManager projectileManager;

    public TowerManager TowerManager => towerManager;   
    public UnitManager UnitManager => unitManager;
    public ProjectileManager ProjectileManager => projectileManager;

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
        GenerateHexMapWithPooling();
    }

    private void Update()
    {

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
        Debug.Log($"Map received tile click: {tile.name} at position {tile.transform.position}");

        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Debug.Log("Left Clicked");
            if (TowerPlacementManager.Instance.PlaceTower(tile))
            {
                var center = new Vector2Int(tile.x, tile.y);
                var coords = towerManager.GetNeighborCoordOfCenter(center, 2);
                foreach (var coord in coords)
                {
                    var curTile = tiles[coord.x, coord.y];
                    curTile.SetType(TileType.TOWER);
                    mapData[curTile.x, curTile.y] = (int)TileType.TOWER;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            Debug.Log("Right Clicked");

            tile.SetType(TileType.GROUND);
            mapData[tile.x, tile.y] = (int)TileType.GROUND;
        }

        GetUnitManager().UpdateUnitPaths();
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

                tile.transform.SetParent(this.transform); // Ensure parent is Map even when pooled
                tile.transform.localPosition = tilePos;
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

                Vector3 tilePos = new Vector3(xPos, yPos, 0);
                tilePos -= (Vector3)mapCenterOffset; // Shift to center
                Vector3 pos = tilePos;
                Gizmos.DrawWireSphere(pos, tileSize * 0.1f);
            }
        }
    }
#endif
}
