using UnityEngine;

public class FireballTower : Tower
{
    [SerializeField] private float fireballSpeed = 5f;
    [SerializeField] private float fireballRange = 3f;
    [SerializeField] private float bigFireballDamage = 3f;

    private int fireballCount = 0;
    private static int bigFireballIndex = 3;

    [SerializeField] private GameObject fireballPrefab;

    public override void Initialize(int size, Vector2Int position, string type, TowerLevelData data, TowerConfig config)
    {
        base.Initialize(size, position, type, data, config);
        fireballSpeed = config.attackProjectileSpeed;
    }

    protected override void PerformAttack()
    {
        if (fireballCount < bigFireballIndex)
        {
            fireballCount++;
            AreaProjectile fireball = _map.ProjectileManager.CreateAreaProjectile(fireballPrefab, transform.localPosition, target.transform.localPosition, fireballRange, physicalDamage, magicalDamage, fireballSpeed);
            fireball.SetOwner(this);
            //Debug.Log($"Throw normal fireball with magic damage {magicalDamage}");
        }
        else
        {
            AreaProjectile fireball = _map.ProjectileManager.CreateAreaProjectile(fireballPrefab, transform.localPosition, target.transform.localPosition, fireballRange, bigFireballDamage * physicalDamage, bigFireballDamage * magicalDamage, fireballSpeed);
            fireball.SetOwner(this);
            // Reset the count for the next attack
            fireballCount = 0;
            //Debug.Log($"Throw big fireball with magic damage {bigFireballDamage * magicalDamage}");
        }
    }
}
