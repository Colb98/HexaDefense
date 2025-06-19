using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Entity
{
    [Header("Unit Stats")]
    [SerializeField] float speed = 3.0f;
    List<Vector2Int> path = new List<Vector2Int>();
    bool isAirborne;

    [SerializeField] bool isMoving = false;
    [SerializeField] float moveProgress = 0.0f;
    int reward = 0;

    public virtual void Initialize(string type, EnemyLevelData data)
    {
        entityType = type;

        maxHP = data.health;
        hp = data.health;
        physicalDamage = data.physicalDamage;
        magicalDamage = data.magicalDamage;

        physicalDefense = data.resistances.physical;
        magicalDefense = data.resistances.magical;

        reward = data.reward;

        attackRange = 1.5f;       // Still default unless you want per-type
        attackCooldown = 1.0f;

        timeSinceLastAttack = 0f;
        speed = data.moveSpeed;
    }

    public override void Tick()
    {
        base.Tick();
        UpdateMoving();
    }

    public void Reset()
    {
        hp = 10;
        isMoving = false;
        path.Clear();
        moveProgress = 0f;
    }

    protected override void OnDead()
    {
        _map.UnitManager.OnUnitDead(this);
        base.OnDead();
    }

    public void SetCoord(Vector2Int pos)
    {
        position = pos;
        transform.position = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
    }

    public Vector2Int GetNextPosition()
    {
        if (path.Count > 0)
        {
            return path[0];
        }
        Debug.LogError("Cannot get next position, path is empty.");
        return Vector2Int.zero;
    }

    public void UpdateMoving() {
        if (IsDead()) return;
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
            PausableUpdateManager.instance.Unregister(this);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        moveProgress = 0f;
        transform.localPosition = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
        this.path.Clear();
    }

    public override bool IsMoving()
    {
        return isMoving;
    }

    public void MoveByPath(List<Vector2Int> path)
    {
        Debug.Log($"Unit {name} move by path {path.Count}");
        this.path.Clear();
        this.path.AddRange(path);
        isMoving = path.Count > 0;
        processPathNextMove();
    }

    public void MoveByPathAfterFinish(List<Vector2Int> path)
    {
        Debug.Log($"Unit {name} move by path after finish {path.Count}. Current position: {position}, {path[0]}");
        this.path.RemoveRange(1, this.path.Count - 1); 
        this.path.AddRange(path);
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
        if (path.Count == 0)
        {
            isMoving = false;
        }
    }
    protected override void OnNoValidTarget()
    {
        _map.UnitManager.FindTargetAndPath(this);
    }

    protected override void ReturnToPool()
    {
        _map.GetUnitManager().ReturnUnitToPool(this);
    }

    public int GetGoldReward()
    {
        return reward;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (path == null || path.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 from = Tile.GetTilePosition(_map.tileSize, path[i], _map.width, _map.height);
            Vector3 to   = Tile.GetTilePosition(_map.tileSize, path[i + 1], _map.width, _map.height);
            Gizmos.DrawLine(from, to);
            Gizmos.DrawWireSphere(from, 0.05f);
        }

        // Draw last point
        Vector3 last = Tile.GetTilePosition(_map.tileSize, path[path.Count - 1], _map.width, _map.height);
        Gizmos.DrawWireSphere(last, 0.05f);
    }
#endif
}
