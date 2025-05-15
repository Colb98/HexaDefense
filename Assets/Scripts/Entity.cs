using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public Vector2Int position;

    public float hp;
    public float physicalDamage;
    public float magicalDamage;
    public float physicalDefense;
    public float magicalDefense;
    public float attackRange;
    public float attackCooldown;
    public float timeSinceLastAttack;
    public string entityType;

    public Entity target;
    protected Map _map;
    public bool isDefense = true;

    void Start()
    {
        timeSinceLastAttack = attackCooldown;
    }

    public void TakeDamage(float physDmg, float magDmg) {
        float totalDamage = Math.Max(0, physDmg - physicalDefense) + Math.Max(0, magDmg - magicalDefense);
        hp -= totalDamage;
        Debug.Log($"Entity take damage {totalDamage}");
        if (IsDead())
        {
            OnDead();
            Debug.Log("On Entity Dead");
        }
    }

    public bool IsDead() {
        return hp <= 0;
    }

    private void OnDead()
    {
        ReturnToPool();
    }

    protected abstract void ReturnToPool();

    public virtual void Update()
    {
        UpdateAttack();
    }

    public void UpdateAttack()
    {
        timeSinceLastAttack += Time.deltaTime;
        if (!isDefense) return; // Only update attack for defense entities
        // Check if the tower has a target
        bool validTarget = target != null && !target.IsDead();
        if (validTarget)
        {
            // Check if the target is within range
            var pos = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
            var targetPos = Tile.GetTilePosition(_map.tileSize, target.position, _map.width, _map.height);
            float distance = Vector3.Distance(pos, targetPos) / Tile.GetUnitDistance(_map.tileSize);
            validTarget = distance <= attackRange;
        }
        if (validTarget)
        {
            // Attack logic here
            if (isAttackCooledDown())
            {
                PerformAttack();
                timeSinceLastAttack = 0;
            }
        }
        else
        {
            target = GetTargetEntity();
        }
    }
    
    protected virtual void PerformAttack()
    {
        // Perform attack
        target.TakeDamage(physicalDamage, magicalDamage);
    }

    protected bool isAttackCooledDown()
    {
        return timeSinceLastAttack >= attackCooldown;
    }

    protected Entity[] GetEntitiesToAttack() {
        // Get all entities in the map
        Entity[] allEntities = _map.GetAllEntities();

        // Filter entities to only include those with a different isDefense value
        var filteredEntities = new System.Collections.Generic.List<Entity>();
        foreach (Entity entity in allEntities)
        {
            if (entity != null && entity.isDefense != this.isDefense && entity.hp > 0)
            {
                filteredEntities.Add(entity);
            }
        }

        return filteredEntities.ToArray();
    }

    public void SetMap(Map map)
    {
        _map = map;
    }

    public Map GetMap()
    {
       return _map;
    }

    public Entity GetTargetEntity()
    {
        Entity[] entities = GetEntitiesToAttack();
        // TODO: use strategy to find target
        float min = float.MaxValue;
        Entity target = null;
        float attackRangeSqr = attackRange * attackRange;
        foreach (Entity entity in entities) {
            if (entity != null)
            {
                float dist = (position - entity.position).sqrMagnitude;
                if (dist < min && dist <= attackRangeSqr)
                {
                    min = dist;
                    target = entity;
                }
            }
        }
        return target;
    }
}