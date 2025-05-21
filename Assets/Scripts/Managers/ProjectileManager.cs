// Projectile Manager to handle projectile creation
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    [SerializeField] private Map _map;

    // Pooling collections for different projectile types
    private Dictionary<GameObject, Queue<AreaProjectile>> areaProjectilePools = new Dictionary<GameObject, Queue<AreaProjectile>>();
    private Dictionary<GameObject, Queue<HomingProjectile>> homingProjectilePools = new Dictionary<GameObject, Queue<HomingProjectile>>();

    // Pool size settings
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;

    public void SetMap(Map map)
    {
        _map = map;
    }

    // Initialize pool for a specific area projectile prefab
    private void InitializeAreaProjectilePool(GameObject prefab)
    {
        if (!areaProjectilePools.ContainsKey(prefab))
        {
            Queue<AreaProjectile> pool = new Queue<AreaProjectile>();

            // Create initial pool of projectiles
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject projectileObj = Instantiate(prefab, Vector3.zero, Quaternion.identity, _map.transform);
                AreaProjectile projectile = projectileObj.GetComponent<AreaProjectile>();
                projectile.gameObject.SetActive(false);
                pool.Enqueue(projectile);
                projectile.SetPrefab(prefab);
            }

            areaProjectilePools[prefab] = pool;
        }
    }

    // Initialize pool for a specific homing projectile prefab
    private void InitializeHomingProjectilePool(GameObject prefab)
    {
        if (!homingProjectilePools.ContainsKey(prefab))
        {
            Queue<HomingProjectile> pool = new Queue<HomingProjectile>();

            // Create initial pool of projectiles
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject projectileObj = Instantiate(prefab, Vector3.zero, Quaternion.identity, _map.transform);
                HomingProjectile projectile = projectileObj.GetComponent<HomingProjectile>();
                projectile.gameObject.SetActive(false);
                pool.Enqueue(projectile);
                projectile.SetPrefab(prefab);
            }

            homingProjectilePools[prefab] = pool;
        }
    }

    // Create an area projectile
    public AreaProjectile CreateAreaProjectile(GameObject prefab, Vector3 origin, Vector3 target, float radius, float physDamage, float magDamage, float speed)
    {
        // Initialize pool if not exists
        InitializeAreaProjectilePool(prefab);

        // Get pool for this prefab
        Queue<AreaProjectile> pool = areaProjectilePools[prefab];
        AreaProjectile projectile;

        // If no inactive projectiles in pool, create a new one if under max size
        if (pool.Count == 0)
        {
            if (areaProjectilePools[prefab].Count < maxPoolSize)
            {
                // Create a new projectile
                GameObject projectileObj = Instantiate(prefab, origin, Quaternion.identity, _map.transform);
                projectile = projectileObj.GetComponent<AreaProjectile>();
                projectile.SetPrefab(prefab);
            }
            else
            {
                Debug.LogWarning("Max pool size reached. Cannot create more projectiles.");
                return null;
            }
        }
        else
        {
            // Get an inactive projectile from pool
            projectile = pool.Dequeue();
        }

        // Reset and activate the projectile
        projectile.transform.position = origin;
        projectile.gameObject.SetActive(true);
        projectile.Initialize(target, radius, physDamage, magDamage, speed);

        return projectile;
    }

    // Create a homing projectile
    public HomingProjectile CreateHomingProjectile(GameObject prefab, Vector3 origin, Entity target, float physDamage, float magDamage, float speed)
    {
        // Initialize pool if not exists
        InitializeHomingProjectilePool(prefab);

        // Get pool for this prefab
        Queue<HomingProjectile> pool = homingProjectilePools[prefab];
        HomingProjectile projectile;

        // If no inactive projectiles in pool, create a new one if under max size
        if (pool.Count == 0)
        {
            if (homingProjectilePools[prefab].Count < maxPoolSize)
            {
                // Create a new projectile
                GameObject projectileObj = Instantiate(prefab, origin, Quaternion.identity, _map.transform);
                projectile = projectileObj.GetComponent<HomingProjectile>();
                projectile.SetPrefab(prefab);
            }
            else
            {
                Debug.LogWarning("Max pool size reached. Cannot create more projectiles.");
                return null;
            }
        }
        else
        {
            // Get an inactive projectile from pool
            projectile = pool.Dequeue();
        }

        // Reset and activate the projectile
        projectile.transform.position = origin;
        projectile.gameObject.SetActive(true);
        projectile.Initialize(target, physDamage, magDamage, speed);

        return projectile;
    }

    // Return an area projectile to its pool
    public void ReturnAreaProjectileToPool(AreaProjectile projectile, GameObject prefab)
    {
        projectile.gameObject.SetActive(false);
        areaProjectilePools[prefab].Enqueue(projectile);
        //Debug.Log("Returned area projectile to pool: " + prefab.name + " | Pool size: " + areaProjectilePools[prefab].Count);
    }

    // Return a homing projectile to its pool
    public void ReturnHomingProjectileToPool(HomingProjectile projectile, GameObject prefab)
    {
        projectile.gameObject.SetActive(false);
        homingProjectilePools[prefab].Enqueue(projectile);
        //Debug.Log("Returned homing projectile to pool: " + prefab.name + " | Pool size: " + homingProjectilePools[prefab].Count);
    }
}