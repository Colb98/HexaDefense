using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatLine : MonoBehaviour
{
    [System.Serializable]
    public struct SpriteEntry {
        public string name;
        public Sprite sprite;
    }
    [SerializeField] Stats.Stat stat;
    [SerializeField] TMPro.TextMeshProUGUI statNameText;
    [SerializeField] TMPro.TextMeshProUGUI baseValueText;
    [SerializeField] TMPro.TextMeshProUGUI bonusValueText;
    [SerializeField] Image statIcon;

    [SerializeField] List<SpriteEntry> sprites;

    public void Initialize(Stats.Stat stat) {
        this.stat = stat;
        statNameText.text = stat.Name;
        baseValueText.text = stat.Base.ToString("0.00");
        bonusValueText.text = (stat.Value - stat.Base).ToString("0.00");

        bonusValueText.gameObject.SetActive(stat.Value != stat.Base);

        Sprite s = null;
        foreach (var entry in sprites) {
            if (entry.name == stat.Name) {
                s = entry.sprite;
                break;
            }
        }
        if (s != null) {
            statIcon.sprite = s;
        }
        else
        {
            statIcon.sprite = sprites[0].sprite;
        }
    }

    public void Update()
    {
        baseValueText.text = stat.Base.ToString("0.00");
        bonusValueText.text = (stat.Value - stat.Base).ToString("0.00");
        bonusValueText.gameObject.SetActive(stat.Value != stat.Base);
    }
}
