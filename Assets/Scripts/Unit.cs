using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity {
    [SerializeField] float speed = 3.0f;
    List<Vector2Int> path = new List<Vector2Int>();
    bool isAirborne;

    bool isMoving = false;
    float moveProgress = 0.0f;

    public virtual void Initialize(string type, EnemyLevelData data)
    {
        entityType = type;

        hp = data.health;
        physicalDamage = data.physicalDamage;
        magicalDamage = data.magicalDamage;

        physicalDefense = data.resistances.physical;
        magicalDefense = data.resistances.magical;

        attackRange = 1.5f;       // Still default unless you want per-type
        attackCooldown = 1.0f;

        timeSinceLastAttack = 0f;
        speed = data.moveSpeed;
    }

    public override void Update()
    {
        base.Update();
        UpdateMoving();
    }

    public void Reset()
    {
        hp = 10;
    }

    public void SetCoord(Vector2Int pos)
    {
        position = pos;
        transform.position = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
    }

    public void UpdateMoving() {
        if (!isMoving || path.Count == 0) return;

        var targetTile = path[0];

        var startWorldPos = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
        var targetWorldPos = Tile.GetTilePosition(_map.tileSize, targetTile, _map.width, _map.height);
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
                targetWorldPos = Tile.GetTilePosition(_map.tileSize, targetTile, _map.width, _map.height);
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
        if (_map.GetMapDataAt(position.x, position.y) == TileType.GOAL)
        {
            this._map.GetUnitManager().RemoveUnit(this);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        moveProgress = 0f;
        transform.localPosition = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
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

    protected override void ReturnToPool()
    {
        _map.GetUnitManager().ReturnUnitToPool(this);
    }
}
