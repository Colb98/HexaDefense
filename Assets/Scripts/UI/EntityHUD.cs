using System.Collections;
using TMPro;
using UnityEngine;

public class EntityHUD : MonoBehaviour, IBlockerListener
{
    [SerializeField] Entity entity;
    [SerializeField] TextMeshProUGUI labelName;
    [SerializeField] GameObject statListPanel;
    [SerializeField] StatLine statLinePrefab;
    [SerializeField] private UIClickBlocker blocker;

    private bool lockInteraction = false;
    private Coroutine lockCoroutine;

    public void ShowEntity(Entity entity)
    {
        this.gameObject.SetActive(true);
        this.entity = entity;
        labelName.SetText(entity.name);
        statListPanel.SetActive(true);
        statListPanel.transform.DetachChildren();
        entity.Stats.GetStats().ForEach(stat =>
        {
            StatLine statLine = Instantiate(statLinePrefab, statListPanel.transform);
            statLine.Initialize(stat);
        });
        lockInteraction = true;

        if (lockCoroutine != null)
        {
            StopCoroutine(lockCoroutine);
        }

        lockCoroutine = StartCoroutine(UnlockInteractionAfterDelay(0.1f));
        blocker.SetBlockerListener(this);
    }

    void Update()
    {
        if (entity.IsDead())
        {
            Hide();
        }
    }

    public void Hide()
    {
        this.entity = null;
        this.gameObject.SetActive(false);
        blocker.removeListener(this);
    }

    private IEnumerator UnlockInteractionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lockInteraction = false;
    }

    public bool IsLocked()
    {
        return lockInteraction;
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
}
