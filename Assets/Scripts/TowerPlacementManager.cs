
// TowerPlacementManager.cs - Handles the placement of towers on the map
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
        if (towerManager != null && towerManager.CanTowerBePlaced(center, 2))
        {
            towerManager.PlaceTower(center, 2, selectedTowerType, selectedTowerLevel);
            return true;
        }
        return false;
    }
}