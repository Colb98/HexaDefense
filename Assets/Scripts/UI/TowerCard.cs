using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TowerCard : MonoBehaviour, IPointerClickHandler
{
    public string towerType;
    public int towerSize = 2;
    public int towerLevel = 1;
    public Image towerImage;
    public Image selectedImage;
    public TextMeshProUGUI towerNameText;

    private TowerSelectionManager selectionManager;

    private void Start()
    {
        selectionManager = FindFirstObjectByType<TowerSelectionManager>();
        selectedImage.enabled = false;

        // Set the tower name text
        towerNameText.text = towerType;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selectionManager.SelectTower(this);
    }

    public void SetSelected(bool isSelected)
    {
        selectedImage.enabled = isSelected;
    }
}