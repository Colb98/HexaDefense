using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TowerHudUI : MonoBehaviour, IBlockerListener
{
    [SerializeField] private RectTransform hudPanel;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private UIClickBlocker blocker;

    private Tower currentTarget;
    private bool lockInteraction = false;

    private Coroutine lockCoroutine;

    void Awake()
    {
        hudPanel.gameObject.SetActive(false);
        upgradeButton.onClick.AddListener(OnUpgrade);
        removeButton.onClick.AddListener(OnRemove);
    }

    public void Show(Tower target, Vector3 worldPosition)
    {
        lockInteraction = true;
        currentTarget = target;
        hudPanel.gameObject.SetActive(true);
        // Convert world position to screen point and set HUD position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        hudPanel.position = screenPos + new Vector3(120, -40, 0);
        upgradeButton.gameObject.SetActive(target.CanUpgrade());

        if (lockCoroutine != null)
        {
            StopCoroutine(lockCoroutine);
        }

        lockCoroutine = StartCoroutine(UnlockInteractionAfterDelay(0.1f));
        blocker.SetBlockerListener(this);
    }

    private IEnumerator UnlockInteractionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lockInteraction = false;
    }

    public void OnBlockerPointerDown()
    {
        if (lockInteraction)
        {
            return;
        }
        Debug.Log("Blocker pointer down, hiding HUD.");
        Hide();
    }

    public void Hide()
    {
        currentTarget.HideAttackRange();
        currentTarget = null;
        hudPanel.gameObject.SetActive(false);
        lockInteraction = false;
        blocker.removeListener(this);
    }

    private void OnUpgrade()
    {
        Debug.Log($"Upgrading tower: {currentTarget.name}");
        GameManager.Instance.GetMap().TowerManager.UpgradeTower(currentTarget);
        Hide();
    }

    private void OnRemove()
    {
        Debug.Log($"Sell tower: {currentTarget.name}");
        GameManager.Instance.GetMap().TowerManager.SellTower(currentTarget);
        Hide();
    }
}
