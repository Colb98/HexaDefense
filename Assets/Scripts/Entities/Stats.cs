
using System.Runtime.InteropServices.WindowsRuntime;

public class Stats
{
    public class Stat
    {
        public string Name;
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
    }

    public Stat HealthPoint = new();
    public float CurrentHealth = new();
    public Stat PhysicalDamage = new();
    public Stat MagicalDamage = new();
    public Stat PhysicResist = new();
    public Stat MagicResist = new();
    public Stat MovementSpeed = new();
    public Stat AttackSpeed = new();
    public Stat AttackRange = new();
    public Stat CritChance = new();
    public Stat CritDamage = new();

    public float GetSecondsPerAttack()
    {
        return 1.0f / AttackSpeed.Value;
    }
}
