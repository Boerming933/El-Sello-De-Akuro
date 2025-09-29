using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EnemyIA : MonoBehaviour
{
    private PathfinderEnemy pathfinder;
    private Enemy Enemy;
    public LayerMask tileLayerMask;
    public OverlayTile Active;

    public float speed;
    private int stepsMoved = 0;
    private bool isMoving = false;
    public bool inAttackMode = false;
    public bool isMyTurn = false;

    public GameObject Player1, Player2, Player3;

    private List<OverlayTile> path;
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>();
    private Vector2Int position = new Vector2Int(0, 0);

    public BattleSystem battleSystem;
    public MouseControler mouseController;
    public AttackControllerEnemy attackControllerEnemy;
    [SerializeField] private RangeFinderPlayer rangeFinder;
    public Unit currentUnit;
    public Unit myUnit;
    private bool hasFinishedMovementThisTurn = false;

    [SerializeField] public Animator animator;

    private void Start()
    {
        if (rangeFinder == null) rangeFinder = new RangeFinderPlayer();
        pathfinder = new PathfinderEnemy();
        path = new List<OverlayTile>();
        Enemy = GetComponent<Enemy>();
        myUnit = GetComponent<Unit>();
    }

    public void InitAfterSpawn(BattleSystem bs, MouseControler mc, GameObject p1 = null, GameObject p2 = null, GameObject p3 = null)
    {
        battleSystem = bs;
        mouseController = mc;
        if (p1 != null) Player1 = p1;
        if (p2 != null) Player2 = p2;
        if (p3 != null) Player3 = p3;

        // asegurar componentes
        if (pathfinder == null) pathfinder = new PathfinderEnemy();
        if (path == null) path = new List<OverlayTile>();
        if (Enemy == null) Enemy = GetComponent<Enemy>();
        if (myUnit == null) myUnit = GetComponent<Unit>();

        // Intentar asignar Active si la unidad ya está sobre una casilla
        try
        {
            var maybe = myUnit?.ActiveTile();
            if (maybe.HasValue)
            {
                var ov = maybe.Value.collider.GetComponent<OverlayTile>();
                if (ov != null)
                {
                    Active = ov;
                    // sincronizar sorting y tile en caso de spawn manual
                    Enemy.transform.position = new Vector3(ov.transform.position.x, ov.transform.position.y + 0.0001f, ov.transform.position.z - 2f);
                    Enemy.GetComponent<SpriteRenderer>().sortingOrder = ov.GetComponent<SpriteRenderer>().sortingOrder;

                    // En lugar de intentar asignar a un campo que no existe en Unit,
                    // usamos el método seguro del BattleSystem para sincronizar la posición del enemigo.
                    if (battleSystem != null)
                        battleSystem.UpdateEnemyPosition(myUnit, ov);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"InitAfterSpawn: no se pudo setear Active para {name}: {ex.Message}");
        }
        // Si Active aún es null, intentamos resolver por posición world usando MapManager
        if (Active == null && MapManager.Instance != null)
        {
            if (MapManager.Instance.TryGetOverlayTileAtWorldPos(transform.position, out var t))
            {
                Active = t;
                // sincronizar orden y tile visual
                Enemy.transform.position = new Vector3(t.transform.position.x, t.transform.position.y + 0.0001f, t.transform.position.z - 2f);
                Enemy.GetComponent<SpriteRenderer>().sortingOrder = t.GetComponent<SpriteRenderer>().sortingOrder;

                // avisar al BattleSystem que actualice posiciones internas
                battleSystem?.UpdateEnemyPosition(myUnit, t);
            }
        }
    }

    private void Update()
    {
        if (!isMoving)             //DETIENE LA ANIMACION DE MOVERSE DE CADA ENEMIGO
        {
            if (myUnit.Name == "Oni1")
            {
                animator.SetBool("isMovingDown", false);
                animator.SetBool("isMovingUp", false);
            }

            if (myUnit.Name == "Oni2")
            {
                animator.SetBool("isMovingDown", false);
                animator.SetBool("isMovingUp", false);
            }

            if (myUnit.Name == "Oni3")
            {
                animator.SetBool("isMovingDown", false);
                animator.SetBool("isMovingUp", false);
            }
        }

        var active = myUnit.ActiveTile();

        // Intentar resolver la casilla activa (Active) de forma segura.
        // Primero con el Raycast (ActiveTile), si no hay resultado uso MapManager (fallback por posición world).
        OverlayTile resolvedActive = null;
        if (active.HasValue)
        {
            resolvedActive = active.Value.collider.GetComponent<OverlayTile>();
        }
        else if (MapManager.Instance != null && MapManager.Instance.TryGetOverlayTileAtWorldPos(transform.position, out var fallbackTile))
        {
            resolvedActive = fallbackTile;
        }

        // Si todavía no pudimos resolver Active -> salimos para evitar NullReferenceException.
        if (resolvedActive == null)
            return;

        // Asignamos Active y actualizamos sorting (siempre comprobando nulls)
        Active = resolvedActive;

        // CONTADOR DE PASOS (igual que antes)
        if (Active != null && position != Active.grid2DLocation)
        {
            stepsMoved++;
            position = Active.grid2DLocation;
            if (battleSystem != null)
                battleSystem.CharacterPosition(myUnit);
        }

        if (path.Count > 0)
        {
            MoveAlongPath();
            if (Vector2.Distance(Enemy.transform.position, path[0].transform.position) < 0.0001f)
            {
                PositionCharacterOnTile(path[0]);
                path.RemoveAt(0);
            }
        }
        
        if (!isMoving && path.Count > 1)
        {
            isMoving = true;
            hasFinishedMovementThisTurn = false; // resetear al inicio del movimiento
            stepsMoved = 0;                       // asegurar contador limpio
        }
    }

    public void LogicAI()
    {
        var platerTile1 = Player1Tile();
        var platerTile2 = Player2Tile();
        var platerTile3 = Player3Tile();
        
        // Si no hay tiles de players, no hacemos nada
        if (!platerTile1.HasValue && !platerTile2.HasValue && !platerTile3.HasValue)
            return;

        // Resolver tiles de jugadores sólo si existen (null-safe)
        OverlayTile overlayTile1 = platerTile1.HasValue ? platerTile1.Value.collider.GetComponent<OverlayTile>() : null;
        OverlayTile overlayTile2 = platerTile2.HasValue ? platerTile2.Value.collider.GetComponent<OverlayTile>() : null;
        OverlayTile overlayTile3 = platerTile3.HasValue ? platerTile3.Value.collider.GetComponent<OverlayTile>() : null;

        if (overlayTile1 != null && Player1 != null) Player1.GetComponent<SpriteRenderer>().sortingOrder = overlayTile1.GetComponent<SpriteRenderer>().sortingOrder;
        if (overlayTile2 != null && Player2 != null) Player2.GetComponent<SpriteRenderer>().sortingOrder = overlayTile2.GetComponent<SpriteRenderer>().sortingOrder;
        if (overlayTile3 != null && Player3 != null) Player3.GetComponent<SpriteRenderer>().sortingOrder = overlayTile3.GetComponent<SpriteRenderer>().sortingOrder;

        // cálculo de path (igual que antes) - usa overlayTile1/2/3 que pueden ser null
        if (!isMoving)
        {
            var range = rangeFinder.GetTilesInRange(Active, myUnit.movement);
            attackControllerEnemy.playerPosition(overlayTile1, overlayTile2, overlayTile3);
            var canPlan = attackControllerEnemy.CanAttackFrom(range, Active);

            var moveTo = attackControllerEnemy.FinalMoveTile();  // casilla donde me paro para atacar
            var select = attackControllerEnemy.AttackTile();     // tile que voy a seleccionar para el área
            var attack = attackControllerEnemy.ChosenAttack(); // ataque que va a utilizar

            if (canPlan)
            {
                if (moveTo == null || Active == null)
                {
                    // Ya estoy en la casilla ideal → NO me muevo este turno
                }
                else if (moveTo == Active)
                {

                    // plan quieto
                    path.Clear();

                    // Hasta que implementes ataques, cerrá turno acá
                    FinishTurn();
                }
                else
                {
                    var toPlan = pathfinder.FindPath(Active, moveTo, null, null, inRangeTiles);
                    if (toPlan != null && toPlan.Count > 0 && toPlan[0] == Active) toPlan.RemoveAt(0);
                    path = (toPlan ?? new List<OverlayTile>()).Take(myUnit.movement).ToList();
                }
            }
            else
            {
                if (moveTo != Active)
                {
                    var fullPath = pathfinder.FindPath(Active, overlayTile1, overlayTile2, overlayTile3, inRangeTiles);

                    if (fullPath != null && fullPath.Count > 0)
                    {

                        if (fullPath.Count > 0 && fullPath[0] == Active)
                            fullPath.RemoveAt(0);

                        path = fullPath.Take(myUnit.movement).Where(t => t != null).ToList();
                    }
                }
                else
                {
                    attackControllerEnemy.ConfirmAttack(select,attack);
                }
            }
        }

    }

    private void MoveAlongPath()
    {
        if (myUnit.Name == "Oni1")                               //ONI 1
        {
            float nextY = path[0].transform.position.y;
            float currentY = Enemy.transform.position.y;

            if (nextY > currentY)
            {
                animator.SetBool("isMovingUp", true);
                animator.SetBool("isMovingDown", false);
            }
            else if (nextY < currentY)
            {
                animator.SetBool("isMovingUp", false);
                animator.SetBool("isMovingDown", true);
            }
        }

        if (myUnit.Name == "Oni1")
        {
            float nextX = path[0].transform.position.x;
            float currentX = Enemy.transform.position.x;

            var sr = Enemy.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (nextX > currentX)
                {
                    sr.flipX = true; 
                }
                else if (nextX < currentX)
                {
                    sr.flipX = false; 
                }
            }
        }

        if (myUnit.Name == "Oni2")                            //ONI 2
        {
            float nextY = path[0].transform.position.y;
            float currentY = Enemy.transform.position.y;

            if (nextY > currentY)
            {
                animator.SetBool("isMovingUp", true);
                animator.SetBool("isMovingDown", false);
            }
            else if (nextY < currentY)
            {
                animator.SetBool("isMovingUp", false);
                animator.SetBool("isMovingDown", true);
            }
        }

        if (myUnit.Name == "Oni2")
        {
            float nextX = path[0].transform.position.x;
            float currentX = Enemy.transform.position.x;

            var sr = Enemy.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (nextX > currentX)
                {
                    sr.flipX = true; 
                }
                else if (nextX < currentX)
                {
                    sr.flipX = false;
                }
            }
        }

        if (myUnit.Name == "Oni3")                            //ONI 3
        {
            float nextY = path[0].transform.position.y;
            float currentY = Enemy.transform.position.y;

            if (nextY > currentY)
            {
                animator.SetBool("isMovingUp", true);
                animator.SetBool("isMovingDown", false);
            }
            else if (nextY < currentY)
            {
                animator.SetBool("isMovingUp", false);
                animator.SetBool("isMovingDown", true);
            }
        }

        if (myUnit.Name == "Oni3")
        {
            float nextX = path[0].transform.position.x;
            float currentX = Enemy.transform.position.x;

            var sr = Enemy.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (nextX > currentX)
                {
                    sr.flipX = true;
                }
                else if (nextX < currentX)
                {
                    sr.flipX = false;
                }
            }
        }


        if (path == null) return; // defensa
        var step = speed * Time.deltaTime;
        var zIndex = path[0].transform.position.z - 1f;

        transform.position = Vector2.MoveTowards(transform.position, path[0].transform.position, step);
        transform.position = new Vector3(transform.position.x, transform.position.y, zIndex);
        
        if (!hasFinishedMovementThisTurn && (path.Count == 1 || stepsMoved >= myUnit.movement))
        {
            FinishTurn();
        }
    }

    public void FinishTurn()
    {
        
        hasFinishedMovementThisTurn = true;

        isMoving = false;
        stepsMoved = 0;
        mouseController.turnEnded = true;
        attackControllerEnemy.ReduceCooldowns();

        return;        
    }

    public RaycastHit2D? Player1Tile()
    {
        Vector2 origin = new Vector2(Player1.transform.position.x, Player1.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero, 0f, tileLayerMask);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    public RaycastHit2D? Player2Tile()
    {
        Vector2 origin = new Vector2(Player2.transform.position.x, Player2.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero, 0f, tileLayerMask);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    public RaycastHit2D? Player3Tile()
    {
        Vector2 origin = new Vector2(Player3.transform.position.x, Player3.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero, 0f, tileLayerMask);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
    
    private void PositionCharacterOnTile(OverlayTile tile)
    {
        transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z - 1f);
        Enemy.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        Enemy.activeTile = tile;
        if (battleSystem != null)
        {
            battleSystem.UpdateEnemyPosition(myUnit, tile);
        }
    }
    
    // Métodos de inspección para el watchdog (debug)

    public bool IsMovingDebug() => isMoving;
    public int PathCountDebug() => (path != null) ? path.Count : -1;
}