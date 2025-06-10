using UnityEngine;
using System.Collections.Generic;

// Base class for all projectiles
public abstract class Projectile : MonoBehaviour, IPausableTick
{
    protected Entity owner; // The entity that fired the projectile

    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float physicalDamage = 10f;
    [SerializeField] protected float magicalDamage = 10f;

    protected GameObject prefab; // Prefab reference for the projectile

    protected virtual void Start()
    {
        PausableUpdateManager.instance.Register(this);
    }

    // Abstract method to be implemented by specific projectile types
    public abstract void Tick();

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
            entity.TakeDamage(physicalDamage, magicalDamage);
        }
    }

    public void SetPrefab(GameObject pf)
    {
        prefab = pf;
    }
}