using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private Map map;
    private int unitCount = 0;

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
        unit.name = $"Enemy_{unitCount++}";

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

    public void OnUnitDead(Unit unit)
    {
        if (unit.target != null)
        {
            FindUnitToAttack(unit.target); // Find another unit to attack this
        }
    }

    void Update()
    {
        if (PausableUpdateManager.instance.IsPaused())
        {
            return;
        }

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
            FindTargetAndPath(unit);

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
        //foreach (var unit in activeEnemies)
        //{
        //    FindTargetAndPath(unit, attackableEntities);
        //}
        foreach (var entity in attackableEntities)
        {
            FindUnitToAttack(entity);
        }
    }

    public void FindUnitToAttack(Entity attackableEntity)
    {
        var distanceToEnd = map.GetDistanceToEnd(attackableEntity.position);
        if (distanceToEnd < 0)
        {
            return;
        }
        List<Unit> units = activeEnemies.Where(u => {
            var uToEnd = map.GetDistanceToEnd(u.position);
            return u.target == null && attackableEntity.CanBeAttacked(u.aggroLevel) && distanceToEnd - uToEnd < 10;
        }).ToList();
        units.Sort((u1, u2) =>
        {
            return Tile.GetHexManhattanDistance(u1.position, attackableEntity.position).CompareTo(Tile.GetHexManhattanDistance(u2.position, attackableEntity.position));
        });
        // Debug.Log($"Units found to attack {attackableEntity.name}: {string.Join(", ", units.Select(u => u.name))}");

        while (units.Count > 0 && attackableEntity.CanBeAttacked(units[0].aggroLevel))
        {
            Unit unit = units[0];

            unit.SetTarget(attackableEntity);

            var tiles = Tile.GetHexesInRange(unit.target.position, (int)unit.attackRange + unit.target.GetSize() - 1);

            // If unit is moving, use next position, otherwise use current position
            var isMoving = unit.IsMoving();
            var unitPosition = isMoving ? unit.GetNextPosition() : unit.position;
            var path = AStar.FindPath(map.GetMapData(), tiles.ToArray(), unitPosition);
            path.Reverse();
            if (isMoving)
            {
                unit.MoveByPathAfterFinish(path);
            }
            else
            {
                unit.MoveByPath(path);
            }
            units.RemoveAt(0);
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
        var unitDistToEnd = map.GetDistanceToEnd(unit.position.x, unit.position.y);
        // If unit cannot attack any tower or defense entity, find the path to target
        if (unit.target == null)
        {
            // TODO: use strategy base on unit type (e.g., prioritize towers)
            Entity target = null;
            float min = float.MaxValue;
            float attackRangeSqr = unit.attackRange * unit.attackRange;
            foreach (var entity in attackableEntities)
            {
                if (entity.CanBeAttacked(unit.aggroLevel))
                {
                    var entityDistToEnd = map.GetDistanceToEnd(entity.position.x, entity.position.y);
                    var distSqr = unit.GetDistanceSqr(entity.position);
                    var onValidTileType = unit.CanFly() ||
                                          map.GetMapDataAt(entity.position.x, entity.position.y) != TileType.WALL;
                    if (distSqr < min && entityDistToEnd - unitDistToEnd < 10 && onValidTileType)
                    {
                        min = distSqr;
                        target = entity;
                    }
                }
            }

            // Nếu có target thì phải tìm thử đường (nếu không có đường thì không setTarget)
            if (target != null)
            {
                var tiles = Tile.GetHexesInRange(target.position, (int)unit.attackRange + target.GetSize() - 1);
                var isMoving = unit.IsMoving();
                var unitPosition = isMoving ? unit.GetNextPosition() : unit.position;
                var path = AStar.FindPath(map.GetMapData(), tiles.ToArray(), unitPosition);
                if (path != null && path.Count > 0)
                {
                    path.Reverse();
                    if (isMoving)
                    {
                        unit.MoveByPathAfterFinish(path);
                    }
                    else
                    {
                        unit.MoveByPath(path);
                    }
                    unit.SetTarget(target);
                }
            }

            // Nếu không có target thì tìm đường tới ô mục tiêu như bình thường
            if (unit.target == null)
            {
                FindPathAndMoveUnitToGoal(unit);
            }
        }
    }

    private void FindPathAndMoveUnitToGoal(Unit unit)
    {
        if (unit.IsMoving())
        {
            var path = FindPath(unit.GetNextPosition());
            Debug.Log($"Finding path for unit {unit.name} to goal. From position {unit.GetNextPosition()}. Path length {path.Count}");
            unit.MoveByPathAfterFinish(path);
        }
        else
        {
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