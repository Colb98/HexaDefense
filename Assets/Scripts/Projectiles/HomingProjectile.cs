// Homing Projectile - follows a specific target entity
using System.Security.Principal;
using UnityEngine;

public class HomingProjectile : Projectile
{
    [SerializeField] private Entity targetEntity;

    // Initialize the projectile with a target entity
    public void Initialize(Entity target, float physDamage, float magDamage, float projSpeed)
    {
        targetEntity = target;
        physicalDamage = physDamage;
        magicalDamage = magDamage;
        speed = projSpeed * Tile.GetUnitDistance();
    }

    public override void Tick()
    {
        // If target is destroyed or null, self-destruct
        if (targetEntity == null || targetEntity.IsDead())
        {
            owner.GetMap().ProjectileManager.ReturnHomingProjectileToPool(this, prefab);
            targetEntity = null;
            return;
        }

        // Calculate direction to target
        Vector3 targetDirection = (targetEntity.transform.position - transform.position).normalized;

        float distanceSqr = (targetEntity.transform.position - transform.position).sqrMagnitude;
        if (distanceSqr <= speed * Time.deltaTime * speed * Time.deltaTime) {
            transform.position = targetEntity.transform.position;
            targetEntity.TakeDamage(physicalDamage, magicalDamage);
            owner.GetMap().ProjectileManager.ReturnHomingProjectileToPool(this, prefab);
        }
        else
        {
            // Move forward
            transform.position += targetDirection * speed * Time.deltaTime;
        }
    }
}
