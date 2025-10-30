using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameConfigManager : MonoBehaviour
{
    public static GameConfigManager Instance { get; private set; }

    public Dictionary<string, TowerConfig> Towers { get; private set; }
    public Dictionary<string, EnemyConfig> Enemies { get; private set; }

    [SerializeField] private TextAsset towerJson;
    [SerializeField] private TextAsset enemyJson;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfigs();
    }

    private void LoadConfigs()
    {
        Towers = JsonUtilityWrapper.LoadTowerConfigs(towerJson.text);
        Enemies = JsonUtilityWrapper.LoadEnemyConfigs(enemyJson.text);

        Debug.Log("Tower Configs Loaded: " + Towers.Count + " with keys: " + Towers.Keys);
        Debug.Log("Enemies Configs Loaded: " + Enemies.Count + " with keys: " + Enemies.Keys);
    }

    public int GetTowerPrice(string towerType, int level = 1)
    {
        var towerConfig = Towers[towerType];
        var levelConfig = towerConfig.levels.Find(lvl => lvl.level == level);
        return levelConfig != null ? levelConfig.cost : 0;
    }
}