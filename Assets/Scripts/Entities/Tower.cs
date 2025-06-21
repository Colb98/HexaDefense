using System;
using System.Linq;
using UnityEngine;

public class Tower : Entity
{
    [SerializeField] TowerType towerType;
    [SerializeField] int size = 2;

    [SerializeField] private Color originalColor;

    [SerializeField] private int level;

    public override void Tick()
    {
        if (IsDead()) return;
        UpdateAttack();
        if (isAttackCooledDown())
        {
            GetComponent<SpriteRenderer>().color = originalColor;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public override int GetSize()
    {
        return size;
    }

    protected override void OnDead()
    {
        // Return to pool and unregister from the update manager
        base.OnDead();
        // Update map data & find path
        _map.OnTowerDead(this);
    }

    protected override void OnNoValidTarget()
    {
        // Find a new target, could be null
        SetTarget(GetTargetEntity());
    }

    /// <summary>
    /// Initializes the tower with the given parameters.
    /// </summary>
    /// <param name="size">The size of the tower footprint.</param>
    /// <param name="position">The grid position of the tower.</param>
    public virtual void Initialize(int size, Vector2Int position, string type, TowerLevelData data, TowerConfig config)
    {
        this.size = size;
        this.position = position;

        // Additional initialization logic here
        entityType = type;

        maxHP = data.health;
        hp = data.health;
        physicalDamage = data.physicalDamage;
        magicalDamage = data.magicalDamage;

        attackRange = config.range;
        attackCooldown = config.attackSpeed;

        timeSinceLastAttack = 0f;
        Debug.Log($"Tower initialized: {gameObject.name}, size: {size}, position: {position}, type: {type}, data: {data}, config: {config}");

        originalColor = GetComponent<SpriteRenderer>().color;
        UpdateAttackRangeSprite();
    }

    private void UpdateAttackRangeSprite()
    {
        // Calculate the attack range based on the tower type
        Transform childTransform = transform.Find("AttackRange");

        if (childTransform != null)
        {
            var scale = attackRange * new Vector3(1.05f, 1.05f, 1.05f) / transform.localScale.x;
            //Debug.Log($"Attack Range {scale}, {attackRange}, {transform.localScale}");

            // Set the local scale
            childTransform.localScale = scale;
        }
        else
        {
            Debug.LogWarning("Child sprite not found: Attack range");
        }
    }

    public override bool IsMovable()
    {
        return false;
    }

    protected override void ReturnToPool()
    {
        _map.GetTowerManager().ReturnTowerToPool(this);
    }

    public int GetSellPrice()
    {
        return 50; // Temp, should be 50% of accumulated buy + upgrade price
    }

    public void Upgrade()
    {
        if (!CanUpgrade())
        {
            return;
        }
        var newLevel = level + 1;
        var config = GameConfigManager.Instance.Towers[entityType];
        var levelConfig = config.levels.FirstOrDefault(lvl => lvl.level == newLevel);
        if (levelConfig == null)
        {

           Debug.LogError($"No level config found for tower {entityType} at level {newLevel}. Cannot upgrade.");
            return;
        }
        Initialize(size, position, entityType, levelConfig, config);
        SetLevel(newLevel);
    }

    public bool CanUpgrade()
    {
        return level < 3;
    }

    internal void SetLevel(int level)
    {
        this.level = level;
    }
}
