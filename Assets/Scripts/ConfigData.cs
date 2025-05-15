using System.Collections.Generic;

[System.Serializable]
public class TowerConfig
{
    public string type;
    public string damageType;
    public float range;
    public float attackSpeed;
    public float damageRadius;
    public float attackProjectileSpeed;
    public List<TowerLevelData> levels;
}

[System.Serializable]
public class TowerLevelData
{
    public int level;
    public float physicalDamage;
    public float magicalDamage;
    public float health;
    public int cost;
    public int upgradeCost;
}
[System.Serializable]
public class EnemyConfig
{
    public string type;
    public string baseDescription;
    public string damageType;
    public List<EnemyLevelData> levels;
}

[System.Serializable]
public class EnemyLevelData
{
    public int level;
    public float health;
    public float physicalDamage;
    public float magicalDamage;
    public float moveSpeed;
    public int reward;
    public ResistanceData resistances;
}

[System.Serializable]
public class ResistanceData
{
    public float physical;
    public float magical;
}
