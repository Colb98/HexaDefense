using System;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public static event Action<Tile> OnTileClicked; // Static if you want global tile clicks

    public TileType type;
    public GameObject fill;
    public int x, y;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateTileType();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateTileType()
    {
        if (GameFlags.DebugMode)
        {
            Color color = Color.white;
            switch (type)
            {
                case TileType.GROUND:
                    color = new Color(0.79f, 0.56f, 0.21f);
                    break;
                case TileType.WALL:
                    color = new Color(0.53f, 0.23f, 0.03f);
                    break;
                case TileType.TOWER:
                    color = new Color(0.31f, 0.31f, 0.31f);
                    break;
                case TileType.SPAWN:
                    color = new Color(0.76f, 0.29f, 0.33f);
                    break;
                case TileType.GOAL:
                    color = new Color(0.22f, 0.53f, 0.16f);
                    break;
                case TileType.PATH:
                    color = new Color(0.78f, 0.47f, 0.84f);
                    break;
            }
            fill.GetComponent<SpriteRenderer>().color = color;
        }
    }

    public void SetType (TileType type)
    {
        this.type = type;
        UpdateTileType();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) // Left mouse button
        {
            OnTileClicked?.Invoke(this); // Notify listeners
        }
    }

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
    public static bool AreNeighbors(Vector2Int a, Vector2Int b)
    {
        (int x, int y)[] directions = a.y % 2 == 0 ? EvenNeighbors : OddNeighbors;

        foreach (var dir in directions)
        {
            if (a.x + dir.x == b.x && a.y + dir.y == b.y) return true;
        }
        return false;
    }

    public static Vector3 GetTilePosition(float tileSize, Vector2Int coord, int width, int height)
    {
        float xOffset = tileSize * Mathf.Sqrt(3);
        float yOffset = tileSize * 1.5f;
        float yPos = coord.y * yOffset;
        float xPos = coord.x * xOffset;
        // Compute map's total size in world units
        float mapWidth = (width - 1) * xOffset + xOffset;
        float mapHeight = (height - 1) * yOffset + yOffset;
        Vector2 mapCenterOffset = new Vector2(mapWidth / 2f, mapHeight / 2f);
        if (coord.y % 2 == 1)
            xPos += xOffset / 2f;

        Vector3 tilePos = new Vector3(xPos, yPos, 0);
        tilePos -= (Vector3)mapCenterOffset; // Shift to center
        return tilePos;
    }
}
