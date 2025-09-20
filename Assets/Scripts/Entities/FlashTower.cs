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
        if (isCrit)
        {
            Buff buff = new StunDebuff(target, this, 1f);
            target.AddBuff(buff);

            buff = new SlowDebuff(target, this, 2f);
            target.AddBuff(buff);
        }
        else
        {
            Buff buff = new SlowDebuff(target, this, 2f);
            target.AddBuff(buff);
        }
        GetComponent<SpriteRenderer>().color = flashColor;
        StartCoroutine(BackToBaseColor());
    }

    private IEnumerator BackToBaseColor()
    {
        // Wait for explosion duration
        yield return new WaitForSeconds(0.05f);
        GetComponent<SpriteRenderer>().color = originalColor;
    }
}
