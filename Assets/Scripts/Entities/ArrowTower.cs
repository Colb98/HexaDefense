using UnityEngine;

public class ArrowTower : Tower
{
    [SerializeField] private float arrowSpeed;
    [SerializeField] private GameObject arrowPrefab;

    public override void Initialize(int size, Vector2Int position, string type, TowerLevelData data, TowerConfig config)
    {
        base.Initialize(size, position, type, data, config);
        arrowSpeed = config.attackProjectileSpeed;
    }

    protected override void PerformAttack()
    {
        HomingProjectile arrow = _map.ProjectileManager.CreateHomingProjectile(arrowPrefab, transform.position, target, physicalDamage, magicalDamage, arrowSpeed);
        arrow.SetOwner(this);
    }
}
