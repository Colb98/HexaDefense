using System;
using System.Collections.Generic;
using UnityEngine;

public class InstantAttack
{
    protected Entity owner; // The entity that fired the projectile

    [SerializeField] protected float physicalDamage = 10f;
    [SerializeField] protected float magicalDamage = 10f;
    [SerializeField] protected bool isCrit = false;

    private List<Entity> affectedEntities;

    // Method to set the owner of the projectile
    public void SetOwner(Entity entity)
    {
        owner = entity;
    }

    // Method to apply damage to targets
    protected void DealDamage(List<Entity> affectedEntities)
    {
        foreach (var entity in affectedEntities)
        {
            entity.TakeDamage(physicalDamage, magicalDamage, isCrit);
        }
    }

    public void SetIsCrit(bool isCrit)
    {
        this.isCrit = isCrit;
    }

    internal void Initialize(List<Entity> affectedEntities, float physDamage, float magDamage, bool isCrit)
    {
        this.affectedEntities = affectedEntities;
        physicalDamage = physDamage;
        magicalDamage = magDamage;
        this.isCrit = isCrit;
    }

    public void Trigger()
    {
        DealDamage(affectedEntities);
    }
}
