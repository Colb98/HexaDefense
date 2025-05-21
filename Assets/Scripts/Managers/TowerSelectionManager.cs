// TowerSelectionManager.cs - Manages the selection of towers from the UI
using System.Collections.Generic;
using UnityEngine;

public class TowerSelectionManager : MonoBehaviour
{
    public List<TowerCard> towerCards = new List<TowerCard>();
    public TowerManager towerManager;

    private TowerCard selectedCard;
    private bool isTowerSelected = false;

    private void Start()
    {
        // Find the TowerCardUI component
        TowerCardUI cardUI = FindFirstObjectByType<TowerCardUI>();

        // Initialize the tower cards
        cardUI.InitializeTowerCards();

        // Find all tower cards in the UI
        TowerCard[] cards = GetComponentsInChildren<TowerCard>();
        towerCards.AddRange(cards);
    }

    public void SelectTower(TowerCard card)
    {
        // Deselect any previously selected card
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }

        // Select the new card
        selectedCard = card;
        selectedCard.SetSelected(true);
        isTowerSelected = true;

        // Notify the TowerPlacementManager of the selected tower type
        TowerPlacementManager.Instance.SetSelectedTower(card.towerType, card.towerSize, card.towerLevel);
    }

    public void DeselectTower()
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
            selectedCard = null;
            isTowerSelected = false;

            // Notify the TowerPlacementManager that no tower is selected
            TowerPlacementManager.Instance.ClearSelectedTower();
        }
    }

    public bool IsTowerSelected()
    {
        return isTowerSelected;
    }

    public string GetSelectedTowerType()
    {
        return selectedCard != null ? selectedCard.towerType : string.Empty;
    }

    public int GetSelectedTowerSize()
    {
        return selectedCard != null ? selectedCard.towerSize : 1;
    }

    public int GetSelectedTowerLevel()
    {
        return selectedCard != null ? selectedCard.towerLevel : 1;
    }
}