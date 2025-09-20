using UnityEngine;

public class SlowDebuff : Buff
{
    public SlowDebuff(Entity entity, Entity source, float duration) : base("Slow", entity, source, duration)
    {
    }

    protected override void Initialize()
    {
        modifier.MovementSpeed.PercentBonus = -0.3f;
    }
}
