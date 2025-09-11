using UnityEngine;
using TMPro;

public enum CombatTextType { Damage, Crit, Heal, Burn, Freeze, Poison, Stun, Miss }

[System.Serializable]
public class CombatTextStyle
{
    public CombatTextType type;
    public Color color = Color.white;
    public float startSize = 36f;
    public float endSize = 28f;
    public float duration = 0.8f;
    public Vector2 rise = new Vector2(0f, 80f);     // how far it floats (in UI px)
    public AnimationCurve alpha = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public AnimationCurve scale = AnimationCurve.EaseInOut(0, 1.1f, 1, 1f);
    public Sprite icon;                              // optional (e.g., flame)
    public bool shakeOnCrit = false;
}
