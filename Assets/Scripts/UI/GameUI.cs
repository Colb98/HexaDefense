using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class GameUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI labelGold;
    [SerializeField] TextMeshProUGUI labelWave;
    [SerializeField] TowerHudUI hud;
    [SerializeField] EntityHUD entityHUD;

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

    public void ShowTowerHud(Tower target, Vector3 worldPosition)
    {
        StartCoroutine(ShowHudNextFrame(target, worldPosition));
    }

    private IEnumerator ShowHudNextFrame(Tower targetTower, Vector3 worldPos)
    {
        yield return null; // Wait for the next frame
        hud.Show(targetTower, worldPos);
    }

    public void ShowEntityHUD(Entity entity)
    {
        StartCoroutine(ShowEntityHudNextFrame(entity));
    }

    private IEnumerator ShowEntityHudNextFrame(Entity entity)
    {
        yield return null; // Wait for the next frame
        entityHUD.ShowEntity(entity);
    }
}
