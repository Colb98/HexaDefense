using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CombatTextManager : MonoBehaviour
{
    public Camera worldCamera;              // if Canvas is Overlay, any world camera is fine
    public RectTransform canvasRect;
    public FloatingText prefab;
    public int prewarm = 32;
    public Vector2 jitter = new Vector2(30f, 10f);
    public List<CombatTextStyle> styles;

    readonly Queue<FloatingText> pool = new Queue<FloatingText>();
    readonly Dictionary<CombatTextType, CombatTextStyle> styleMap = new();

    void Awake()
    {
        foreach (var s in styles) styleMap[s.type] = s;

        for (int i = 0; i < prewarm; i++)
        {
            var ft = Instantiate(prefab, canvasRect);
            ft.Init(worldCamera, canvasRect);
            ft.gameObject.SetActive(false);
            pool.Enqueue(ft);
        }
    }

    FloatingText Get()
    {
        if (pool.Count == 0)
        {
            var ft = Instantiate(prefab, canvasRect);
            ft.Init(worldCamera, canvasRect);
            return ft;
        }
        return pool.Dequeue();
    }

    void Return(FloatingText ft)
    {
        ft.gameObject.SetActive(false);
        pool.Enqueue(ft);
    }

    // Public API
    public void ShowNumber(int amount, Vector3 worldPos, Transform follow = null, bool crit = false)
    {
        var type = crit ? CombatTextType.Crit : CombatTextType.Damage;
        var s = styleMap[type];
        var ft = Get();
        var txt = crit ? $"<b>{-amount}</b>" : (-amount).ToString();
        Debug.Log($"Show crit {crit} with style {s}");
        ft.Show(txt, worldPos, follow, s, RandomJitter());
        // Optional: schedule Return at end; here we let the item deactivate itself, then a small cleanup could re-enqueue
        StartCoroutine(ReturnWhenDisabled(ft));
    }

    public void ShowEffect(string tag, Vector3 worldPos, Transform follow = null, CombatTextType type = CombatTextType.Burn)
    {
        var s = styleMap[type];
        var ft = Get();
        ft.Show(tag, worldPos, follow, s, RandomJitter());
        StartCoroutine(ReturnWhenDisabled(ft));
    }

    System.Collections.IEnumerator ReturnWhenDisabled(FloatingText ft)
    {
        // wait until it deactivates itself
        while (ft.gameObject.activeSelf) yield return null;
        Return(ft);
    }

    Vector2 RandomJitter() => new Vector2(Random.Range(-jitter.x, jitter.x), Random.Range(0, jitter.y));
}
