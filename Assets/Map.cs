using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 10;
    public int height = 10;
    public GameObject tilePrefab;
    public GameObject enemyPrefab;
    public float tileSize = 1f;

    [Header("Tile Pool (Read-Only)")]
    [SerializeField] private List<GameObject> tilePool = new List<GameObject>();

    private int[,] mapData;
    private Tile[,] tiles;

    private bool found = false;

    private float timerGenerateEnemy = 0.0f;

    [Header("Mobs spawn & Path")]
    public List<Vector2Int> path = new List<Vector2Int>();
    [SerializeField] private float mobSpawnInterval = 2f;
    private List<Unit> enemies = new List<Unit>();

    private void Start()
    {
        LoadMapData();
        GenerateHexMapWithPooling();
        timerGenerateEnemy = mobSpawnInterval + 0.5f;
    }

    private void Update()
    {
        if (!found)
        {
            FindPath();
            found = true;
        }
        else
        {
            timerGenerateEnemy += Time.deltaTime;
            if (timerGenerateEnemy >= mobSpawnInterval && path.Count > 0)
            {
                GameObject enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
                Unit unit = enemy.GetComponent<Unit>();
                unit.map = this;
                unit.SetCoord(path[0]);
                unit.MoveByPath(path);
                Debug.Log($"Move by path {path.Count}");
                enemies.Add(unit);
                timerGenerateEnemy = 0.0f;
                enemy.transform.SetParent(this.transform); // Ensure parent is Map even when pooled
            }
        }
    }

    private void FindPath()
    {
        bool foundStart = false;
        (int x, int y) start = (0, 0);
        if (mapData == null)
        {
            Debug.LogError("mapData is not initialized!");
            return;
        }
        Debug.Log("Start Find Path");
        // Reset old path
        for (int i = 0; i < mapData.GetLength(0); i++)
        {
            for (int j = 0; j < mapData.GetLength(1); j++)
            {
                if (!foundStart && (TileType)mapData[i, j] == TileType.SPAWN)
                {
                    start = new(i, j);
                    foundStart = true;
                }
                if (tiles[i, j].type == TileType.PATH)
                {
                    tiles[i, j].SetType(TileType.GROUND);
                }
            }
        }
        this.path.Clear();
        var path = AStar.FindPath(mapData, (start.x, start.y));
        if (path != null)
        {
            foreach (var node in path)
            {
                var type = (TileType)mapData[node.x, node.y];
                if (type == TileType.GOAL || type == TileType.SPAWN)
                {
                    this.path.Add(new Vector2Int(node.x, node.y));
                    continue;
                }
                tiles[node.x, node.y].SetType(TileType.PATH);
                this.path.Add(new Vector2Int(node.x, node.y));
            }
        }
        //this.enemies.ForEach(enemy =>
        //{
        //    enemy.MoveByPath(this.path);
        //});
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

            tile.SetType(TileType.TOWER);
            mapData[tile.x, tile.y] = (int)TileType.TOWER;
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            Debug.Log("Right Clicked");

            tile.SetType(TileType.GROUND);
            mapData[tile.x, tile.y] = (int)TileType.GROUND;
        }

        FindPath();
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
