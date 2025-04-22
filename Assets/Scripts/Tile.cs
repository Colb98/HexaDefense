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
}
