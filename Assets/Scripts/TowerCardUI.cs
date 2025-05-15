// TowerCardUI.cs - Helper script to create tower cards programmatically
using UnityEngine;

public class TowerCardUI : MonoBehaviour
{
    public GameObject towerCardPrefab;
    public Transform cardContainer;

    public void CreateTowerCard(string name, string towerType, Sprite towerSprite, int size = 2, int level = 1)
    {
        GameObject cardObject = Instantiate(towerCardPrefab, cardContainer);
        TowerCard card = cardObject.GetComponent<TowerCard>();

        if (card != null)
        {
            Debug.Log(card.towerImage.ToString() + " " + card.towerImage.name);
            card.towerType = towerType;
            card.towerSize = size;
            card.towerLevel = level;
            card.towerImage.sprite = towerSprite;
            card.towerNameText.text = towerType;
        }
    }

    // Call this from Start or Awake to initialize your tower cards
    public void InitializeTowerCards()
    {
        // Load tower sprites or use assigned sprites
        Sprite fireballTowerSprite = Resources.Load<Sprite>("Sprites/Avatars/fireball_tower");
        Sprite arrowTowerSprite = Resources.Load<Sprite>("Sprites/Avatars/arrow_tower");

        // Create cards for different tower types
        CreateTowerCard("Fireball Tower", "FireballTower", fireballTowerSprite);
        CreateTowerCard("Arrow Tower", "ArrowTower", arrowTowerSprite);
    }
}