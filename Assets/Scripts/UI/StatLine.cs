using UnityEngine;

public class StatLine : MonoBehaviour
{
    [SerializeField] Stats.Stat stat;
    [SerializeField] TMPro.TextMeshProUGUI statNameText;
    [SerializeField] TMPro.TextMeshProUGUI baseValueText;
    [SerializeField] TMPro.TextMeshProUGUI bonusValueText;

    public void Initialize(Stats.Stat stat) {
        statNameText.text = stat.Name;
        baseValueText.text = stat.Base.ToString("0.00");
        bonusValueText.text = (stat.Value - stat.Base).ToString("0.00");

        bonusValueText.gameObject.SetActive(stat.Value != stat.Base);
    }
}
