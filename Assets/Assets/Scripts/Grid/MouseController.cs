using System.Collections.Generic;
using System.Linq;
using finished3;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseControler : MonoBehaviour
{
    public GameObject characterPrefab;
    private List<OverlayTile> path = new List<OverlayTile>();
    private List<OverlayTile> inRangeTiles = new List<OverlayTile>();

    private CharacterInfo character;
    private PathFinder pathFinder;
    private RangeFinder rangeFinder;

    public float speed;
    public int movement;
    private bool isMoving = false;


    private void Start()
    {
        pathFinder = new PathFinder();
        rangeFinder = new RangeFinder();
        character = GetComponent<CharacterInfo>();
    }

    void Update()
    {
        var focusedTileHit = GetFocusedOnTile();

        if (focusedTileHit.HasValue)
        {
            OverlayTile overlayTile = focusedTileHit.Value.collider.gameObject.GetComponent<OverlayTile>();
            transform.position = overlayTile.transform.position;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = overlayTile.GetComponent<SpriteRenderer>().sortingOrder;

            if (inRangeTiles.Contains(overlayTile) && !isMoving)
            {
                path = pathFinder.FindPath(character.activeTile, overlayTile, inRangeTiles);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (character == null)
                {
                    //PositionCharacterOnTile(overlayTile);
                    //GetInRangeTiles();
                }
                else
                {
                    isMoving = true;
                }
            }
        }

        if (path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    private void GetInRangeTiles()
    {
        foreach (var item in inRangeTiles)
        {
            item.HideTile();
        }
        
        inRangeTiles = rangeFinder.GetTilesInRange(character.activeTile, movement);

        foreach (var item in inRangeTiles)
        {
            item.ShowTile();
        }
    }

    private void MoveAlongPath()
    {
        var stop = speed * Time.deltaTime;

        var zIndex = path[0].transform.position.z;
        character.transform.position = Vector2.MoveTowards(character.transform.position, path[0].transform.position, stop);
        character.transform.position = new Vector3(character.transform.position.x, character.transform.position.y, zIndex);

        if (Vector2.Distance(character.transform.position, path[0].transform.position) < 0.0001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            GetInRangeTiles();
            isMoving = false;
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
        character.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        character.activeTile = tile;
    }
}
