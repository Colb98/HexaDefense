using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    [SerializeField] private Map map;
    [SerializeField] private GameDataModel data;

    // these fire whenever gold or wave changes
    public event Action<int> OnGoldChanged;
    public event Action<int> OnWaveChanged;
    public event Action<int> OnHealthChanged;
    public event Action OnGameOverEvent;

    public string CharacterName { get; set; } = "William";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!map) map = GetComponent<Map>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Map GetMap()
    {
        return map;
    }

    public int GetGold()
    {
        // Placeholder for gold retrieval logic
        return data.Gold;
    }

    public int GetCurrentWave()
    {
        // Placeholder for current wave retrieval logic
        return data.Wave;
    }

    public int GetCurrentHealth()
    {
        // Placeholder for current health retrieval logic
        return data.BaseHealth;
    }

    public void IncrementWave()
    {
        data.IncrementWave();
        // Additional logic for wave increment can be added here
        OnWaveChanged?.Invoke(data.Wave); // Notify subscribers about wave change
    }

    public int UseGold(int amount)
    {
        var ret = data.UseGold(amount);
        if (ret >= 0)
        {
            OnGoldChanged?.Invoke(data.Gold); // Notify subscribers about gold change
        }
        return ret;
    }

    public int AddGold(int amount)
    {
        var ret = data.AddGold(amount);
        if (ret >= 0)
        {
            OnGoldChanged?.Invoke(data.Gold); // Notify subscribers about gold change
        }
        return ret;
    }

    public void DecreaseHealth(int amount)
    {
        data.DecreaseBaseHealth(amount);
        OnHealthChanged?.Invoke(data.BaseHealth); // Notify subscribers about health change
        if (data.BaseHealth <= 0)
        {
            // Handle game over logic here
            OnGameOver();
        }
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over!");
        // Clear map 
        map.ClearAllEntities();
        map.ResetMapData();
        map.UnitManager.Reset();
        map.TowerManager.Reset();

        PausableUpdateManager.instance.UnregisterAll();
        PausableUpdateManager.instance.Pause();
        OnGameOverEvent?.Invoke();
    }
}
