using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Tile : MonoBehaviour
{
    public static readonly Vector2Int[] EvenNeighbors = {
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(-1, -1), new Vector2Int(0, -1),
        new Vector2Int(-1, 1), new Vector2Int(0, 1)
    };

    public static readonly Vector2Int[] OddNeighbors = {
        new Vector2Int(-1,  0), new Vector2Int(1,  0),
        new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(0, 1), new Vector2Int(1, 1)
    };

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

    public void SetType(TileType type)
    {
        this.type = type;
        UpdateTileType();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) // Left mouse button
        {
            if (!IsPointerOverUI()) // Check if the pointer is over a UI element
            {
                OnTileClicked?.Invoke(this); // Notify listeners
            }
        }
    }

    // Method to check if the pointer is over a UI element
    private bool IsPointerOverUI()
    {
        // Check if the pointer is over a UI element
        if (EventSystem.current != null)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return false;
    }

    public static bool AreNeighbors(Vector2Int a, Vector2Int b)
    {
        Vector2Int[] directions = a.y % 2 == 0 ? EvenNeighbors : OddNeighbors;

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

    public static float GetUnitDistance(float tileSize = 0.08f)
    {
        return tileSize * Mathf.Sqrt(3);
    }

    // Chuyển offset (col, row) -> cube (x, y, z) cho layout odd-r
    public static Vector3Int OffsetToCube_OddR(int col, int row)
    {
        int x = col - (row - (row & 1)) / 2;
        int z = row;
        int y = -x - z;
        return new Vector3Int(x, y, z); // tương ứng (q, r, s)
    }

    public static Vector2Int CubeToOffset_OddR(int x, int y, int z)
    {
        int col = x + (z - (z & 1)) / 2;
        int row = z;
        return new Vector2Int(col, row); // tương ứng (col, row)
    }

    // Trả về danh sách các ô offset (col, row) nằm trong bán kính range
    public static List<Vector2Int> GetHexesInRange(Vector2Int centerOffset, int range)
    {
        var results = new List<Vector2Int>();

        // Bước 1: chuyển offset về cube
        Vector3Int centerCube = OffsetToCube_OddR(centerOffset.x, centerOffset.y);

        // Bước 2: duyệt cube coordinate xung quanh
        for (int dx = -range; dx <= range; dx++)
        {
            //Duyệt trên toạ độ cube
            //Ta có q +r + s = 0
            //không xét tới s vì s = -q - r;
            //            1 ô cách ô hiện tại range -> nghĩa là ta di chuyển trên 1 trục(q hoặc r hoặc s) range ô, tổng 2 trục còn lại phải là - range và không có trục nào được cùng chiều(âm hoặc dương) với trục đã chọn.
            //VD: cách 5 ô, thì khi đi trên trục - q 5 ô, trục r và s(chiều dương) phải đi dy và dz ô sao cho dy + dz = 5.
            //Nói cách khác, | q + r | <= range(đúng với tất cả các cặp khác).

            //Khi đó, muốn duyệt tất cả ô trong tầm range ta sẽ duyệt từ - range-> + range của trục q
            //Trục r thì sao?
            //Từ công thức | q + r | <= range ta sẽ => q + r <= range && q + r >= -range. (Do range >= 0)
            //=> r <= -q + range && r >= -q - range;
            //            Do đó ta cần duyệt r từ - q - range đến - q + range
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                int dz = -dx - dy;
                Vector3Int cube = new Vector3Int(
                    centerCube.x + dx,
                    centerCube.y + dy,
                    centerCube.z + dz
                );

                // Bước 3: chuyển về offset (col, row)
                Vector2Int offset = CubeToOffset_OddR(cube.x, cube.y, cube.z);
                results.Add(offset);
            }
        }

        return results;
    }

    public static int GetHexManhattanDistance(Vector2Int tile1, Vector2Int tile2)
    {
        var cube1 = OffsetToCube_OddR(tile1.x, tile1.y);
        var cube2 = OffsetToCube_OddR(tile2.x, tile2.y);
        return (Math.Abs(cube1.x - cube2.x) + Math.Abs(cube1.y - cube2.y) + Math.Abs(cube1.z - cube2.z)) / 2;
    }
}
