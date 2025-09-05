using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MouseControler : MonoBehaviour
{
    public float speed;

    private CharacterInfo character; // personaje actualmente seleccionado
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;
    public Turnero turnero;
    public CharacterDetailsUI characterDetailsUI;
    public bool showPanelAcciones = false;
    

    private List<OverlayTile> path = new List<OverlayTile>();
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>();

    public bool canAttack = false;
    public bool canMove = false;
    public bool prevCanMove = false;

    private void Start()
    {
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
    }

    void LateUpdate()
    {
        var focusedTileHit = GetFocusedOnTile();

        if (focusedTileHit.HasValue)
        {
            OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();

            if (overlayTile == null) return;

            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder;

            if (Input.GetMouseButtonDown(0))
            {
                GameObject clickedObject = focusedTileHit.Value.collider.gameObject;

                // Selección de personaje
                if (character == null)
                {
                    var characterHit = Physics2D.OverlapPointAll(overlayTile.transform.position)
                        .FirstOrDefault(obj => obj.CompareTag("Aliado"));

                    if (characterHit != null)
                    {
                        character = characterHit.GetComponent<CharacterInfo>();

                        OverlayTile tileBelow = MapManager.Instance.map.Values
                            .FirstOrDefault(t => Vector2.Distance(t.transform.position, character.transform.position) < 0.1f);

                        if (tileBelow != null)
                        {
                            character.activeTile = tileBelow;
                        }

                        // Mostrar rango sólo si ya está habilitado para moverse
                        if (canMove)
                        {
                            GetInRangeTiles();
                        }

                    }
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if (character.activeTile != null && character != null)
                    {
                        if (canMove && inRangeTiles.Contains(overlayTile))
                        {
                            path = pathFinder.FindPath(character.activeTile, overlayTile, inRangeTiles);
                        }
                       
                    }
                    //else                  //Deseleccionar el personaje al clickear fuera del rango
                    //{                                //actualmente no se usa, pero fuera de combate se usará i guess
                    //    DeselectCharacter();
                    //}

                }
            }
        }

        if (character != null && path.Count > 0)
        {
            MoveAlongPath();
        }

        if (Input.GetKeyDown(KeyCode.Space))      //Termina el turno al presionar espacio
        {
            DeselectCharacter();
            
            return;

        }

        // Disparar/limpiar rango cuando cambia canMove
        try
        {
            if (character != null && character.activeTile != null)
            {
                if (!prevCanMove && canMove)
                {
                    GetInRangeTiles();
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

    private void GetInRangeTiles()
    {
        ClearRangeTiles();

        // RANGO DE MOVIMIENTO
        if (character.tilesMoved <= 6)
        {
            inRangeTiles = rangeFinder.GetTilesInRange(character.activeTile, 4);
        }
        else
        {
            if(character.tilesMoved == 7)
            {
                inRangeTiles = rangeFinder.GetTilesInRange(character.activeTile, 3);
            }
            if (character.tilesMoved == 8)
            {
                inRangeTiles = rangeFinder.GetTilesInRange(character.activeTile, 2);
            }
            if (character.tilesMoved == 9)
            {
                inRangeTiles = rangeFinder.GetTilesInRange(character.activeTile, 1);
            }
        } 
        
        foreach (var item in inRangeTiles)
            {
                item.ShowTile();
            }
    }

    private void ClearRangeTiles()
    {
        foreach (var item in inRangeTiles)
        {
            item.HideTile();
        }
        inRangeTiles.Clear();
    }

    public void DeselectCharacter()
    {
        ClearRangeTiles();
        if (character != null)
        {
            character.tilesMoved = 0;
            character = null;
            turnero.EndTurn();
            characterDetailsUI.HideDetails();
            canMove = false;
            canAttack = false;
            prevCanMove = false;
        }
        path.Clear();
    }

    private void MoveAlongPath()
    {
        if (character == null) return;

        var stop = speed * Time.deltaTime;

        var zIndex = path[0].transform.position.z;
        character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, stop);
        character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);

        if (Vector2.Distance(character.transform.position, path[0].transform.position) < 0.08f)
        {
            OverlayTile reachedTile = path[0];
            path.RemoveAt(0);
            PositionCharacterOnTile(reachedTile);

            character.tilesMoved++;
            Debug.Log("Tiles moved: " + character.tilesMoved);

            if (character.tilesMoved >= 10)  //Termina el turno al moverse x tiles
            {
                DeselectCharacter();
                
                return; 

            }
        }

        if (path.Count == 0)
        {
            // El personaje ya llegó a su destino
            canMove = false;  
            canAttack = false;
            prevCanMove = false;
            showPanelAcciones = true;
            ClearRangeTiles();
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
        character.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder+1;
        if (character.activeTile != null)
            character.activeTile.occupant = null;

        tile.occupant = character;
        character.activeTile = tile;
    }
}