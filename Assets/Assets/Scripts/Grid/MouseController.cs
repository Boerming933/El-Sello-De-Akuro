using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using TMPro;
using UnityEngine.UI;

public class MouseControler : MonoBehaviour
{
    public float speed;
    public bool canSkip = true;

    private CharacterInfo character; // personaje actualmente seleccionado
    private PathFinder pathFinder;
    private RangeFinderPlayer rangeFinder;
    [HideInInspector] public bool turnEnded = false;
    public CharacterDetailsUI characterDetailsUI;
    public bool showPanelAcciones = false;

    private List<OverlayTile> path = new List<OverlayTile>();
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>();
    public Unit myUnit;
    private OverlayTile previousTile;
    public BattleSystem battleSystem;

    public bool canAttack = false;
    public bool canMove = false;
    public bool prevCanMove = false;

    public GameObject pocionOn;
    public TextMeshProUGUI pociones;
    public int cantidadPociones = 10;
    public bool canPocion = true;
    public GameObject bgIconoPocion;

    public CharacterInfo CurrentCharacter => character;

    [SerializeField] public Animator animatorSamurai;
    [SerializeField] public Animator animatorGeisha;
    [SerializeField] public Animator animatorNinja;


    private void Start()
    {
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinderPlayer();
        myUnit = GetComponent<Unit>();
        pociones.text = cantidadPociones.ToString();
    }

    /// <summary>
    /// El BattleSystem llamará a esto para decirle quién es el personaje activo.
    /// </summary>
    public void SetActiveCharacter(CharacterInfo ci, Unit unit)
    {
        character = ci;
        myUnit = unit;
        
        // ✅ For player characters, check status effects when activated
        if (unit != null && unit.CompareTag("Aliado"))
        {
            var statusManager = unit.GetComponent<StatusEffectManager>();
            if (statusManager != null && statusManager.ShouldSkipTurn())
            {
                Debug.Log($"Player {unit.name} should skip turn due to status effects!");
                // The BattleSystem should handle this, but we can add extra safety here
            }
        }
    }

    /// <summary>
    /// Encuentra el OverlayTile bajo el personaje activo
    /// </summary>
    private OverlayTile FindCenterTile()
    {
        const float threshold = 0.1f;
        return MapManager.Instance
            .map.Values
            .FirstOrDefault(t =>
                Vector2.Distance(
                    new Vector2(t.transform.position.x, t.transform.position.y),
                    new Vector2(character.transform.position.x, character.transform.position.y)
                ) < threshold
            );
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            if (showPanelAcciones && character != null && cantidadPociones > 0 && canPocion)
            {
            var unit = character.GetComponent<Unit>();
            if (unit != null)
            {
                canPocion = false;
                cantidadPociones--;
                pocionOn.SetActive(false);
                bgIconoPocion.SetActive(false);
                pociones.text = cantidadPociones.ToString();
                unit.Heal();

                var turnable = character.GetComponent<Turnable>();
                if (turnable != null && turnable.btnBatalla != null)
                {
                    turnable.btnBatalla.interactable = false;
                }
                TryAutoEndTurn();
            }
                
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && canMove)
        {
            canMove = false;
            ClearRangeTiles();
            showPanelAcciones = true;

            var turnable = character?.GetComponent<Turnable>();
            if (turnable != null)
            {
                turnable.ActivateTurn(); // Reactiva el panel de acciones
            }
        }

        if (canPocion)
        {
            pocionOn.SetActive(true);
            bgIconoPocion.SetActive(true);
        }
        else
        {
            pocionOn.SetActive(false);
            bgIconoPocion.SetActive(false);
        }

        

        pocionOn.SetActive(canPocion && cantidadPociones > 0 && showPanelAcciones);
        bgIconoPocion.SetActive(canPocion && cantidadPociones > 0 && showPanelAcciones);

        var focusedTileHit = GetFocusedOnTile();

        if (focusedTileHit.HasValue)
        {
            OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();

            if (overlayTile == null) return;

            if (canMove && inRangeTiles.Contains(overlayTile))
            {
                transform.position = overlayTile.transform.position;
                gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder;
                GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                GetComponent<SpriteRenderer>().enabled = false; // Ocultar cursor si no está en rango
            }

            if (Input.GetMouseButtonDown(0))
            {
                GameObject clickedObject = focusedTileHit.Value.collider.gameObject;

                // Selección de personaje
                if (character == null)
                {
                    var characterHit = Physics2D
                        .OverlapPointAll(overlayTile.transform.position)
                        .FirstOrDefault(obj => obj.CompareTag("Aliado"));

                    if (characterHit != null)
                    {
                        character = characterHit.GetComponent<CharacterInfo>();

                        OverlayTile tileBelow = MapManager.Instance.map.Values
                            .FirstOrDefault(t => Vector2.Distance(t.transform.position, character.transform.position) < 0.1f);

                        if (tileBelow != null)
                        {
                            character.activeTile = tileBelow;
                            tileBelow = previousTile;
                        }

                        // Mostrar rango sólo si ya está habilitado para moverse
                        if (canMove)
                        {
                            //GetInRangeTiles();
                        }

                    }
                }

                else if (character != null      // hay personaje  
                    && character.activeTile != null  // ya tiene tile base  
                    && canMove                      // está habilitado para mover  
                    && inRangeTiles.Contains(overlayTile)  // el tile clicado está en rango  
                    )
                {
                    // Additional check for status effects blocking movement
                    var statusManager = character.GetComponent<StatusEffectManager>();
                    if (statusManager != null && !statusManager.CanMove())
                    {
                        Debug.Log($"{character.name} tried to move but is affected by status effects!");
                        return; // Block movement
                    }

                    path = pathFinder.FindPath(
                        character.activeTile,
                        overlayTile,
                        inRangeTiles
                    );
                }

                //else                  //Deseleccionar el personaje al clickear fuera del rango
                //{                                //actualmente no se usa, pero fuera de combate se usará i guess
                //    DeselectCharacter();
                //}
            }
        }
        else
        {
            // ✅ Si no hay tile bajo el mouse, ocultar el cursor
            GetComponent<SpriteRenderer>().enabled = false;
        }

        if (character != null && path.Count > 0)
        {
            MoveAlongPath();
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Joystick1Button7))      //Termina el turno al presionar espacio
        {
            if (canSkip)
            {
                // ✅ NEW: Check if character must attack next turn (Draconic Stance effect)
                if (character != null)
                {
                    var statusManager = character.GetComponent<StatusEffectManager>();
                    if (statusManager != null && statusManager.MustAttackNextTurn())
                    {
                        Debug.Log($"{character.name} cannot skip turn - must attack due to Draconic Stance!");
                        return; // Block turn skipping
                    }
                }

                DeselectCharacter();
                return;  
            }
        }

        // Disparar/limpiar rango cuando cambia canMove
        try
        {
            if (character != null && character.activeTile != null)
            {
                if (!prevCanMove && canMove)
                {
                    //GetInRangeTiles();
                }
                else if (prevCanMove && !canMove)
                {
                    ClearRangeTiles();
                }
            }
            else
            {
                // Si no hay personaje válido, aseguramos limpiar rango
                ClearRangeTiles();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("LateUpdate -> Error manejando canMove: " + ex.Message);
        }
        finally
        {
            prevCanMove = canMove;
        }
    }

    public void GetInRangeTiles()
    {
        ClearRangeTiles();

        OverlayTile center = FindCenterTile();
        if (center == null)
        {
            Debug.LogError($"GetInRangeTiles: no encontré OverlayTile bajo {character.name}");
            return;
        }

        character.activeTile = center;

        // RANGO DE MOVIMIENTO
        // 3) Genera el rango tal cual tenías
        int maxRange = character.tilesMoved <= character.maxTiles ? character.maxTiles - character.tilesMoved : 1;

        inRangeTiles = rangeFinder.GetTilesInRange(center, maxRange);

        // 4) Pintar como siempre
        foreach (var item in inRangeTiles)
        {
            item.ShowTile();
        }       
    }

    public void ClearRangeTiles()
    {
        foreach (var item in inRangeTiles)
        {
            item.HideTile();
        }
        inRangeTiles.Clear();
    }

    public void DeselectCharacter()
    {
        animatorSamurai.SetBool("idleBatalla", false);
        animatorGeisha.SetBool("idleBatalla", false);
        animatorNinja.SetBool("idleBatalla", false);


        //canPocion = true;
        canSkip = true;
        if (character != null)
        {
            var turnable = character.GetComponent<Turnable>();
            if (turnable != null)
            {
                turnable.btnBatalla.interactable = true;
                turnable.btnMoverse.interactable = true;
                turnable.DeactivateTurn();   // <— quita el aura aquí

                character.tilesMoved = 0;
            }
        }
        ClearRangeTiles();
        if (character != null)
        {
            character.tilesMoved = 0;
            character = null;
            turnEnded = true;
            characterDetailsUI.HideDetails();
            canMove = false;
            canAttack = false;
            prevCanMove = false;

        }
        path.Clear();
        // === REACTIVAR panelBatallaGeneral tras finalizar turno ===
        var allProxies = Object.FindObjectsByType<AttackButtonProxy>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var proxy in allProxies)
            proxy.ShowGeneralBattlePanel();
    }

    private void TryAutoEndTurn()
    {
        

        if (myUnit.Name == "Riku Takeda")
        {
            animatorSamurai.SetBool("isMovingDown", false);
            animatorSamurai.SetBool("isMovingUp", false);
            
        }

        if (myUnit.Name == "Sayuri")
        {
            animatorGeisha.SetBool("isMovingDown", false);
            animatorGeisha.SetBool("isMovingUp", false);
        }

        if (myUnit.Name == "Raiden")
        {
            animatorNinja.SetBool("isMovingDown", false);
            animatorNinja.SetBool("isMovingUp", false);
        }

        canSkip = true;
        if (character == null) return;

        var turnable = character.GetComponent<Turnable>();
        if (turnable == null) return;

        bool puedeMoverse = turnable.btnMoverse != null && turnable.btnMoverse.interactable;
        bool puedeAtacar = turnable.btnBatalla != null && turnable.btnBatalla.interactable;

        if (!puedeMoverse && !puedeAtacar)
        {
            
            DeselectCharacter();
        }
    }

    private void MoveAlongPath()
    {
        if (myUnit.Name == "Riku Takeda")
        {
            float nextY = path[0].transform.position.y;
            float currentY = character.transform.position.y;

            if (nextY > currentY)
            {
                animatorSamurai.SetBool("isMovingUp", true);
                animatorSamurai.SetBool("isMovingDown", false);
            }
            else if (nextY < currentY)
            {
                animatorSamurai.SetBool("isMovingUp", false);
                animatorSamurai.SetBool("isMovingDown", true);
            }
        }

        if (myUnit.Name == "Riku Takeda")
        {
            float nextX = path[0].transform.position.x;
            float currentX = character.transform.position.x;

            var sr = character.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (nextX > currentX)
                {
                    sr.flipX = true; // mira a la derecha
                }
                else if (nextX < currentX)
                {
                    sr.flipX = false; // mira a la izquierda (por defecto)
                }
            }
        }


        if (myUnit.Name == "Sayuri")
        {
            float nextY = path[0].transform.position.y;
            float currentY = character.transform.position.y;

            if (nextY > currentY)
            {
                animatorGeisha.SetBool("isMovingUp", true);
                animatorGeisha.SetBool("isMovingDown", false);
            }
            else if (nextY < currentY)
            {
                animatorGeisha.SetBool("isMovingUp", false);
                animatorGeisha.SetBool("isMovingDown", true);
            }
        }

        if (myUnit.Name == "Sayuri")
        {
            float nextX = path[0].transform.position.x;
            float currentX = character.transform.position.x;

            var sr = character.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (nextX > currentX)
                {
                    sr.flipX = true; // mira a la derecha
                }
                else if (nextX < currentX)
                {
                    sr.flipX = false; // mira a la izquierda (por defecto)
                }
            }
        }

        if (myUnit.Name == "Raiden")
        {
            float nextY = path[0].transform.position.y;
            float currentY = character.transform.position.y;

            if (nextY > currentY)
            {
                animatorNinja.SetBool("isMovingUp", true);
                animatorNinja.SetBool("isMovingDown", false);
            }
            else if (nextY < currentY)
            {
                animatorNinja.SetBool("isMovingUp", false);
                animatorNinja.SetBool("isMovingDown", true);
            }
        }

        if (myUnit.Name == "Raiden")
        {
            float nextX = path[0].transform.position.x;
            float currentX = character.transform.position.x;

            var sr = character.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (nextX > currentX)
                {
                    sr.flipX = true; // mira a la derecha
                }
                else if (nextX < currentX)
                {
                    sr.flipX = false; // mira a la izquierda (por defecto)
                }
            }
        }


        canSkip = false;

        if (character == null) return;

        var stop = speed * Time.deltaTime;

        var zIndex = path[0].transform.position.z;
        character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, stop);
        character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);

        if (Vector2.Distance(character.transform.position, path[0].transform.position) < 0.0001f)
        {
            OverlayTile reachedTile = path[0];
            PositionCharacterOnTile(reachedTile);
            path.RemoveAt(0);

            character.tilesMoved++;
            battleSystem.CharacterPosition(myUnit);
            
            Debug.Log("Tiles moved: " + character.tilesMoved);

            if (character.tilesMoved >= character.maxTiles)
            {
                canMove = false;
                prevCanMove = false;
                ClearRangeTiles();
                showPanelAcciones = true;

                if (character != null)
                {
                    var turnable = character.GetComponent<Turnable>();
                    if (turnable != null)
                    {
                        if (turnable.btnMoverse != null)
                            turnable.btnMoverse.interactable = false;

                        turnable.ActivateTurn();
                    }
                }

                TryAutoEndTurn();

                return;
            }
        }

        if (path.Count == 0)
        {
            if (myUnit.Name == "Riku Takeda")
            {
                animatorSamurai.SetBool("isMovingDown", false);
                animatorSamurai.SetBool("isMovingUp", false);
            }

            if (myUnit.Name == "Sayuri")
            {
                animatorGeisha.SetBool("isMovingDown", false);
                animatorGeisha.SetBool("isMovingUp", false);
            }

            if (myUnit.Name == "Raiden")
            {
                animatorNinja.SetBool("isMovingDown", false);
                animatorNinja.SetBool("isMovingUp", false);
            }

            // El personaje ya llegó a su destino
            canSkip = true;
            canMove = false;
            canAttack = false;
            prevCanMove = false;
            showPanelAcciones = true;
            ClearRangeTiles();
            if (character != null)
            {
                var turnable = character.GetComponent<Turnable>();
                if (turnable != null)
                {
                    turnable.ActivateTurn();
                }
            }
        }
    }

    public RaycastHit2D? GetFocusedOnTile()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    private void PositionCharacterOnTile(OverlayTile tile)
    {
        character.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.000001f, tile.transform.position.z);
        // var tileOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        character.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder + 1;
        if (character.activeTile != null)
            character.activeTile.occupant = null;

        tile.occupant = character;
        character.activeTile = tile;
    }
    
    public void StartMoveMode()
{
    // ✅ Check if character should skip their entire turn
    if (character != null)
    {
        var statusManager = character.GetComponent<StatusEffectManager>();
        if (statusManager != null && statusManager.ShouldSkipTurn())
        {
            Debug.Log($"{character.name} should skip turn - cannot start move mode!");
            
            // Show a message that the turn is skipped
            canMove = false;
            showPanelAcciones = false;
            turnEnded = true; // End turn immediately
            return;
        }
        
        // Check if character can move due to status effects (but not full skip)
        if (statusManager != null && !statusManager.CanMove())
        {
            Debug.Log($"{character.name} cannot move due to status effects (Hypnotic Chant)!");
            
            // Show a message or visual indicator that movement is blocked
            canMove = false;
            showPanelAcciones = true;
            
            // Reactivate turn panel but keep movement disabled
            var turnable = character.GetComponent<Turnable>();
            if (turnable != null)
            {
                turnable.ActivateTurn();
                if (turnable.btnMoverse != null)
                    turnable.btnMoverse.interactable = false; // Disable move button visually
            }
            
            return; // Don't allow movement
        }
    }

    // Original StartMoveMode logic (if movement is allowed)
    canMove = true;
    prevCanMove = false;
    showPanelAcciones = false;

    ClearRangeTiles();

    if (character != null)
        GetInRangeTiles();
}

}