
// TowerPlacementManager.cs - Handles the placement of towers on the map
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance { get; private set; }

    public TowerManager towerManager;

    [SerializeField] private string selectedTowerType;
    [SerializeField] private int selectedTowerSize = 1;
    [SerializeField] private int selectedTowerLevel = 1;
    [SerializeField] private bool canPlaceTower = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectedTower(string towerType, int towerSize, int towerLevel)
    {
        selectedTowerType = towerType;
        selectedTowerSize = towerSize;
        selectedTowerLevel = towerLevel;
        canPlaceTower = true;
    }

    public void ClearSelectedTower()
    {
        selectedTowerType = string.Empty;
        canPlaceTower = false;
    }

    public bool PlaceTower(Tile tile)
    {
        var center = new Vector2Int(tile.x, tile.y);
        var towerConfig = GameConfigManager.Instance.Towers[selectedTowerType];
        var levelConfig = towerConfig.levels.FirstOrDefault(lvl => lvl.level == 1);
        int price = levelConfig != null ? levelConfig.cost : 0;
        if (towerManager == null)
        {
            Debug.LogError("TowerManager reference is missing.");
            return false;
        }

        if (!towerManager.CanTowerBePlaced(center, 2))
        {
            Debug.LogError("Cannot place tower at the selected location.");
            return false;
        }

        if (GameManager.Instance.GetGold() < price)
        {
            Debug.LogError("Not enough gold to place tower.");
            return false;
        }

        return towerManager.PlaceTower(center, 2, selectedTowerType, 1) != null;
    }
}