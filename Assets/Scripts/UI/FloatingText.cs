using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform rect;
    public TextMeshProUGUI tmp;
    public CanvasGroup canvasGroup;

    // runtime state
    Vector3 worldPos;         // or fixed world position
    Camera cam;
    public RectTransform canvasRect;

    // anim
    float t;
    CombatTextStyle style;
    Vector2 startScreenLocal;
    Vector2 endScreenLocal;

    public void Init(Camera camera, RectTransform canvas)
    {
        cam = camera;
        canvasRect = canvas;
    }

    public void Show(string text, Vector3 worldPosition, Transform followTarget, CombatTextStyle s, Vector2 randomJitter)
    {
        style = s;
        worldPos = worldPosition;

        tmp.text = text;
        tmp.color = s.color;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = Mathf.Min(s.endSize, s.startSize) * 0.6f;
        tmp.fontSizeMax = Mathf.Max(s.endSize, s.startSize);

        t = 0f;
        Vector2 startLocal = WorldToCanvasLocal(worldPos) + randomJitter;
        startScreenLocal = startLocal;
        endScreenLocal = startLocal + s.rise;

        rect.anchoredPosition = startLocal;
        rect.localScale = Vector3.one * s.scale.Evaluate(0);
        canvasGroup.alpha = s.alpha.Evaluate(0);

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (style == null) return;

        t += Time.deltaTime / Mathf.Max(0.0001f, style.duration);
        float tt = Mathf.Clamp01(t);

        // Lerp position
        rect.anchoredPosition = Vector2.Lerp(startScreenLocal, endScreenLocal, tt);

        // Scale & alpha over time
        float scl = style.scale.Evaluate(tt);
        rect.localScale = Vector3.one * scl;
        canvasGroup.alpha = style.alpha.Evaluate(tt);

        if (tt >= 1f)
        {
            gameObject.SetActive(false);
        }
    }

    Vector2 WorldToCanvasLocal(Vector3 world)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out var local);
        return local;
    }
}
