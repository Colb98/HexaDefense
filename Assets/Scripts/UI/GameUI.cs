using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelGold;
    [SerializeField] TextMeshProUGUI labelWave;

    void Start()
    {
        // set initial text
        UpdateGoldLabel(GameManager.Instance.GetGold());
        UpdateWaveLabel(GameManager.Instance.GetCurrentWave());

        Debug.LogWarning("GameUI, subscribing to GameManager events.");
        // subscribe to the GameManager events
        GameManager.Instance.OnGoldChanged += UpdateGoldLabel;
        GameManager.Instance.OnWaveChanged += UpdateWaveLabel;
    }

    private void OnDestroy()
    {
        Debug.LogWarning("GameUI OnDestroy called, unsubscribing from GameManager events.");
        // unsubscribe to avoid memory leaks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateGoldLabel;
            GameManager.Instance.OnWaveChanged -= UpdateWaveLabel;
        }
    }

    private void UpdateGoldLabel(int newGold)
    {
        labelGold.SetText($"Gold: {newGold}");
    }

    private void UpdateWaveLabel(int newWave)
    {
        labelWave.SetText($"Wave: {newWave}");
    }
}
