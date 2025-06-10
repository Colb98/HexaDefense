using System;
using UnityEngine;


[Serializable]
public class GameDataModel
{
    [SerializeField] private int gold = 0;
    [SerializeField] private int wave = 0;

    [SerializeField] private int score = 0;
    [SerializeField] private int baseHealth = 100;

    public int Gold
    {
        get => gold;
        set => gold = value;
    }

    public int Wave
    {
        get => wave;
    }

    public int Score
    {
        get => score;
        set => score = value;
    }

    public void IncrementWave()
    {
        wave++;
    }

    public int UseGold(int amount)
    {
        if (amount <= gold)
        {
            gold -= amount;
            return gold;
        }
        else
        {
            Debug.LogError("Not enough gold to use the specified amount.");
            return -1;
        }
    }

    public int AddGold(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("Cannot add a negative amount of gold.");
            return -1;
        }
        gold += amount;
        return gold;
    }
}
