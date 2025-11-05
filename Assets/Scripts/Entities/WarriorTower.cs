using System.Collections;
using UnityEngine;

public class WarriorTower : Tower
{
    public override void Initialize(int size, Vector2Int position, string type, TowerLevelData data, TowerConfig config)
    {
        base.Initialize(size, position, type, data, config);
    }

    protected override void PerformAttack()
    {
        bool isCrit = Random.Range(0, 100) < Stats.CritChance.Value;
        _map.ProjectileManager.CreateInstantAttack(target, GetPhysicalDamage(isCrit), GetMagicalDamage(isCrit), isCrit);
        GetComponent<SpriteRenderer>().color = Color.white;
        StartCoroutine(BackToBaseColor());
    }
    private IEnumerator BackToBaseColor()
    {
        // Wait for explosion duration
        yield return new WaitForSeconds(0.05f);
        GetComponent<SpriteRenderer>().color = originalColor;
    }
}
