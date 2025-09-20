using UnityEngine;

public class StunDebuff : Buff
{
    public StunDebuff(Entity entity, Entity source, float duration) : base("Stun", entity, source, duration)
    {
    }

    protected override void Initialize()
    {
        modifier.MovementSpeed.PercentBonus = -1f;
    }
}
