using System.Collections;
using UnityEngine;

public class FlashTower : Tower
{
    [SerializeField] private Color flashColor;

    public override void Initialize(int size, Vector2Int position, string type, TowerLevelData data, TowerConfig config)
    {
        base.Initialize(size, position, type, data, config);
    }

    protected override void PerformAttack()
    {
        bool isCrit = UnityEngine.Random.Range(0, 100) < Stats.CritChance.Value;
        _map.ProjectileManager.CreateInstantAttack(target, GetPhysicalDamage(isCrit), GetMagicalDamage(isCrit), isCrit);
        target.SetStun(isCrit);
        GetComponent<SpriteRenderer>().color = flashColor;
        StartCoroutine(BackToBaseColor());
    }

    private IEnumerator BackToBaseColor()
    {
        // Wait for explosion duration
        yield return new WaitForSeconds(0.05f);
        Debug.Log($"Back to original color {originalColor}");
        GetComponent<SpriteRenderer>().color = originalColor;
    }
}
