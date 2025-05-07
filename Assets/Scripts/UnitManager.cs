using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private Map map;
    [Header("Mobs spawn & Path")]
    public GameObject enemyPrefab;
    [SerializeField] private float mobSpawnInterval = 2f;
    [SerializeField] private float timerGenerateEnemy = 0.0f;
    private List<Unit> enemies = new List<Unit>();
    private Vector2Int start = new Vector2Int(-1, -1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timerGenerateEnemy = mobSpawnInterval + 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (start.x < 0)
        {
            start = map.GetStartCoord();
            if (start.x < 0)
            {
                return;
            }
        }

        timerGenerateEnemy += Time.deltaTime;
        if (timerGenerateEnemy >= mobSpawnInterval)
        {
            GameObject enemy = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
            Unit unit = enemy.GetComponent<Unit>();
            unit.map = map;
            unit.SetCoord(start);
            var path = FindPath(unit.position);
            //Debug.Log($"Path {path.Count} from {unit.position} to GOAL");
            unit.MoveByPath(path);
            enemies.Add(unit);
            timerGenerateEnemy = 0.0f;
            enemy.transform.SetParent(map.transform); // Ensure parent is Map even when pooled
        }
    }

    public void UpdateUnitPaths()
    {
        foreach (var unit in enemies)
        {
            unit.StopMoving();
            var path = FindPath(unit.position);
            unit.MoveByPath(path);
        }
    }

    private List<Vector2Int> FindPath(Vector2Int start)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        var mapData = map.GetMapData();
        var path = AStar.FindPath(mapData, (start.x, start.y));
        if (path != null)
        {
            foreach (var node in path)
            {
                var type = (TileType)mapData[node.x, node.y];
                if (type == TileType.GOAL || type == TileType.SPAWN)
                {
                    result.Add(new Vector2Int(node.x, node.y));
                    continue;
                }
                result.Add(new Vector2Int(node.x, node.y));
            }
        }
        return result;
    }
}
