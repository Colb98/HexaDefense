using System.Collections.Generic;
using Newtonsoft.Json;

public static class JsonUtilityWrapper
{
    [System.Serializable]
    private class TowerDictWrapper
    {
        public List<TowerEntry> towers;
    }

    [System.Serializable]
    private class TowerEntry
    {
        public string key;
        public TowerConfig value;
    }

    [System.Serializable]
    private class EnemyDictWrapper
    {
        public List<EnemyEntry> enemies;
    }

    [System.Serializable]
    private class EnemyEntry
    {
        public string key;
        public EnemyConfig value;
    }

    public static Dictionary<string, TowerConfig> LoadTowerConfigs(string json)
    {
        return JsonConvert.DeserializeObject<Dictionary<string, TowerConfig>>(json); // Use Newtonsoft.Json
    }

    public static Dictionary<string, EnemyConfig> LoadEnemyConfigs(string json)
    {
        return JsonConvert.DeserializeObject<Dictionary<string, EnemyConfig>>(json);
    }
}
