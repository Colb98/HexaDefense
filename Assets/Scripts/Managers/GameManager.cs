using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    private Map map;
    [SerializeField] private GameDataModel data;

    // these fire whenever gold or wave changes
    public event Action<int> OnGoldChanged;
    public event Action<int> OnWaveChanged;

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
        map = GetComponent<Map>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
            Debug.Log("Invoke event with gold: " + data.Gold);
        }
        return ret;
    }
}
