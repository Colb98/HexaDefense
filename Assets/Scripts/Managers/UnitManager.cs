using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private Map map;

    [Header("Mobs spawn & Path")]
    public GameObject enemyPrefab;
    [SerializeField] private float waveInterval = 2f;
    [SerializeField] private float waveTimer = 0.0f;

    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float enemyInterval = 0.1f;
    [SerializeField] private float enemyTimer = 0.1f; // Time between enemy spawns
    [SerializeField] private bool spawningWaves = false;
    [SerializeField] private int enemyInWaveIndex = 0; // Index of the enemy in the current wave

    [Header("Object Pooling")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private bool expandPoolIfNeeded = true;

    [SerializeField] private List<Unit> activeEnemies = new List<Unit>();
    private Queue<Unit> pooledEnemies = new Queue<Unit>();
    private Vector2Int start = new Vector2Int(-1, -1);

    void Start()
    {
        waveTimer = waveInterval + 0.5f;
        enemyTimer = enemyInterval + 0.5f; 
        InitializePool();
    }

    private void InitializePool()
    {
        // Create initial pool of inactive enemies
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledUnit();
        }
    }

    private Unit CreatePooledUnit()
    {
        GameObject enemyObject = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
        enemyObject.transform.SetParent(transform); // Parent to UnitManager initially
        Unit unit = enemyObject.GetComponent<Unit>();
        unit.SetMap(map);
        unit.isDefense = false;

        // Deactivate the unit
        enemyObject.SetActive(false);

        // Add to pool
        pooledEnemies.Enqueue(unit);
        return unit;
    }

    private Unit GetUnitFromPool()
    {
        if (pooledEnemies.Count == 0)
        {
            if (expandPoolIfNeeded)
            {
                Debug.Log("Expanding unit pool");
                return CreatePooledUnit();
            }
            else
            {
                Debug.LogWarning("Unit pool is empty and expandPoolIfNeeded is disabled");
                return null;
            }
        }
        var unit = pooledEnemies.Dequeue();
        unit.Reset();

        return unit;
    }

    public void ReturnUnitToPool(Unit unit)
    {
        // Stop any ongoing behaviors
        unit.StopMoving();

        // Remove from active list
        activeEnemies.Remove(unit);

        // Reset the unit state as needed
        GameObject enemyObject = unit.gameObject;
        enemyObject.transform.SetParent(transform); // Reparent to UnitManager for storage
        enemyObject.SetActive(false);

        // Add back to pool
        pooledEnemies.Enqueue(unit);
    }

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

        if (spawningWaves)
        {
           enemyTimer += Time.deltaTime;
           HandleWaveSpawning();
        }
        else
        {
            waveTimer += Time.deltaTime;
            if (waveTimer >= waveInterval)
            {
                SpawnWave();
            }
        }
    }

    private void SpawnWave()
    {
        spawningWaves = true;
        waveTimer = 0.0f;

    }
    private void HandleWaveSpawning()
    {
        if (enemyInWaveIndex >= enemiesPerWave)
        {
            enemyInWaveIndex = 0;
            spawningWaves = false;
            return;
        }
        
        enemyTimer += Time.deltaTime;
        if (enemyTimer >= enemyInterval)
        {
            enemyTimer = 0.0f;
            SpawnEnemy("GraveWalker", 1);
            enemyInWaveIndex++;
        }
    }

    private void SpawnEnemy(string enemyType, int lvl)
    {
        // Get a unit from the pool
        Unit unit = GetUnitFromPool();

        if (unit != null)
        {
            GameObject enemyObject = unit.gameObject;

            var enemyConfig = GameConfigManager.Instance.Enemies[enemyType];
            var levelData = enemyConfig.levels.FirstOrDefault(level => level.level == lvl);

            unit.Initialize(enemyType, levelData);

            // Configure the unit
            unit.SetCoord(start);

            // Move under map for hierarchy organization
            enemyObject.transform.SetParent(map.transform);

            // Activate the unit
            enemyObject.SetActive(true);

            // Set up path and movement
            var path = FindPath(unit.position);
            unit.MoveByPath(path);

            // Add to active units list
            activeEnemies.Add(unit);
        }
    }

    public Unit[] GetEntities()
    {
        return activeEnemies.ToArray();
    }

    // This method is now used to return unit to pool instead of destroying it
    public void RemoveUnit(Unit unit)
    {
        ReturnUnitToPool(unit);
    }

    public void UpdateUnitPaths()
    {
        Debug.Log("Update unit paths");
        var entities = map.GetAllEntities();
        Entity[] attackableEntities = entities.Where(e => e.isDefense).ToArray();
        foreach (var unit in activeEnemies)
        {
            FindTargetAndPath(unit, attackableEntities);
        }
    }

    public void FindTargetAndPath(Unit unit)
    {
        var entities = map.GetAllEntities();
        Entity[] attackableEntities = entities.Where(e => e.isDefense).ToArray();
        FindTargetAndPath(unit, attackableEntities);
    }

    public void FindTargetAndPath(Unit unit, Entity[] attackableEntities)
    {
        unit.StopMoving();
        Debug.Log($"Stop movement for unit: {unit.name}, unit target == null {unit.target == null}");
        // If unit cannot attack any tower or defense entity, find the path to target
        if (unit.target == null)
        {
            // TODO: use strategy base on unit type (e.g., prioritize towers)
            Entity target = null;
            float min = float.MaxValue;
            float attackRangeSqr = unit.attackRange * unit.attackRange;
            foreach (var entity in attackableEntities)
            {
                var distSqr = unit.GetDistanceSqr(entity.position);
                var onValidTileType = unit.CanFly() ||
                                      map.GetMapDataAt(entity.position.x, entity.position.y) != TileType.WALL;
                if (entity.CanBeAttacked(unit.aggroLevel) && distSqr < min && onValidTileType)
                {
                    min = distSqr;
                    target = entity;
                }
            }
            unit.SetTarget(target);

            if (unit.target == null)
            {
                var path = FindPath(unit.position);
                unit.MoveByPath(path);
            }
            else
            {
                var tiles = Tile.GetHexesInRange(unit.target.position, (int)unit.attackRange + unit.target.GetSize() - 1);
                var path = AStar.FindPath(map.GetMapData(), tiles.ToArray(), unit.position);
                path.Reverse();
                unit.MoveByPath(path);
            }
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
                result.Add(new Vector2Int(node.x, node.y));
            }
        }

        return result;
    }

    // Optional method to clear all units when needed (like level reset)
    public void ClearAllUnits()
    {
        // Make a copy since we'll be modifying the collection while iterating
        Unit[] units = activeEnemies.ToArray();
        foreach (var unit in units)
        {
            ReturnUnitToPool(unit);
        }
    }

    // Optional method to preload specific number of units
    public void PreloadUnits(int count)
    {
        int amountToCreate = count - pooledEnemies.Count;
        for (int i = 0; i < amountToCreate; i++)
        {
            if (i > 0 && i % 10 == 0)
            {
                Debug.Log($"Created {i} additional units for pool");
            }
            CreatePooledUnit();
        }
    }
}