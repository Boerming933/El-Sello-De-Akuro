using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitiativeManager : MonoBehaviour
{
    [Tooltip("Unidades aliadas y enemigas que participan en la ronda")]
    public List<Unit> allies;
    public List<Unit> enemies;
    public TurnOrderDisplay turnOrderDisplay;

    [System.Serializable]
    public class InspectorSpawnRequest
    {
        public GameObject prefabToInstantiate;
        public GameObject existingObject;
        public bool useGridPosition = true;
        public Vector2Int gridPosition;
        public int spawnRound = 1;
    }

    public List<InspectorSpawnRequest> inspectorScheduledSpawns = new List<InspectorSpawnRequest>();

    private class SpawnRequest
    {
        public GameObject prefabToInstantiate;
        public GameObject existingObject;
        public Vector2Int? gridPosition;
        public int spawnRound;
    }

    private List<SpawnRequest> pendingSpawns = new List<SpawnRequest>();

    private class InitiativeBuff { public Unit unit; public int bonus; public int remainingTurns; }
    private List<InitiativeBuff> activeBuffs = new List<InitiativeBuff>();

    private List<InitiativeEntry> turnOrder = new List<InitiativeEntry>();
    public int currentIndex = 0;
    public int lastIndex = -1;
    public bool reorderPending = false;
    public int currentRound = 1;

    public int spawnSearchRadius = 6;
    public Dictionary<Unit, int> playCounts = new Dictionary<Unit, int>();

    //public List<InitiativeEntry> TurnOrder => turnOrder;
    //public int CurrentIndex => currentIndex;

    private void Awake()
    {
        foreach (var s in inspectorScheduledSpawns)
        {
            if (s.prefabToInstantiate == null && s.existingObject == null)
                continue;

            var req = new SpawnRequest
            {
                prefabToInstantiate = s.prefabToInstantiate,
                existingObject = s.existingObject,
                spawnRound = Mathf.Max(1, s.spawnRound),
                gridPosition = s.useGridPosition ? s.gridPosition : (Vector2Int?)null
            };

            pendingSpawns.Add(req);
        }
    }

    public void RollInitiative()
    {
        turnOrderDisplay.UpdateTurnOrder();
        turnOrder.Clear();
        reorderPending = false;
        currentRound = 1;
        currentIndex = 0;
        lastIndex = -1;

        var participants = allies.Concat(enemies);
        foreach (var u in participants)
        {
            var entry = new InitiativeEntry
            {
                unit = u,
                dexterity = u.Des,
                roll = UnityEngine.Random.Range(1, 21)
            };
            entry.total = entry.dexterity + entry.roll + GetBuffSum(u);
            turnOrder.Add(entry);
        }

        SortTurnOrder();
    }

    public Unit GetNextUnit()
    {
        turnOrderDisplay.UpdateTurnOrder();
        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("InitiativeManager: no hay turnOrder. Llama a RollInitiative primero.");
            return null;
        }

        bool isNewRound = (currentIndex == 0 && lastIndex == turnOrder.Count - 1);
        if (isNewRound)
        {
            Unit lastUnitBefore = lastIndex >= 0 && lastIndex < turnOrder.Count ? turnOrder[lastIndex].unit : null;
            int lastPlayedTotal = lastUnitBefore != null ? turnOrder.Find(e => e.unit == lastUnitBefore)?.total ?? int.MinValue : int.MinValue;
            int lastUnitPlayCount = lastUnitBefore != null && playCounts.ContainsKey(lastUnitBefore) ? playCounts[lastUnitBefore] : 0;

            ProcessPendingSpawnsForRound(currentRound + 1, lastPlayedTotal, lastUnitBefore, lastUnitPlayCount);

            currentRound++;
            UpdateBuffs();

            if (reorderPending)
            {
                RecalculateAndKeepCurrent();
                reorderPending = false;
            }
            else
            {
                if (turnOrder.Count > 0 && lastUnitBefore != null)
                {
                    SortTurnOrder();
                    int idx = turnOrder.FindIndex(e => e.unit == lastUnitBefore);
                    currentIndex = idx >= 0 ? (idx + 1) % turnOrder.Count : 0;
                }
                else
                {
                    SortTurnOrder();
                    currentIndex = 0;
                }
            }
        }

        if (turnOrder.Count == 0) return null;

        int attempts = 0;
        while (attempts < turnOrder.Count)
        {
            var cand = turnOrder[currentIndex];
            bool roundOk = cand.spawnEligibleRound == 0 || cand.spawnEligibleRound <= currentRound;
            bool blockingOk = true;

            if (cand.blockingUnit != null)
            {
                int count = playCounts.ContainsKey(cand.blockingUnit) ? playCounts[cand.blockingUnit] : 0;
                blockingOk = count > cand.blockingUnitPlayCountAtSpawn;
            }

            if (roundOk && blockingOk)
                break;

            currentIndex = (currentIndex + 1) % turnOrder.Count;
            attempts++;
        }

        var entry = turnOrder[currentIndex];
        lastIndex = currentIndex;
        currentIndex = (currentIndex + 1) % turnOrder.Count;

        if (!playCounts.ContainsKey(entry.unit))
            playCounts[entry.unit] = 0;

        playCounts[entry.unit]++;

        return entry.unit;
    }

    private void ProcessPendingSpawnsForRound(int targetRound, int lastPlayedTotal, Unit lastUnitBefore, int lastUnitPlayCount)
    {
        var toProcess = pendingSpawns.Where(s => s.spawnRound == targetRound).ToList();
        foreach (var req in toProcess)
        {
            OverlayTile spawnTile = null;
            if (req.gridPosition.HasValue && MapManager.Instance.map != null)
                MapManager.Instance.map.TryGetValue(req.gridPosition.Value, out spawnTile);

            spawnTile ??= MapManager.Instance.map.Values.FirstOrDefault(t => t != null && !t.isBlocked);
            var resolved = FindNearestFreeTile(spawnTile, spawnSearchRadius);
            if (resolved == null) continue;
            spawnTile = resolved;

            GameObject spawnGO = null;
            Unit spawnedUnit = null;

            if (req.existingObject != null)
            {
                spawnGO = req.existingObject;
                spawnGO.SetActive(true);
                spawnedUnit = spawnGO.GetComponent<Unit>();
                spawnGO.transform.position = spawnTile.transform.position;
                Physics2D.SyncTransforms();
            }
            else if (req.prefabToInstantiate != null)
            {
                spawnGO = Instantiate(req.prefabToInstantiate, spawnTile.transform.position, Quaternion.identity);
                spawnedUnit = spawnGO.GetComponent<Unit>();
            }

            if (spawnedUnit == null || spawnGO == null) continue;

            BattleSystem.Instance?.AddEnemyToBattle(spawnGO);

            var entry = new InitiativeEntry
            {
                unit = spawnedUnit,
                dexterity = spawnedUnit.Des,
                roll = UnityEngine.Random.Range(1, 21)
            };
            entry.total = entry.dexterity + entry.roll + GetBuffSum(spawnedUnit);
            entry.spawnEligibleRound = targetRound;

            if (lastPlayedTotal != int.MinValue && lastUnitBefore != null)
            {
                if (entry.total <= lastPlayedTotal)
                {
                    entry.blockingUnit = lastUnitBefore;
                    entry.blockingUnitPlayCountAtSpawn = lastUnitPlayCount;
                }
            }

            turnOrder.Add(entry);
        }

        pendingSpawns.RemoveAll(s => s.spawnRound == targetRound);
        SortTurnOrder();
    }

    private OverlayTile FindNearestFreeTile(OverlayTile preferred, int maxRadius = 6)
    {
        if (MapManager.Instance.map == null) return null;

        if (preferred == null)
            return MapManager.Instance.map.Values.FirstOrDefault(t => t != null && !t.isBlocked);

        if (!preferred.isBlocked && !InternalIsTileOccupied(preferred))
            return preferred;

        var visited = new HashSet<Vector3Int>();
        var queue = new Queue<(OverlayTile tile, int dist)>();
        queue.Enqueue((preferred, 0));
        visited.Add(preferred.gridLocation);

        while (queue.Count > 0)
        {
            var (tile, dist) = queue.Dequeue();

            if (dist > 0 && tile != null && !tile.isBlocked && !InternalIsTileOccupied(tile))
                return tile;

            if (dist >= maxRadius) continue;

            var neighbours = MapManager.Instance.GetNeighbourTiles(tile, MapManager.Instance.map.Values.ToList());
            foreach (var n in neighbours)
            {
                if (n == null || visited.Contains(n.gridLocation)) continue;
                visited.Add(n.gridLocation);
                queue.Enqueue((n, dist + 1));
            }
        }

        return null;
    }

    private bool InternalIsTileOccupied(OverlayTile tile)
    {
        if (tile == null) return false;
        if (tile.isBlocked) return true;

        var bs = BattleSystem.Instance;
        if (bs != null)
        {
            if (bs.PositionEnemy != null && bs.PositionEnemy.Contains(tile)) return true;
            if (bs.PositionPlayer != null && bs.PositionPlayer.Contains(tile)) return true;
        }

        return false;
    }

    private void UpdateBuffs()
    {
        foreach (var b in activeBuffs) b.remainingTurns--;
        activeBuffs.RemoveAll(b => b.remainingTurns <= 0);
    }

    public void ApplyInitiativeBuff(Unit u, int bonus, int durationTurns)
    {
        activeBuffs.Add(new InitiativeBuff { unit = u, bonus = bonus, remainingTurns = durationTurns });
        reorderPending = true;
    }

    private int GetBuffSum(Unit u) => activeBuffs.Where(b => b.unit == u).Sum(b => b.bonus);

    private void RecalculateAndKeepCurrent()
    {
        if (turnOrder.Count == 0) return;

        int lastIdx = (lastIndex + turnOrder.Count) % turnOrder.Count;
        Unit lastUnit = turnOrder[lastIdx].unit;

        foreach (var e in turnOrder)
            e.total = e.dexterity + e.roll + GetBuffSum(e.unit);

        SortTurnOrder();

        int idx = turnOrder.FindIndex(e => e.unit == lastUnit);
        currentIndex = (idx + 1) % turnOrder.Count;
    }

    private void SortTurnOrder()
    {
        turnOrder = turnOrder.OrderByDescending(e => e.total)
                             .ThenByDescending(e => e.dexterity)
                             .ThenByDescending(e => e.roll)
                             .ToList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            PrintTurnOrder();

    }

    public void PrintTurnOrder()
    {
        Debug.Log("=== Orden de Turnos ===");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            var e = turnOrder[i];

        }
    }

    public List<InitiativeEntry> TurnOrder => turnOrder;
    public int CurrentIndex => currentIndex;
}