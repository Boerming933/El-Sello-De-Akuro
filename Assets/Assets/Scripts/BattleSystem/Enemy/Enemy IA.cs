using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class EnemyIA : MonoBehaviour
{
    private PathfinderEnemy pathfinder;
    private Enemy Enemy;
    public LayerMask tileLayerMask;
    public OverlayTile Active;

    public float speed;
    private int stepsMoved = 0;
    private bool isMoving = false;

    public GameObject Player1, Player2, Player3;

    private List<OverlayTile> path;
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>();
    private Vector2Int position = new Vector2Int(0, 0);

    public BattleSystem battleSystem;
    public MouseControler mouseController;
    private Unit myUnit;
    private bool hasFinishedMovementThisTurn = false;

    private void Start()
    {
        pathfinder = new PathfinderEnemy();
        path = new List<OverlayTile>();
        Enemy = GetComponent<Enemy>();
        myUnit = GetComponent<Unit>();
        Debug.Log($"[EnemyIA.Start] name={name} myUnit={(myUnit != null ? myUnit.name : "null")} battleSystem={(battleSystem != null ? "ok" : "null")} mouseController={(mouseController != null ? "ok" : "null")}");

    }

    public void InitAfterSpawn(BattleSystem bs, MouseControler mc, GameObject p1 = null, GameObject p2 = null, GameObject p3 = null)
    {
        Debug.Log($"[EnemyIA.InitAfterSpawn] called for {name}. bs={(bs != null ? "ok" : "null")} mc={(mc != null ? "ok" : "null")} p1={(p1 != null ? p1.name : "null")}");
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
        Debug.Log($"[EnemyIA.InitAfterSpawn] resolved Active={(Active != null ? Active.grid2DLocation.ToString() : "null")}, Player1={(Player1 != null ? Player1.name : "null")}, myUnit={(myUnit != null ? myUnit.name : "null")}");
        
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
                Debug.Log($"[EnemyIA.InitAfterSpawn] Fallback resolved Active for {name} -> {t.grid2DLocation}");
            }
        }
    }

    private void Update()
    {
        var platerTile1 = Player1Tile();
        var platerTile2 = Player2Tile();
        var platerTile3 = Player3Tile();
        var active = myUnit.ActiveTile();

        // Si no hay tiles de players, no hacemos nada
        if (!platerTile1.HasValue && !platerTile2.HasValue && !platerTile3.HasValue)
            return;

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

        // Resolver tiles de jugadores sólo si existen (null-safe)
        OverlayTile overlayTile1 = platerTile1.HasValue ? platerTile1.Value.collider.GetComponent<OverlayTile>() : null;
        OverlayTile overlayTile2 = platerTile2.HasValue ? platerTile2.Value.collider.GetComponent<OverlayTile>() : null;
        OverlayTile overlayTile3 = platerTile3.HasValue ? platerTile3.Value.collider.GetComponent<OverlayTile>() : null;

        // Asignamos Active y actualizamos sorting (siempre comprobando nulls)
        Active = resolvedActive;
        if (overlayTile1 != null && Player1 != null) Player1.GetComponent<SpriteRenderer>().sortingOrder = overlayTile1.GetComponent<SpriteRenderer>().sortingOrder;
        if (overlayTile2 != null && Player2 != null) Player2.GetComponent<SpriteRenderer>().sortingOrder = overlayTile2.GetComponent<SpriteRenderer>().sortingOrder;
        if (overlayTile3 != null && Player3 != null) Player3.GetComponent<SpriteRenderer>().sortingOrder = overlayTile3.GetComponent<SpriteRenderer>().sortingOrder;

        // CONTADOR DE PASOS (igual que antes)
        if (Active != null && position != Active.grid2DLocation)
        {
            stepsMoved++;
            position = Active.grid2DLocation;
            Debug.Log($"[EnemyIA] Llego a nuevo tile. stepsMoved incrementado a {stepsMoved} para {name} - Active={position}");
            if (battleSystem != null)
                battleSystem.CharacterPosition(myUnit);
        }

        // cálculo de path (igual que antes) - usa overlayTile1/2/3 que pueden ser null
        if (!isMoving)
        {
            var fullPath = pathfinder.FindPath(Active, overlayTile1, overlayTile2, overlayTile3, inRangeTiles);
            if (fullPath != null && fullPath.Count > 0)
            {
                if (fullPath.Count > 0 && fullPath[0] == Active)
                    fullPath.RemoveAt(0);
                path = fullPath.Take(myUnit.movement).ToList();
            }
            else
            {
                path = new List<OverlayTile>();
            }
        }

        if (!isMoving && battleSystem.CurrentUnit == myUnit && path.Count > 0)
        {
            // ✅ CRITICAL: Check if this enemy should skip their entire turn
            var statusManager = GetComponent<StatusEffectManager>();
            if (statusManager != null && statusManager.ShouldSkipTurn())
            {
                Debug.Log($"{name} is skipping entire turn due to status effects (Stun)!");
                
                // Skip entire turn
                isMoving = false;
                hasFinishedMovementThisTurn = true;
                mouseController.turnEnded = true;
                return;
            }
            
            // Check if this enemy can move due to status effects
            if (statusManager != null && !statusManager.CanMove())
            {
                Debug.Log($"{name} cannot move due to status effects (Hypnotic Chant)!");
                
                // Skip movement but still end turn
                isMoving = false;
                hasFinishedMovementThisTurn = true;
                mouseController.turnEnded = true;
                return;
            }

            isMoving = true;
            hasFinishedMovementThisTurn = false; // resetear al inicio del movimiento
            stepsMoved = 0;                       // asegurar contador limpio
            Debug.Log($"{name}: Start moving (path count {path.Count})");
        }


        if (path.Count > 1 && isMoving)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        if (path == null || path.Count == 0) return; // defensa
        var step = speed * Time.deltaTime;
        Debug.Log("StepsMoved es " + stepsMoved);
        var zIndex = path[0].transform.position.z - 2f;

        Enemy.transform.position = Vector2.MoveTowards(transform.position, path[0].transform.position, step);
        Enemy.transform.position = new Vector3(Enemy.transform.position.x, Enemy.transform.position.y, zIndex);

        if (Vector2.Distance(Enemy.transform.position, path[0].transform.position) < 0.0001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }

        Debug.Log($"[EnemyIA.MoveAlongPath] {name} pathCount={path?.Count} isMoving={isMoving} stepsMoved={stepsMoved}, movementCap={myUnit.movement}");


        if (!hasFinishedMovementThisTurn && path.Count == 1 || stepsMoved >= myUnit.movement)
        {
            hasFinishedMovementThisTurn = true; // evita que esto se repita
            Debug.Log($"[EnemyIA] Movimiento terminado (segunda etapa), setting turnEnded=true para {name}. pathCount={path.Count}, stepsMoved={stepsMoved}");
            isMoving = false;
            stepsMoved = 0;
            Debug.Log("StepsMoved es " + stepsMoved);
            Debug.Log($"[EnemyIA] Movimiento terminado, setting turnEnded=true para {name}");
            mouseController.turnEnded = true;
            Debug.Log("isMoving es " + isMoving);
            Debug.Log($"{name}: finished moving. Ending turn.");
        }
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
    /*
    public RaycastHit2D? ActiveTile()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
*/
    private void PositionCharacterOnTile(OverlayTile tile)
    {
        Enemy.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z - 2f);
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
