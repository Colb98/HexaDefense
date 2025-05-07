using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity {
    float speed = 3.0f;
    List<Vector2Int> path = new List<Vector2Int>();
    bool isAirborne;

    bool isMoving = false;
    float moveProgress = 0.0f;

    public Map map;

    public override void Update()
    {
        base.Update();
        UpdateMoving();
    }

    public void SetCoord(Vector2Int pos)
    {
        position = pos;
        transform.position = Tile.GetTilePosition(map.tileSize, position, map.width, map.height);
    }

    public void UpdateMoving() {
        if (!isMoving || path.Count == 0) return;

        var targetTile = path[0];

        var startWorldPos = Tile.GetTilePosition(map.tileSize, position, map.width, map.height);
        var targetWorldPos = Tile.GetTilePosition(map.tileSize, targetTile, map.width, map.height);
        float distance = Vector3.Distance(startWorldPos, targetWorldPos);
        float step = speed * Time.deltaTime;
        moveProgress += step / distance;
        //Debug.Log($"Move step from {position} to {targetTile} with progress {moveProgress}");

        while (moveProgress >= 1f)
        {
            position = targetTile;
            path.RemoveAt(0);
            processPathNextMove();
            moveProgress -= 1f;

            if (path.Count > 0)
            {
                targetTile = path[0];
                startWorldPos = targetWorldPos;
                targetWorldPos = Tile.GetTilePosition(map.tileSize, targetTile, map.width, map.height);
            }
            else
            {
                isMoving = false;
                moveProgress = 0f;
                startWorldPos = targetWorldPos;
                OnStopMoving();
            }
        }

        transform.localPosition = Vector3.Lerp(startWorldPos, targetWorldPos, moveProgress);
    }

    public void OnStopMoving()
    {
        if (map.GetMapDataAt(position.x, position.y) == TileType.GOAL)
        {
            Destroy(gameObject);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        moveProgress = 0f;
        transform.localPosition = Tile.GetTilePosition(map.tileSize, position, map.width, map.height);
        this.path.Clear();
    }

    public void MoveByPath(List<Vector2Int> path)
    {
        this.path.Clear();
        this.path.AddRange(path);
        isMoving = true;
        processPathNextMove();
    }

    private void processPathNextMove()
    {
        while (path.Count > 0)
        {
            var next = path[0];
            if ((position - next).sqrMagnitude < 0.0001)
            {
                path.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }
}
