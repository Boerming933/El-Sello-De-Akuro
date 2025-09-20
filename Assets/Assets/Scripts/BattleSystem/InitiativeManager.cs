using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager de iniciativa que además soporta spawns programados por ronda
/// usando posiciones de grilla (Vector2Int). Es robusto ante tiles nulos,
/// busca casilla libre mediante BFS si la preferida está ocupada, y registra
/// la unidad con BattleSystem tras instanciar/activar.
/// </summary>
public class InitiativeManager : MonoBehaviour
{
    [Tooltip("Unidades aliadas y enemigas que participan en la ronda")]
    public List<Unit> allies;
    public List<Unit> enemies;

    [System.Serializable]
    public class InspectorSpawnRequest
    {
        [Tooltip("Prefab a instanciar (si se usa existingObject, ese tiene prioridad)")]
        public GameObject prefabToInstantiate;

        [Tooltip("Objeto ya en la escena (SetActive(false) si es pooling). Si lo asignás, se activará.")]
        public GameObject existingObject;

        [Tooltip("Si marcás esto usará gridPosition (Vector2Int).")]
        public bool useGridPosition = true;

        [Tooltip("Coordenadas de la casilla donde aparecerá (usar si useGridPosition = true).")]
        public Vector2Int gridPosition;

        [Tooltip("Ronda en la que debe aparecer (ej: 3 -> aparece al iniciar la ronda 3).")]
        public int spawnRound = 1;
    }

    [Tooltip("Lista de spawns definidos en el Inspector. Se encolan automáticamente en Awake().")]
    public List<InspectorSpawnRequest> inspectorScheduledSpawns = new List<InspectorSpawnRequest>();

    // Internal pending spawn representation
    private class SpawnRequest
    {
        public GameObject prefabToInstantiate;
        public GameObject existingObject;
        public Vector2Int? gridPosition;
        public int spawnRound;
    }
    private List<SpawnRequest> pendingSpawns = new List<SpawnRequest>();

    // Buffs temporales de iniciativa
    private class InitiativeBuff { public Unit unit; public int bonus; public int remainingTurns; }
    private List<InitiativeBuff> activeBuffs = new List<InitiativeBuff>();

    // Turn order
    private List<InitiativeEntry> turnOrder = new List<InitiativeEntry>();
    private int currentIndex = 0;
    private int lastIndex = -1;
    private bool reorderPending = false;
    private int currentRound = 1;

    [Header("Spawn settings")]
    [Tooltip("Radio máximo (en tiles) para buscar casilla libre si la solicitada está ocupada")]
    public int spawnSearchRadius = 6;

    // NUEVO: flag para prevenir reentradas en el procesamiento de spawns
    private Dictionary<Unit,int> playCounts = new Dictionary<Unit,int>();
    

    private void Awake()
    {
        // Convertir entradas del inspector en pendingSpawns
        if (inspectorScheduledSpawns != null && inspectorScheduledSpawns.Count > 0)
        {
            foreach (var s in inspectorScheduledSpawns)
            {
                if (s.prefabToInstantiate == null && s.existingObject == null)
                {
                    Debug.LogWarning("[InitiativeManager] InspectorSpawnRequest inválida (sin prefab ni existingObject). Saltada.");
                    continue;
                }

                var req = new SpawnRequest();
                req.prefabToInstantiate = s.prefabToInstantiate;
                req.existingObject = s.existingObject;
                req.spawnRound = Mathf.Max(1, s.spawnRound);

                if (s.useGridPosition)
                    req.gridPosition = s.gridPosition;
                else
                    req.gridPosition = null; // no se usó grid position

                pendingSpawns.Add(req);

                Debug.Log($"[InitiativeManager] (Awake) Encolado spawn inspector: {(req.prefabToInstantiate != null ? req.prefabToInstantiate.name : req.existingObject.name)} en ronda {req.spawnRound}"
                          + (req.gridPosition.HasValue ? $" grid {req.gridPosition.Value}" : " (no grid)"));
            }
        }
    }

    // -------------------- Iniciativa básica --------------------
    public void RollInitiative()
    {
        turnOrder.Clear();
        reorderPending = false;
        currentRound = 1;
        currentIndex = 0;
        lastIndex = -1;

        var participants = allies.Concat(enemies);
        foreach (var u in participants)
        {
            var entry = new InitiativeEntry { unit = u, dexterity = u.Des, roll = UnityEngine.Random.Range(1, 21)};
            entry.total = entry.dexterity + entry.roll + GetBuffSum(u);
            turnOrder.Add(entry);
        }

        SortTurnOrder();
    }

    public Unit GetNextUnit()
{
    if (turnOrder.Count == 0)
    {
        Debug.LogWarning("InitiativeManager: no hay turnOrder. Llama a RollInitiative primero.");
        return null;
    }

    Debug.Log($"[InitiativeManager] GetNextUnit called. currentIndex={currentIndex}, lastIndex={lastIndex}, turnOrderCount={turnOrder.Count}, currentRound={currentRound}");

    // Detectar inicio de nueva ronda completa (la ronda entera terminó)
    bool isNewRound = (currentIndex == 0 && lastIndex == turnOrder.Count - 1);
    if (isNewRound)
    {
        Debug.Log("[InitiativeManager] --- Nueva ronda detectada (transición) ---");

        // Guardar referencia a la última unidad jugada (si existe)
        Unit lastUnitBefore = null;
        if (lastIndex >= 0 && lastIndex < turnOrder.Count)
            lastUnitBefore = turnOrder[lastIndex].unit;

        // Calcular total de la última unidad jugada (para comparar con los spawns)
        int lastPlayedTotal = int.MinValue;
        if (lastUnitBefore != null)
        {
            var prevEntry = turnOrder.Find(e => e.unit == lastUnitBefore);
            if (prevEntry != null)
                lastPlayedTotal = prevEntry.total;
        }

        // Obtener el contador actual de plays de la última unidad (0 si no existe)
        int lastUnitPlayCount = 0;
        if (lastUnitBefore != null && playCounts.ContainsKey(lastUnitBefore))
            lastUnitPlayCount = playCounts[lastUnitBefore];

        // Procesar spawns destinados a la ronda siguiente (targetRound = currentRound + 1)
        int targetRound = currentRound + 1;
        ProcessPendingSpawnsForRound(targetRound, lastPlayedTotal, lastUnitBefore, lastUnitPlayCount);

        // 2) Ahora incrementamos la ronda (se considera que la "nueva ronda" empieza)
        currentRound++;
        // 3) Actualizar buffs (ahora que estamos en la nueva ronda)
        UpdateBuffs();

        // 4) Reordenar / mantener flujo como antes
        if (reorderPending)
        {
            RecalculateAndKeepCurrent();
            reorderPending = false;
            Debug.Log("[InitiativeManager] Nuevo orden por buff: " + FormatTurnOrder());
        }
        else
        {
            if (turnOrder.Count > 0 && lastUnitBefore != null)
            {
                SortTurnOrder();
                int idx = turnOrder.FindIndex(e => e.unit == lastUnitBefore);
                if (idx >= 0)
                    currentIndex = (idx + 1) % turnOrder.Count;
                else
                    currentIndex = 0;
            }
            else
            {
                SortTurnOrder();
                currentIndex = 0;
            }
        }

        Debug.Log($"[InitiativeManager] Después de ProcessPendingSpawnsForRound: pendingSpawns={pendingSpawns.Count}");
    }

    // ahora buscamos la siguiente entrada elegible (saltamos las entradas cuyo spawnEligibleRound > currentRound
    // o cuyo bloqueo por unidad no se cumplió aún)
    if (turnOrder.Count == 0)
        return null;

    int attempts = 0;
    while (attempts < turnOrder.Count)
    {
        var cand = turnOrder[currentIndex];

        // 1) comprobar ronda mínima
        bool roundOk = (cand.spawnEligibleRound == 0 || cand.spawnEligibleRound <= currentRound);

        // 2) comprobar bloqueo por unidad (si existe)
        bool blockingOk = true;
        if (cand.blockingUnit != null)
        {
            int count = 0;
            if (playCounts.ContainsKey(cand.blockingUnit)) count = playCounts[cand.blockingUnit];
            blockingOk = (count > cand.blockingUnitPlayCountAtSpawn);
        }

        if (roundOk && blockingOk)
            break;

        currentIndex = (currentIndex + 1) % turnOrder.Count;
        attempts++;
    }

    var entry = turnOrder[currentIndex];
    Debug.Log($"[InitiativeManager] Turno {currentIndex + 1}: {entry.unit.name} (Total: {entry.total}, eligibleRound:{entry.spawnEligibleRound}, blockingUnit:{(entry.blockingUnit!=null?entry.blockingUnit.name:"none")})");

    // Actualizamos trackers y avanzamos índice para la próxima llamada
    lastIndex = currentIndex;
    currentIndex = (currentIndex + 1) % turnOrder.Count;

    // --- Registro: incrementamos playCounts para la unidad que acabamos de entregar ---
    if (!playCounts.ContainsKey(entry.unit)) playCounts[entry.unit] = 0;
    playCounts[entry.unit]++;

    return entry.unit;
}



    // -------------------- Scheduling API (sobrecargas) --------------------
    // Desde código: pasar OverlayTile (compatibilidad)
    public void ScheduleSpawnPrefab(GameObject prefab, OverlayTile tile, int round)
    {
        Vector2Int? pos = tile != null ? (Vector2Int?)tile.grid2DLocation : null;
        ScheduleSpawnPrefab(prefab, pos, round);
    }

    // Desde código: pasar grid position directamente
    public void ScheduleSpawnPrefab(GameObject prefab, Vector2Int? gridPos, int round)
    {
        pendingSpawns.Add(new SpawnRequest { prefabToInstantiate = prefab, gridPosition = gridPos, spawnRound = round });
        Debug.Log($"[InitiativeManager] ScheduleSpawnPrefab encolado: {prefab?.name} ronda {round} grid {(gridPos.HasValue ? gridPos.Value.ToString() : "null")}");
    }

    public void ScheduleActivateExisting(GameObject existingObject, OverlayTile tile, int round)
    {
        Vector2Int? pos = tile != null ? (Vector2Int?)tile.grid2DLocation : null;
        ScheduleActivateExisting(existingObject, pos, round);
    }

    public void ScheduleActivateExisting(GameObject existingObject, Vector2Int? gridPos, int round)
    {
        pendingSpawns.Add(new SpawnRequest { existingObject = existingObject, gridPosition = gridPos, spawnRound = round });
        Debug.Log($"[InitiativeManager] ScheduleActivateExisting encolado: {existingObject?.name} ronda {round} grid {(gridPos.HasValue ? gridPos.Value.ToString() : "null")}");
    }

    // -------------------- Procesamiento de spawns --------------------
private void ProcessPendingSpawnsForRound(int targetRound, int lastPlayedTotal, Unit lastUnitBefore, int lastUnitPlayCount)
{
    if (pendingSpawns == null || pendingSpawns.Count == 0) return;

    var toProcess = pendingSpawns.Where(s => s.spawnRound == targetRound).ToList();
    if (toProcess.Count == 0) return;

    Debug.Log($"[InitiativeManager] Procesando {toProcess.Count} spawn(s) para la ronda objetivo {targetRound}");

    foreach (var req in toProcess)
    {
        // Resolver tile objetivo a partir de gridPosition (si existe)
        OverlayTile spawnTile = null;
        if (req.gridPosition.HasValue && MapManager.Instance != null && MapManager.Instance.map != null)
        {
            MapManager.Instance.map.TryGetValue(req.gridPosition.Value, out spawnTile);
        }

        // fallback: si tile nulo, tomar la primera libre del mapa
        if (spawnTile == null && MapManager.Instance != null && MapManager.Instance.map != null)
        {
            spawnTile = MapManager.Instance.map.Values.FirstOrDefault(t => t != null && !t.isBlocked);
        }

        // Resolver spawnTile final buscando el nearest free tile (BFS)
        var resolved = FindNearestFreeTile(spawnTile, spawnSearchRadius);
        if (resolved == null)
        {
            Debug.LogWarning($"[InitiativeManager] No hay casilla libre para spawn en ronda {targetRound}; se ignora este spawn.");
            continue;
        }
        spawnTile = resolved;

        GameObject spawnGO = null;
        Unit spawnedUnit = null;

        // Si es existing object: activar y posicionar
        if (req.existingObject != null)
        {
            spawnGO = req.existingObject;
            spawnGO.SetActive(true);
            spawnedUnit = spawnGO.GetComponent<Unit>();
            if (spawnTile != null)
            {
                spawnGO.transform.position = spawnTile.transform.position;
                Physics2D.SyncTransforms();
            }
        }
        // Si es prefab: instanciar en tile
        else if (req.prefabToInstantiate != null)
        {
            if (spawnTile != null)
            {
                Vector3 pos = spawnTile.transform.position;
                spawnGO = GameObject.Instantiate(req.prefabToInstantiate, pos, Quaternion.identity);
                spawnedUnit = spawnGO.GetComponent<Unit>();
            }
            else
            {
                Debug.LogWarning("[InitiativeManager] Spawn pedido sin tile resoluble; se saltó.");
                continue;
            }
        }

        if (spawnedUnit == null || spawnGO == null)
        {
            Debug.LogWarning("[InitiativeManager] Spawn creó un objeto sin Unit o spawnGO null. Ignorado.");
            continue;
        }

        // 1) Añadir la unidad al battle (la registramos en BattleSystem para que se actualicen PositionEnemy/EnemyUnity/HUD)
        if (BattleSystem.Instance != null)
        {
            BattleSystem.Instance.AddEnemyToBattle(spawnGO);
            Debug.Log($"[InitiativeManager] AddEnemyToBattle llamado para {spawnedUnit.name}");
        }
        else
        {
            Debug.LogWarning("[InitiativeManager] BattleSystem.Instance es null al agregar spawn; asegúrate de iniciar BattleSystem primero.");
        }

        // 2) Crear entrada de iniciativa para la nueva unidad y añadir al turnOrder
        var entry = new InitiativeEntry
        {
            unit = spawnedUnit,
            dexterity = spawnedUnit.Des,
            roll = UnityEngine.Random.Range(1, 21)
        };
        // decidir la primera ronda donde es elegible:
        entry.total = entry.dexterity + entry.roll + GetBuffSum(spawnedUnit);

        // Por defecto lo hacemos elegible para la ronda objetivo (targetRound)
        // y solo aplicamos bloqueo si su total es menor o igual al total de la última unidad jugada,
        // lo que indica que en el orden quedaría *después* de esa unidad.
        entry.spawnEligibleRound = targetRound;

        if (lastPlayedTotal == int.MinValue || lastUnitBefore == null)
        {
            // No hay "última unidad" definida (caso raro): dejarlo elegible sin bloqueo.
            entry.blockingUnit = null;
            entry.blockingUnitPlayCountAtSpawn = 0;
        }
        else
        {
            if (entry.total <= lastPlayedTotal)
            {
                // Si su total es menor o igual, bloqueamos hasta que la última unidad juegue otra vez.
                entry.blockingUnit = lastUnitBefore;
                entry.blockingUnitPlayCountAtSpawn = lastUnitPlayCount;
            }
            else
            {
                // Si su total es mayor, no bloquear: puede jugar normalmente en la targetRound.
                entry.blockingUnit = null;
                entry.blockingUnitPlayCountAtSpawn = 0;
            }
        }

        turnOrder.Add(entry);

        Debug.Log($"[InitiativeManager] Spawn activo: {spawnedUnit.name} agregado al turnOrder con total {entry.total} " +
                $"(eligibleRound={entry.spawnEligibleRound}, blockingUnit={(entry.blockingUnit!=null?entry.blockingUnit.name:"none")})");
    }

    // Quitar procesados
    pendingSpawns.RemoveAll(s => s.spawnRound == targetRound);

    // Reordenar y mantener flujo
    if (turnOrder.Count == 0) return;

    SortTurnOrder();
    Debug.Log("[InitiativeManager] Turn order actualizado tras spawns: " + FormatTurnOrder());
}



    // -------------------- BFS nearest free tile --------------------
    // Usamos Vector3Int para visited porque OverlayTile.gridLocation es Vector3Int
    private OverlayTile FindNearestFreeTile(OverlayTile preferred, int maxRadius = 6)
    {
        if (MapManager.Instance == null || MapManager.Instance.map == null) return null;

        // si preferred es null, tomar la primera libre del mapa
        if (preferred == null)
            return MapManager.Instance.map.Values.FirstOrDefault(t => t != null && !t.isBlocked);

        // Si la preferred ya está libre y no ocupada por chequeos, devolverla
        if (!preferred.isBlocked && !InternalIsTileOccupied(preferred))
            return preferred;

        var visited = new HashSet<Vector3Int>();
        var queue = new Queue<(OverlayTile tile, int dist)>();
        queue.Enqueue((preferred, 0));
        visited.Add(preferred.gridLocation);

        while (queue.Count > 0)
        {
            var (tile, dist) = queue.Dequeue();

            if (dist > 0)
            {
                if (tile != null && !tile.isBlocked && !InternalIsTileOccupied(tile))
                    return tile;
            }

            if (dist >= maxRadius) continue;

            var neighbours = MapManager.Instance.GetNeighbourTiles(tile, MapManager.Instance.map.Values.ToList());
            if (neighbours == null) continue;

            foreach (var n in neighbours)
            {
                if (n == null) continue;
                if (visited.Contains(n.gridLocation)) continue;
                visited.Add(n.gridLocation);
                queue.Enqueue((n, dist + 1));
            }
        }

        return null;
    }

    // Comprobación local de ocupación (no depende de un método extra en BattleSystem)
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

    // -------------------- Buffs / Helpers --------------------
    private void UpdateBuffs()
    {
        foreach (var b in activeBuffs) b.remainingTurns--;
        int removed = activeBuffs.RemoveAll(b => b.remainingTurns <= 0);
        if (removed > 0) Debug.Log($"[InitiativeBuff] Expiraron {removed} buff(s). Quedan: {activeBuffs.Count}");
    }

    public void ApplyInitiativeBuff(Unit u, int bonus, int durationTurns)
    {
        var buff = new InitiativeBuff { unit = u, bonus = bonus, remainingTurns = durationTurns };
        activeBuffs.Add(buff);
        reorderPending = true;
        Debug.Log($"[InitiativeBuff] Aplicado a {u.name}: +{bonus} ini por {durationTurns} turnos.");
    }

    private int GetBuffSum(Unit u) => activeBuffs.Where(b => b.unit == u).Sum(b => b.bonus);

    private void RecalculateAndKeepCurrent()
    {
        if (turnOrder.Count == 0) return;
        int lastIdx = (lastIndex + turnOrder.Count) % turnOrder.Count;
        Unit lastUnit = turnOrder[lastIdx].unit;
        foreach (var e in turnOrder) e.total = e.dexterity + e.roll + GetBuffSum(e.unit);
        SortTurnOrder();
        int idx = turnOrder.FindIndex(e => e.unit == lastUnit);
        currentIndex = (idx + 1) % turnOrder.Count;
    }

    private string FormatTurnOrder() => string.Join(" | ", turnOrder.Select(e => $"{e.unit.name}(tot:{e.total})"));

    private void SortTurnOrder()
    {
        turnOrder = turnOrder.OrderByDescending(e => e.total)
                             .ThenByDescending(e => e.dexterity)
                             .ThenByDescending(e => e.roll)
                             .ToList();
    }

    public void PrintTurnOrder()
    {
        Debug.Log("=== Turn Order ===");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            var e = turnOrder[i];
            Debug.Log($"{i + 1}. {e.unit.name} (Roll: {e.roll}, Dex: {e.dexterity}, Total: {e.total})");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) PrintTurnOrder();
    }
}
