using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour, IPausableTick
{
    public bool Registered { get; set; }

    [Header("Entity Stats")]
    public Vector2Int position;

    public Stats Stats = new();
    public float timeSinceLastAttack;
    public string entityType;

    public Entity target;
    protected Map _map;
    public bool isDefense = true;

    public int aggroLimit = 4;
    public int aggroLevel = 1;
    public List<Entity> aggroEntities = new();


    [Header("Health Bar")]
    [SerializeField] private HealthBarUI healthBarPrefab;
    [SerializeField] private Transform healthBarPivot; // assign HealthBarPivot in Inspector

    public CombatTextManager combatText;

    private HealthBarUI healthBarUI;

    [SerializeField] protected List<Buff> buffs = new();

    void Start()
    {
        timeSinceLastAttack = Stats.GetSecondsPerAttack();

        if (healthBarPivot)
        {
            // Spawn and attach health bar
            healthBarUI = Instantiate(healthBarPrefab, healthBarPivot.position, Quaternion.identity, healthBarPivot);
            healthBarUI.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(float physDmg, float magDmg, bool isCrit = false) {
        float totalDamage = Math.Max(0, physDmg - Stats.PhysicResist.Value) + Math.Max(0, magDmg - Stats.MagicResist.Value);
        Stats.CurrentHealth -= totalDamage;

        if (combatText != null)
        {
            combatText.ShowNumber(-((int)totalDamage), transform.position, transform, isCrit);
        }

        if (healthBarUI != null)
            healthBarUI.SetHealth(Stats.CurrentHealth, Stats.HealthPoint.Value);
        //Debug.Log($"Entity take damage {totalDamage}");
        if (IsDead())
        {
            OnDead();
            //Debug.Log("On Entity Dead");
        }
    }

    public bool IsDead() {
        return Stats.CurrentHealth <= 0;
    }

    public void OnSpawn()
    {
        if (PausableUpdateManager.instance.IsTickableRegistered(this))
        {
            Debug.Log("Entity already registered in PausableUpdateManager, not registering again." + name, this);
        }
        PausableUpdateManager.instance.Register(this);
    }

    public void OnRemoved()
    {
        if (target)
        {
            target.aggroEntities.Remove(this);
        }
        target = null;
        aggroEntities.Clear();
        ReturnToPool();
        PausableUpdateManager.instance.Unregister(this);

        if (healthBarUI != null)
        {
            healthBarUI.gameObject.SetActive(false);
        }
    }

    protected virtual void OnDead()
    {
        OnRemoved();
        RemoveAllBuffs();
    }

    public virtual bool IsMovable()
    {
        return true;
    }

    public virtual bool IsMoving()
    {
        return false;
    }

    public virtual bool CanFly()
    {
        return false;
    }

    public virtual int GetSize()
    {
        return 1;
    }

    protected virtual void OnNoValidTarget()
    {
        //Debug.Log("No valid target");
    }

    protected abstract void ReturnToPool();

    void Update()
    {
        // UpdateAttack();
    }

    public virtual void Tick()
    {
        if (IsDead())
        {
            return;
        }
        UpdateAttack();
        UpdateBuffs();
    }

    public void UpdateBuffs()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            buffs[i].Tick(Time.deltaTime);
        }
    }

    public void UpdateAttack()
    {
        if (!CanAttack())
        {
            timeSinceLastAttack = 0;
            return;
        }
        timeSinceLastAttack += Time.deltaTime;

        if (target != null && target.IsDead())
        {
            ResetTarget();
            if (IsMoving())
            {
                _map.UnitManager.FindTargetAndPath((Unit)this);
            }
        }

        if (IsMoving()) return;

        if (target != null)
        {
            var pos = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
            var targetPos = Tile.GetTilePosition(_map.tileSize, target.position, _map.width, _map.height);
            float distance = Vector3.Distance(pos, targetPos) / Tile.GetUnitDistance(_map.tileSize);
            
            if (distance > Stats.AttackRange.Value)
            {
                if (!IsMovable())
                {
                    ResetTarget();
                }
            }
        }

        if (target != null && CanAttack())
        {
            if (IsAttackCooledDown())
            {
                PerformAttack();
                timeSinceLastAttack = 0;
            }
            return;
        } 

        if (target == null)
        {
            OnNoValidTarget();
        }

        //// Check if the tower has a target
        //bool validTarget = target != null && !target.IsDead();
        //if (!IsMovable())
        //{
        //    if (validTarget)
        //    {
        //        // Check if the target is within range
        //        var pos = Tile.GetTilePosition(_map.tileSize, position, _map.width, _map.height);
        //        var targetPos = Tile.GetTilePosition(_map.tileSize, target.position, _map.width, _map.height);
        //        float distance = Vector3.Distance(pos, targetPos) / Tile.GetUnitDistance(_map.tileSize);
        //        validTarget = distance <= attackRange;
        //    }
        //}

        //if (validTarget)
        //{
        //    // Attack logic here
        //    if (isAttackCooledDown())
        //    {
        //        PerformAttack();
        //        timeSinceLastAttack = 0;
        //    }
        //}
        //else
        //{
        //    if (target != null)
        //    {
        //        ResetTarget();
        //    }

        //    // If cannot move: Find new target immediately
        //    // If can move: Unit manager will look for new target
        //    if (!IsMovable())
        //    {
        //        // Find a new target, could be null
        //        target = GetTargetEntity();
        //        if (target != null)
        //        {
        //            target.SetAggroEntity(this);
        //        }
        //    }
        //}
    }

    public void SetTarget(Entity target)
    {
        //Debug.Log($"Set target for entity {name} ({entityType}) to {target?.name} ({target?.entityType})");
        ResetTarget();
        this.target = target;
        if (target != null)
        {
            target.SetAggroEntity(this);
        }
    }

    public void ResetTarget()
    {
        if (target != null)
        {
            target.RemoveAggroEntity(this);
            target = null;
        }
    }

    public void SetAggroEntity(Entity entity)
    {
        aggroEntities.Add(entity);
    }
    
    public void RemoveAggroEntity(Entity entity)
    {
        aggroEntities.Remove(entity);
    }
    
    public bool CanAttack()
    {
        // Check for status/debuffs
        return true;
    }

    public bool CanBeAttacked(int attackerAggroLevel)
    {
        int count = 0;
        foreach (Entity entity in aggroEntities)
        {
            if (entity != null)
            {
                count += entity.aggroLevel;
            }
        }
        return count + attackerAggroLevel <= aggroLimit;
    }

    protected virtual void PerformAttack()
    {
        bool isCrit = UnityEngine.Random.Range(0, 1) < Stats.CritChance.Value;
        // Perform attack
        target.TakeDamage(GetPhysicalDamage(isCrit), GetMagicalDamage(isCrit), isCrit);
    }


    protected bool IsAttackCooledDown()
    {
        return timeSinceLastAttack >= Stats.GetSecondsPerAttack();
    }

    protected float GetPhysicalDamage(bool isCrit)
    {
        if (isCrit)
        {
            return Stats.PhysicalDamage.Value * (1 + Stats.CritDamage.Value);
        }
        return Stats.PhysicalDamage.Value;
    }

    protected float GetMagicalDamage(bool isCrit)
    {
        if (isCrit)
        {
            return Stats.MagicalDamage.Value * (1 + Stats.CritDamage.Value);
        }
        return Stats.MagicalDamage.Value;
    }


    protected Entity[] GetEntitiesToAttack() {
        // Get all entities in the map
        Entity[] allEntities = _map.GetAllEntities();

        // Filter entities to only include those with a different isDefense value
        var filteredEntities = new System.Collections.Generic.List<Entity>();
        foreach (Entity entity in allEntities)
        {
            if (entity != null && entity.isDefense != this.isDefense && entity.Stats.CurrentHealth > 0)
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
        float attackRangeSqr = Stats.AttackRange.Value * Tile.GetUnitDistance() * Stats.AttackRange.Value * Tile.GetUnitDistance();
        foreach (Entity entity in entities) {
            if (entity != null)
            {
                float dist = (entity.transform.position - transform.position).sqrMagnitude;
                if (dist < min && dist <= attackRangeSqr)
                {
                    min = dist;
                    target = entity;
                }
            }
        }
        return target;
    }

    public bool IsInRange(Vector2 position)
    {

        return (this.position - position).sqrMagnitude <= Stats.AttackRange.Value * Stats.AttackRange.Value;
    }

    public float GetDistanceSqr(Vector2 position)
    {
        return (this.position - position).sqrMagnitude;
    }

    public void AddBuff(Buff buff)
    {
        // Check if buff of the same name already exists
        foreach (Buff existingBuff in buffs)
        {
            if (existingBuff.name == buff.name)
            {
                existingBuff.AddBuff(buff);
                return;
            }
        }
        buffs.Add(buff);
    }

    public void RemoveBuff(string name)
    {
        buffs.RemoveAll(b => b.name == name);
    }

    public void RemoveBuff(Buff buff)
    {
        buffs.Remove(buff);
    }

    public void RemoveAllBuffs()
    {
        foreach (Buff buff in buffs)
        {
            buff.entity.Stats.RemoveModifier(buff.modifier);
        }
        buffs.Clear();
    }
}