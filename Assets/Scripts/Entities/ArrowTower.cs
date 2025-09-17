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
        bool isCrit = UnityEngine.Random.Range(0, 100) < Stats.CritChance.Value;
        HomingProjectile arrow = _map.ProjectileManager.CreateHomingProjectile(arrowPrefab, transform.position, target, GetPhysicalDamage(isCrit), GetMagicalDamage(isCrit), arrowSpeed, isCrit);
        arrow.SetOwner(this);
    }
}
