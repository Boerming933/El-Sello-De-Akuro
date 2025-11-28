using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    public List<GameObject> PlayersPrefab;
    public List<GameObject> EnemiesPrefab;
    public BattleState state;

    public bool start = false;

    public List<BattleHUD> playerHUD;
    public List<BattleHUD> enemyHUD;

    public List<OverlayTile> PositionEnemy = new List<OverlayTile>();
    public List<OverlayTile> PositionPlayer = new List<OverlayTile>();

    public List<Unit> PlayerUnity = new List<Unit>();
    public List<Unit> EnemyUnity = new List<Unit>();

    public InitiativeManager initiativeManager;
    public MouseControler mouseController;
    public List<EnemyIA> EnemyIAs = new List<EnemyIA>();
    // Colección de todos los participantes
    private List<Unit> allUnits;

    public event System.Action<Unit> OnTurnStart;
    private Unit _currentUnit;
    public Unit CurrentUnit => _currentUnit;
    public BuffDebuffAttackController attackController;

    public CharacterDetailsUI detailsUI;

    public float finalPositionPlayers;

    private void Awake()
    {
        Instance = this;
        
        // ✅ Add AttackUsesRuntimeManager component if not present
        if (GetComponent<AttackUsesRuntimeManager>() == null)
        {
            gameObject.AddComponent<AttackUsesRuntimeManager>();
        }
    }

    IEnumerator Start()
    {
        // Espera hasta que MapManager.map ya exista
        yield return new WaitUntil(() => MapManager.Instance != null
                                    && MapManager.Instance.map != null);

        StartBattle();
    }

    void Update()
    {
        for (int i = 0; i < PlayersPrefab.Count; i++)
        {
            if (PlayersPrefab[i] == null)
            {
                PositionPlayer[i].isBlocked = false;
                PositionPlayer.RemoveAt(i);
                playerHUD.RemoveAt(i);
                PlayersPrefab.RemoveAt(i);
                PlayerUnity.RemoveAt(i);
            }
        }
        for (int i = 0; i < EnemiesPrefab.Count; i++)
        {
            if (EnemiesPrefab[i] == null)
            {
                EnemiesPrefab.RemoveAt(i);
                PositionEnemy[i].isBlocked = false;
                PositionEnemy.RemoveAt(i);
                EnemyUnity.RemoveAt(i);
                EnemyIAs.RemoveAt(i);
            }
        }


    }

    void StartBattle()
    {
        // 1) Reúne aliados y enemigos en un solo listado
        allUnits = new List<Unit>();
        if (initiativeManager != null)
        {
            allUnits = initiativeManager.allies
                .Concat(initiativeManager.enemies)
                .ToList();
        }

        // 2) Rola iniciativa UNA VEZ
        initiativeManager?.RollInitiative();

        // Registrar players prefabs
        PlayerUnity.Clear();
        for (int i = 0; i < PlayersPrefab.Count; i++)
        {
            Unit unit = PlayersPrefab[i].GetComponent<Unit>();
            RegisterUnits(unit);
            PlayerUnity.Add(unit);
        }

        // Instanciar/registrar enemigos visibles iniciales
        EnemyUnity.Clear();
        for (int i = 0; i < EnemiesPrefab.Count; i++)
        {
            Unit unit = EnemiesPrefab[i].GetComponent<Unit>();
            EnemyIA enemyIA = EnemiesPrefab[i].GetComponent<EnemyIA>();
            RegisterUnits(unit);
            EnemyIAs.Add(enemyIA);
            EnemyUnity.Add(unit);
        }

        // 3) Arranca el bucle de turnos
        StartCoroutine(RunTurns());

        start = true;

        int playerCount = Mathf.Min(PlayerUnity.Count, playerHUD.Count);
        int enemyCount = Mathf.Min(EnemyUnity.Count, enemyHUD.Count);
        // Asignar cada unidad a su HUD correspondiente
        for (int i = 0; i < playerCount; i++)
        {
            playerHUD[i].SetHUD(PlayerUnity[i]);
        }
        for (int i = 0; i < enemyCount; i++)
        {
            enemyHUD[i].SetHUD(EnemyUnity[i]);
        }
    }

    public void RegisterUnits(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("RegisterUnits: unit es null");
            return;
        }

        var maybePos = unit.ActiveTile();
        if (!maybePos.HasValue)
        {
            Debug.LogWarning($"RegisterUnits: no se encontró casilla para {unit.name}");
            // aun así añadimos la unidad a la lista correspondiente (se registrará cuando se mueva)
            if (unit.isEnemy)
            {
                //if (!EnemyUnity.Contains(unit)) EnemyUnity.Add(unit);
                // aseguramos lista PositionEnemy sincronizada
                while (PositionEnemy.Count < EnemyUnity.Count) PositionEnemy.Add(null);
            }
            else
            {
                //if (!PlayerUnity.Contains(unit)) PlayerUnity.Add(unit);
                while (PositionPlayer.Count < PlayerUnity.Count) PositionPlayer.Add(null);
            }
            return;
        }

        var overlay = maybePos.Value.collider.GetComponent<OverlayTile>();
        if (overlay == null)
        {
            Debug.LogWarning($"RegisterUnits: overlaytile nulo para {unit.name}");
            return;
        }

        if (unit.isEnemy)
        {
            // Asegurar tamaño de lista
            while (PositionEnemy.Count <= EnemyUnity.Count)
                PositionEnemy.Add(null);

            PositionEnemy[EnemyUnity.Count] = overlay;
            overlay.isBlocked = true;

        }
        else
        {
            while (PositionPlayer.Count <= PlayerUnity.Count)
                PositionPlayer.Add(null);

            PositionPlayer[PlayerUnity.Count] = overlay;
            overlay.isBlocked = true;
        }
    }

    public void CharacterPosition(Unit unit)
    {
        if (unit == null) return;

        // Buscar índice del jugador en PlayerUnity
        int idx = PlayerUnity.IndexOf(unit);
        if (idx < 0)
        {
            Debug.LogWarning($"CharacterPosition: unidad {unit.name} no encontrada en PlayerUnity");
            return;
        }

        var position = unit.FindCenterTile();
        if (position == null)
        {
            Debug.Log($"{unit.name}: CharacterPosition -> no hay casilla (position null)");
            return;
        }

        OverlayTile newPos = position;
        // Aseguramos que PositionPlayer tenga índice idx
        while (PositionPlayer.Count <= idx)
            PositionPlayer.Add(null);

        var prev = PositionPlayer[idx];
        if (prev != null && prev != newPos)
            prev.isBlocked = false;

        PositionPlayer[idx] = newPos;
        newPos.isBlocked = true;
    }

    /// <summary>
    /// Añade dinámicamente un GameObject enemigo (instanciado o activado) al BattleSystem.
    /// Se encarga de sincronizar listas internas, bloquear tile y asignar HUD si hay slot.
    /// </summary>
    public void AddEnemyToBattle(GameObject enemyGO)
    {
        if (enemyGO == null)
        {
            return;
        }

        Unit u = enemyGO.GetComponent<Unit>();
        if (u == null) { Debug.LogWarning("AddEnemyToBattle: no tiene Unit"); return; }

        // 1) registrar en listas (tu código original)
        RegisterUnits(u);
        if (!EnemiesPrefab.Contains(enemyGO)) EnemiesPrefab.Add(enemyGO);
        if (!EnemyUnity.Contains(u)) EnemyUnity.Add(u);
        //int enemyCount = Mathf.Min(EnemyUnity.Count, enemyHUD.Count);
        //for (int i = 0; i < enemyCount; i++) enemyHUD[i].SetHUD(EnemyUnity[i]);

        // 2) intentar resolver overlay tile de forma robusta
        OverlayTile resolvedOverlay = null;
        var maybe = u.ActiveTile();
        if (maybe.HasValue)
        {
            resolvedOverlay = maybe.Value.collider.GetComponent<OverlayTile>();
        }
        else if (MapManager.Instance != null && MapManager.Instance.TryGetOverlayTileAtWorldPos(enemyGO.transform.position, out var tileFromMap))
        {
            resolvedOverlay = tileFromMap;
        }

        if (resolvedOverlay != null)
        {
            // Actualiza PositionEnemy / isBlocked via tu método (asegura consistencia)
            UpdateEnemyPosition(u, resolvedOverlay);
        }
        else
        {
            Debug.LogWarning($"[AddEnemyToBattle] No se pudo resolver overlay para {enemyGO.name}");
        }

        // 3) Ahora que BattleSystem intentó fijar la casilla, asignar referencias y llamar InitAfterSpawn
        var enemyIA = enemyGO.GetComponent<EnemyIA>();
        if (enemyIA != null)
        {
            enemyIA.battleSystem = this;
            enemyIA.mouseController = this.mouseController;
            if (PlayersPrefab != null)
            {
                if (PlayersPrefab.Count > 0) enemyIA.Player1 = PlayersPrefab[0];
                if (PlayersPrefab.Count > 1) enemyIA.Player2 = PlayersPrefab[1];
                if (PlayersPrefab.Count > 2) enemyIA.Player3 = PlayersPrefab[2];
            }

            // Llamada crítica: inicializar IA después de intentar fijar la casilla
            enemyIA.InitAfterSpawn(this, this.mouseController, enemyIA.Player1, enemyIA.Player2, enemyIA.Player3);
        }
        else
        {
            Debug.LogWarning($"[AddEnemyToBattle] EnemyIA no encontrado en {enemyGO.name}");
        }
    }

    private IEnumerator RunTurns()
    {
        while (!BattleOver())
        {
            // 4) Siguiente unidad en orden prefijado
            _currentUnit = initiativeManager.GetNextUnit();
            if (_currentUnit == null)
            {
                continue;
            }
            
            OnTurnStart?.Invoke(_currentUnit);

            var detailsUI = FindAnyObjectByType<CharacterDetailsUI>();
            if (detailsUI != null)
                detailsUI.ShowDetails(_currentUnit);

            // 5) Desactiva siempre todos los inputs/panels
            mouseController.canMove = false;
            mouseController.canAttack = false;
            mouseController.showPanelAcciones = false;
            mouseController.turnEnded = false;

            // 6) Activa/desactiva visualmente y funcionalmente a todos
            SetActiveUnit(_currentUnit);

            // 7) Ejecuta el turno según sea aliado o enemigo
            if (_currentUnit.CompareTag("Aliado"))
            {
                // arrancamos player turn con watchdog
                yield return PlayerTurn(_currentUnit);
            }
            else
            {
                yield return EnemyTurn(_currentUnit);
            }
        }

        Debug.Log("¡Batalla terminada!");
    }

    private IEnumerator PlayerTurn(Unit ally)
    {
        // ✅ Double-check: Should this player skip their turn?
        var statusManager = ally.GetComponent<StatusEffectManager>();
        if (statusManager != null && statusManager.ShouldSkipTurn())
        {
            Debug.Log($"Player {ally.name} is skipping turn due to status effects!");
            mouseController.turnEnded = true;
            yield break;
        }

        // Habilita lógica de entrada (mover/atacar).
        mouseController.canMove = true;
        mouseController.canAttack = true;
        mouseController.showPanelAcciones = true;
        mouseController.enabled = true;
        mouseController.canPocion = true;

        float timeLeft = 60f;

        while (timeLeft > 0f && !mouseController.turnEnded)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        mouseController.DeselectCharacter();
    }

    private IEnumerator EnemyTurn(Unit enemy)
    {
        // ✅ Double-check: Should this enemy skip their turn?
        var statusManager = enemy.GetComponent<StatusEffectManager>();
        if (statusManager != null && statusManager.ShouldSkipTurn())
        {
            Debug.Log($"Enemy {enemy.name} is skipping turn due to status effects!");
            mouseController.turnEnded = true;
            yield break;
        }

        // Deshabilitar inputs de jugador
        mouseController.enabled = false;
        mouseController.canMove = false;
        mouseController.canAttack = false;
        mouseController.showPanelAcciones = false;
        int n = 0;

        for (int i = 0; i < EnemyUnity.Count; i++)
        {
            if (EnemyUnity[i] == enemy)
            {
                n = i;
                break;
            }
        }
        EnemyIAs[n].currentUnit = enemy;
        EnemyIAs[n].LogicAI();

        float timeLeft = 5f;

        while (timeLeft > 0f && !mouseController.turnEnded)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // después de que mouseController.turnEnded sea true:
            for (int i = 0; i < EnemiesPrefab.Count; i++)
            {
                if (EnemiesPrefab[i] == null) continue;
                Unit unit = EnemiesPrefab[i].GetComponent<Unit>();
                if (unit == null) continue;

                if (enemy == unit)
                {
                    var positionMaybe = enemy.ActiveTile();
                    if (!positionMaybe.HasValue)
                    {
                        Debug.LogWarning($"EnemyTurn: active tile nulo para {enemy.name}");
                        continue;
                    }

                    OverlayTile newPosition = positionMaybe.Value.collider.GetComponent<OverlayTile>();
                    if (newPosition == null) continue;

                    // Asegurar capacidad
                    while (PositionEnemy.Count <= i)
                        PositionEnemy.Add(null);

                    var prevPos = PositionEnemy[i];
                    if (prevPos == null)
                    {
                        // Si estaba vacío, simplemente asignar
                        PositionEnemy[i] = newPosition;
                        newPosition.isBlocked = true;
                    }
                    else if (prevPos != newPosition)
                    {
                        // liberar anterior y ocupar nuevo
                        prevPos.isBlocked = false;
                        newPosition.isBlocked = true;
                        PositionEnemy[i] = newPosition;
                    }
                }
            }
            mouseController.turnEnded = true;
            mouseController.DeselectCharacter();
        //yield return null;
    }

    /// </summary>
    void SetActiveUnit(Unit current)
    {
        if (MapManager.Instance != null) MapManager.Instance.HideAllTiles();
        if (mouseController != null) mouseController.ClearRangeTiles();

        // Asegurar allUnits no sea null
        if (allUnits == null) allUnits = new List<Unit>();

        foreach (var u in allUnits)
        {
            if (u == null) continue;
            var turnable = u.GetComponent<Turnable>();
            if (turnable == null)
                continue;

            if (u == current)
                turnable.ActivateTurn();
            else
                turnable.DeactivateTurn();
        }

        // Informa al MouseController sobre el current
        if (current != null && mouseController != null)
        {
            var ci = current.GetComponent<CharacterInfo>();
            var unit = current.GetComponent<Unit>();
            if (ci != null)
                mouseController.SetActiveCharacter(ci, unit);
        }

        // ✅ CHECK: Should this unit skip their turn due to status effects?
        var statusManager = current.GetComponent<StatusEffectManager>();
        if (statusManager != null && statusManager.ShouldSkipTurn())
        {
            Debug.Log($"[BattleSystem] {current.name} is skipping turn due to status effects (Stun, etc.)!");
            
            // Skip this turn immediately by setting turnEnded = true
            mouseController.turnEnded = true;
            return;
        }

        if (attackController != null) attackController.SetCurrentUnit(current);
        // >>> Forzamos refrescar el HUD de detalles:
        if (detailsUI != null)
            detailsUI.ShowDetails(current);

        var zoom = Camera.main.GetComponent<Zoom>();
        if (zoom != null)
        {
            zoom.SetTarget(current.transform);
        }
    }

    public bool BattleOver()
    {
        bool allDeadEnemies = initiativeManager.enemies.All(e => e.currentHP <= 0);
        bool allDeadAllies = initiativeManager.allies.All(a => a.currentHP <= 0);
        return allDeadEnemies || allDeadAllies;
    }

    public void UpdateEnemyPosition(Unit enemyUnit, OverlayTile newTile)
    {
        if (enemyUnit == null || newTile == null) return;

        // Asegurarse de que la unidad esté registrada
        if (!EnemyUnity.Contains(enemyUnit))
        {
            RegisterUnits(enemyUnit);
        }

        int idx = EnemyUnity.IndexOf(enemyUnit);
        if (idx < 0)
        {
            return;
        }

        // Asegurar capacidad de PositionEnemy
        while (PositionEnemy.Count <= idx)
            PositionEnemy.Add(null);

        var prev = PositionEnemy[idx];
        if (prev != null && prev != newTile)
        {
            prev.isBlocked = false;
        }

        PositionEnemy[idx] = newTile;
        newTile.isBlocked = true;
    }

    public void finalPosition()
    {
        finalPositionPlayers++;

        if(finalPositionPlayers >= PlayerUnity.Count)
        {
            mouseController.FinalDialogue();
        }
    }
}