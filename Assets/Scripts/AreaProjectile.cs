
// Area of Effect (AoE) Projectile - attacks a specific area
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AreaProjectile : Projectile
{
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private Vector3 targetPosition;

    // Reference to the explosion sprite/animation
    [SerializeField] private GameObject explosionSprite;

    // Explosion duration
    [SerializeField] private float explosionDuration = 0.4f;

    // Flag to prevent multiple explosions
    private bool hasExploded = false;

    // Initialize the projectile with target position and configuration
    public void Initialize(Vector3 target, float radius, float physDamage, float magDamage, float projSpeed)
    {
        targetPosition = target;
        explosionRadius = radius;
        physicalDamage = physDamage;
        magicalDamage = magDamage;
        speed = projSpeed * Tile.GetUnitDistance();
        Renderer projectileRenderer = GetComponent<Renderer>();
        if (projectileRenderer != null)
        {
            projectileRenderer.enabled = true;
        }

        // Ensure explosion sprite is initially disabled
        if (explosionSprite != null)
        {
            var scale = radius / 0.5f;
            explosionSprite.SetActive(false);
            explosionSprite.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public override void Update()
    {
        // Only move if not exploded
        if (!hasExploded)
        {
            // Move towards target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Check if reached target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        List<Entity> affectedEntities = new List<Entity>();
        Entity[] allEntities = owner.GetMap().GetAllEntities();

        var radiusSqr = explosionRadius * explosionRadius;
        foreach (var entity in allEntities)
        {
            // Skip if the entity is the owner or not in the same team
            if (entity == owner || entity.IsDead() || entity.isDefense == owner.isDefense) continue;
            var unitSqr = Tile.GetUnitDistance(owner.GetMap().tileSize) * Tile.GetUnitDistance(owner.GetMap().tileSize);
            var magnitudeSqr = Vector3.SqrMagnitude(entity.transform.position - targetPosition) / unitSqr;
            // Check if the entity is within the explosion radius
            if (magnitudeSqr <= radiusSqr)
            {
                affectedEntities.Add(entity);
            }
        }

        // Deal damage to affected entities
        DealDamage(affectedEntities);

        // Show explosion sprite
        if (explosionSprite != null)
        {
            // Position explosion sprite at target position
            explosionSprite.transform.position = targetPosition;
            explosionSprite.SetActive(true);

            // Destroy the projectile after explosion duration
            StartCoroutine(DestroyAfterExplosion());
        }
        else
        {
            // If no explosion sprite, destroy immediately
            owner.GetMap().ProjectileManager.ReturnAreaProjectileToPool(this, prefab);
        }
    }

    private IEnumerator DestroyAfterExplosion()
    {
        // Disable the main projectile renderer to make it invisible
        Renderer projectileRenderer = GetComponent<Renderer>();
        if (projectileRenderer != null)
        {
            projectileRenderer.enabled = false;
        }

        // Wait for explosion duration
        yield return new WaitForSeconds(explosionDuration);

        // Destroy explosion sprite and projectile
        //if (explosionSprite != null)
        //{
        //    Destroy(explosionSprite);
        //}
        owner.GetMap().ProjectileManager.ReturnAreaProjectileToPool(this, prefab);
    }


    // Optional: Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, explosionRadius * Tile.GetUnitDistance());
    }
}