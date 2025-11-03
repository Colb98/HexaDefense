using UnityEngine;

[System.Serializable]
public class Buff
{
    public Stats.StatsModifier modifier;
    public float duration;
    public float timeRemaining;
    public string name;
    public Entity source;
    public Entity entity;

    public Buff(string name, Entity entity, Entity source, float duration)
    {
        this.name = name;
        this.modifier = new Stats.StatsModifier();
        this.duration = duration;
        this.timeRemaining = duration;
        this.source = source;
        this.entity = entity;

        Initialize();
    }

    protected virtual void Initialize()
    {
        // Custom initialization logic for derived classes
    }

    public void AddBuff(Buff newBuff)
    {
        if (newBuff.name != name || newBuff.entity != entity)
        {
            Debug.LogError("Cannot add buff: Buffs must have the same name and entity.");
            return;
        }

        if (newBuff.duration > duration)
        {
            duration = newBuff.duration;
        }
    }

    public void Tick(float deltaTime)
    {
        if (entity == null || entity.IsDead())
        {
            return;
        }
        timeRemaining -= deltaTime;
        if (timeRemaining <= 0)
        {
            entity.Stats.RemoveModifier(modifier);
            entity.RemoveBuff(this);
        }
    }
}
