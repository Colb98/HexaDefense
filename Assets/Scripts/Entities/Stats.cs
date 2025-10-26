using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats
{
    [System.Serializable]
    public class Stat
    {
        [SerializeField] private string _name = "";
        public string Name 
        {
            get => _name;
            set => _name = value;
        }

        public Stat(string name)
        {
            Name = name;
        }

        public Stat() { }

        private bool IsDirty;

        private float _value;
        public float Value
        {
            get
            {
                if (IsDirty)
                {
                    _value = (((Base + BaseFlatBonus) * (1 + BasePercentBonus)) + FlatBonus) * (1 + PercentBonus);
                    IsDirty = false;
                }
                return _value;
            }
            private set
            {
                _value = value;
            }
        }

        // Private backing fields
        private float _base;
        private float _baseFlatBonus;
        private float _basePercentBonus;
        private float _flatBonus;
        private float _percentBonus;

        // (((Base + BaseFlatBonus) * (1 + BasePercentBonus)) + FlatBonus) * (1 + PercentBonus)
        public float Base 
        { 
            get => _base; 
            set {
                _base = value;
                IsDirty = true;
            } 
        }
        public float BaseFlatBonus
        {
            get => _baseFlatBonus;
            set
            {
                _baseFlatBonus = value;
                IsDirty = true;
            }
        }
        public float BasePercentBonus
        {
            get => _basePercentBonus;
            set
            {
                _basePercentBonus = value;
                IsDirty = true;
            }
        }
        public float FlatBonus
        {
            get => _flatBonus;
            set
            {
                _flatBonus = value;
                IsDirty = true;
            }
        }
        public float PercentBonus
        {
            get => _percentBonus;
            set
            {
                _percentBonus = value;
                IsDirty = true;
            }
        }

        public float GetBonus ()
        {
            return Value - Base;
        }

        public void AddStatModifier(StatModifier modifier)
        {
            Base += modifier.Base;
            BaseFlatBonus += modifier.BaseFlatBonus;
            BasePercentBonus += modifier.BasePercentBonus;
            FlatBonus += modifier.FlatBonus;
            PercentBonus += modifier.PercentBonus;
        }

        public void RemoveStatModifier(StatModifier modifier)
        {
            Base -= modifier.Base;
            BaseFlatBonus -= modifier.BaseFlatBonus;
            BasePercentBonus -= modifier.BasePercentBonus;
            FlatBonus -= modifier.FlatBonus;
            PercentBonus -= modifier.PercentBonus;
        }
    }

    public class StatModifier
    {
        public float Base = 0.0f;
        public float BaseFlatBonus = 0.0f;
        public float BasePercentBonus = 0.0f;
        public float FlatBonus = 0.0f;
        public float PercentBonus = 0.0f;
    }

    public class StatsModifier
    {
        public StatModifier HealthPoint = new();
        public StatModifier PhysicalDamage = new();
        public StatModifier MagicalDamage = new();
        public StatModifier PhysicResist = new();
        public StatModifier MagicResist = new();
        public StatModifier MovementSpeed = new();
        public StatModifier AttackSpeed = new();
        public StatModifier AttackRange = new();
        public StatModifier CritChance = new();
        public StatModifier CritDamage = new();
    }

    public Stat HealthPoint;
    public float CurrentHealth;
    public Stat PhysicalDamage;
    public Stat MagicalDamage;
    public Stat PhysicResist;
    public Stat MagicResist;
    public Stat MovementSpeed;
    public Stat AttackSpeed;
    public Stat AttackRange;
    public Stat CritChance;
    public Stat CritDamage;
    public Dictionary<string, Stat> CustomStats = new();

    private List<StatsModifier> Modifiers = new();

    public Stats()
    {
        HealthPoint = new Stat("Health Point");
        CurrentHealth = HealthPoint.Value;
        PhysicalDamage = new Stat("Physical Damage");
        MagicalDamage = new Stat("Magical Damage");
        PhysicResist = new Stat("Physic Resist");
        MagicResist = new Stat("Magic Resist");
        MovementSpeed = new Stat("Movement Speed");
        AttackSpeed = new Stat("Attack Speed");
        AttackRange = new Stat("Attack Range");
        CritChance = new Stat("Crit Chance");
        CritDamage = new Stat("Crit Damage");
    }

    public float GetSecondsPerAttack()
    {
        return 1.0f / AttackSpeed.Value;
    }

    public void AddModifier(StatsModifier modifier)
    {
        Modifiers.Add(modifier);
        HealthPoint.AddStatModifier(modifier.HealthPoint);
        PhysicalDamage.AddStatModifier(modifier.PhysicalDamage);
        MagicalDamage.AddStatModifier(modifier.MagicalDamage);
        PhysicResist.AddStatModifier(modifier.PhysicResist);
        MagicResist.AddStatModifier(modifier.MagicResist);
        MovementSpeed.AddStatModifier(modifier.MovementSpeed);
        AttackSpeed.AddStatModifier(modifier.AttackSpeed);
        AttackRange.AddStatModifier(modifier.AttackRange);
        CritChance.AddStatModifier(modifier.CritChance);
        CritDamage.AddStatModifier(modifier.CritDamage);
    }

    public void RemoveModifier(StatsModifier modifier)
    {
        Modifiers.Remove(modifier);
        HealthPoint.RemoveStatModifier(modifier.HealthPoint);
        PhysicalDamage.RemoveStatModifier(modifier.PhysicalDamage);
        MagicalDamage.RemoveStatModifier(modifier.MagicalDamage);
        PhysicResist.RemoveStatModifier(modifier.PhysicResist);
        MagicResist.RemoveStatModifier(modifier.MagicResist);
        MovementSpeed.RemoveStatModifier(modifier.MovementSpeed);
        AttackSpeed.RemoveStatModifier(modifier.AttackSpeed);
        AttackRange.RemoveStatModifier(modifier.AttackRange);
        CritChance.RemoveStatModifier(modifier.CritChance);
        CritDamage.RemoveStatModifier(modifier.CritDamage);
    }

    // Clear all, useful for when an entity dies
    public void ClearAllModifiers ()
    {
        foreach (var modifier in Modifiers)
        {
            HealthPoint.RemoveStatModifier(modifier.HealthPoint);
            PhysicalDamage.RemoveStatModifier(modifier.PhysicalDamage);
            MagicalDamage.RemoveStatModifier(modifier.MagicalDamage);
            PhysicResist.RemoveStatModifier(modifier.PhysicResist);
            MagicResist.RemoveStatModifier(modifier.MagicResist);
            MovementSpeed.RemoveStatModifier(modifier.MovementSpeed);
            AttackSpeed.RemoveStatModifier(modifier.AttackSpeed);
            AttackRange.RemoveStatModifier(modifier.AttackRange);
            CritChance.RemoveStatModifier(modifier.CritChance);
            CritDamage.RemoveStatModifier(modifier.CritDamage);
        }
        Modifiers.Clear();
    }

    public void AddStat(Stat stat)
    {
        CustomStats[stat.Name] = stat;
    }

    public void RemoveStat(string statName)
    {
        if (CustomStats.ContainsKey(statName))
        {
            CustomStats.Remove(statName);
        }
    }

    public List<Stat> GetStats()
    {
        var ret = new List<Stat>()
        {
            HealthPoint,
            PhysicalDamage,
            MagicalDamage,
            PhysicResist,
            MagicResist,
            MovementSpeed,
            AttackSpeed,
            AttackRange,
            CritChance,
            CritDamage
        };
        ret.AddRange(CustomStats.Values);

        return ret;
    }
}
