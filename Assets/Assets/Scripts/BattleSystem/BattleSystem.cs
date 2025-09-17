using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public enum BattleState{ START, PLAYERTURN, ENEMYTURN, WON, LOST}

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    public List<GameObject> PlayersPrefab;
    public List<GameObject> EnemiesPrefab;
    public BattleState state;

    public bool start = false;

    public List<BattleHUD> playerHUD;
    public List<BattleHUD> enemyHUD;

    public List<OverlayTile> PositionEnemy;
    public List<OverlayTile> PositionPlayer;

    public List<Unit> PlayerUnity = new List<Unit>();
    public List<Unit> EnemyUnity = new List<Unit>();


    public InitiativeManager initiativeManager;
    public MouseControler     mouseController;  
    // Colección de todos los participantes
    private List<Unit> allUnits;


    public event System.Action<Unit> OnTurnStart;
    public Unit _currentUnit;
    public Unit CurrentUnit => _currentUnit;
    public AttackController attackController;

    public CharacterDetailsUI detailsUI;

    IEnumerator Start()
    {
        // Espera hasta que MapManager.map ya exista
        yield return new WaitUntil(() => MapManager.Instance != null 
                                    && MapManager.Instance.map  != null);

        StartBattle();
    }

    void StartBattle()
    {
        // 1) Reúne aliados y enemigos en un solo listado
        allUnits = initiativeManager.allies
            .Concat(initiativeManager.enemies)
            .ToList();

        // 2) Rola iniciativa UNA VEZ
        initiativeManager.RollInitiative();

        // 3) Arranca el bucle de turnos
        StartCoroutine(RunTurns());

        start = true;

        for (int i = 0; i < PlayersPrefab.Count; i++)
        {
            Unit unit = PlayersPrefab[i].GetComponent<Unit>();
            RegisterUnits(unit);
            PlayerUnity.Add(unit);
        }

        // Instanciar enemigos y capturar sus componentes Unit
        for (int i = 0; i < EnemiesPrefab.Count; i++)
        {
            Unit unit = EnemiesPrefab[i].GetComponent<Unit>();
            RegisterUnits(unit);
            EnemyUnity.Add(unit);
        }

        int playerCount = Mathf.Min(PlayerUnity.Count, playerHUD.Count);
        int enemyCount  = Mathf.Min(EnemyUnity.Count,  enemyHUD.Count);
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
        var position = unit.ActiveTile();
        if (unit.isEnemy)
        {
            PositionEnemy.Add(position.Value.collider.GetComponent<OverlayTile>());
            OverlayTile Position = position.Value.collider.GetComponent<OverlayTile>();
            Position.isBlocked = true;
        }
        else
        {
            PositionPlayer.Add(position.Value.collider.GetComponent<OverlayTile>());
            OverlayTile Position = position.Value.collider.GetComponent<OverlayTile>();
            Position.isBlocked = true;
        }
    }

    public void CharacterPosition(Unit unit)
    {
        for (int i = 0; i < PlayersPrefab.Count; i++)
        {
            Unit ally = PlayersPrefab[i].GetComponent<Unit>();

            if (unit == ally)
            {
                var position = unit.FindCenterTile();
                if (position != PositionPlayer[i])
                {
                    if (position != null)
                    {
                        Debug.Log("tiene valor");
                        PositionPlayer[i].isBlocked = false;
                        Debug.Log(PositionPlayer[i].isBlocked);
                        position.isBlocked = true;
                        PositionPlayer[i] = position;
                    }
                }
            }
        }
    }

    private IEnumerator RunTurns()
    {
        while (!BattleOver())
        {
            // 4) Siguiente unidad en orden prefijado
            _currentUnit = initiativeManager.GetNextUnit();
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
                yield return PlayerTurn(_currentUnit);
            else
                yield return EnemyTurn(_currentUnit);
        }

        Debug.Log("¡Batalla terminada!");
    }

    private IEnumerator PlayerTurn(Unit ally)
    {
        // Habilita lógica de entrada (mover/atacar).
        mouseController.canMove = true;
        mouseController.canAttack = true;
        mouseController.showPanelAcciones = true;
        mouseController.enabled = true;

        // Espera hasta que el jugador termine
        yield return new WaitUntil(() => mouseController.turnEnded);
     
        mouseController.DeselectCharacter();
    }

    private IEnumerator EnemyTurn(Unit enemy)
    {
        // Deshabilita input de jugador mientras la IA actúa
        mouseController.enabled = false;
        mouseController.canMove = false;
        mouseController.canAttack = false;
        mouseController.showPanelAcciones = false;

        // IA del enemigo…
        yield return new WaitUntil(() => mouseController.turnEnded);

        for (int i = 0; i < EnemiesPrefab.Count; i++)
        {            
            Unit unit = EnemiesPrefab[i].GetComponent<Unit>();

            if (enemy == unit)
            {
                var position = enemy.ActiveTile();
                OverlayTile Position = position.Value.collider.GetComponent<OverlayTile>();
                if (Position != PositionEnemy[i])
                {
                    if (Position != null)
                    {
                        PositionEnemy[i].isBlocked = false;
                        Position.isBlocked = true;
                        PositionEnemy[i] = Position;
                    }
                }
            }
        }
        mouseController.turnEnded = true;
        mouseController.DeselectCharacter();
    }

    /// <summary>
    /// Activa el turno de 'current' y desactiva a todos los demás.
    /// </summary>
    void SetActiveUnit(Unit current)
    {
        MapManager.Instance.HideAllTiles();
        mouseController.ClearRangeTiles();
        foreach (var u in allUnits)
        {
            var turnable = u.GetComponent<Turnable>();
            if (turnable == null)
                continue;

            if (u == current)
                turnable.ActivateTurn();
            // Informa al MouseController
            var ci = current.GetComponent<CharacterInfo>();
            var unit = current.GetComponent<Unit>();
            if (ci != null)
                mouseController.SetActiveCharacter(ci, unit);
            else
                turnable.DeactivateTurn();
        }
        attackController.SetCurrentUnit(current);
        // >>> Forzamos refrescar el HUD de detalles:
        if (detailsUI != null)
            detailsUI.ShowDetails(current);

        var zoom = Camera.main.GetComponent<Zoom>();
        if (zoom != null)
        {
            zoom.SetTarget(current.transform);
        }

        //mouseController.DeselectCharacter(); // limpia cualquier selección previa
    }

    bool BattleOver()
    {
        bool allDeadEnemies  = initiativeManager.enemies .All(e => e.currentHP <= 0);
        bool allDeadAllies   = initiativeManager.allies  .All(a => a.currentHP <= 0);
        return allDeadEnemies || allDeadAllies;
    }
}
