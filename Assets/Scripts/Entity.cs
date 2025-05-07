using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public Vector2Int position;

    public int hp;
    public int physicalDamage;
    public int magicalDamage;
    public int physicalDefense;
    public int magicalDefense;
    public float attackRange;
    public float attackCooldown;
    public float timeSinceLastAttack;

    public Entity target;

    public void TakeDamage(int physDmg, int magDmg) {
        int totalDamage = Math.Max(0, physDmg - physicalDefense) + Math.Max(0, magDmg - magicalDefense);
        hp -= totalDamage;
    }

    public bool IsDead() {
        return hp <= 0;
    }

    public virtual void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        // Have target to attack
        if (target != null)
        {
            UpdateAttack();
        }
    }

    public void UpdateAttack()
    {
        if (timeSinceLastAttack >= attackCooldown)
        {
            // Throw attack here
            timeSinceLastAttack = 0;
        }
    }

    public Entity getTargetEntity(Entity[] entities)
    {
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