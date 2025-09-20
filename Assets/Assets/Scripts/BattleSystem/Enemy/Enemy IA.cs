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
    private Vector2Int position = new Vector2Int(0,0);

    public BattleSystem battleSystem;
    public MouseControler mouseController;
    private Unit myUnit;

    private void Start()
    {
        pathfinder = new PathfinderEnemy();
        path = new List<OverlayTile>();
        Enemy = GetComponent<Enemy>();
        myUnit = GetComponent<Unit>();
        Debug.Log(isMoving);
    }

    private void Update()
    {
        var platerTile1 = Player1Tile();
        var platerTile2 = Player2Tile();
        var platerTile3 = Player3Tile();
        var active = myUnit.ActiveTile();

        if (platerTile1.HasValue || platerTile2.HasValue || platerTile3.HasValue)
        {
            OverlayTile overlayTile1 = platerTile1.Value.collider.GetComponent<OverlayTile>();
            OverlayTile overlayTile2 = platerTile2.Value.collider.GetComponent<OverlayTile>();
            OverlayTile overlayTile3 = platerTile3.Value.collider.GetComponent<OverlayTile>();
            Active = active.Value.collider.GetComponent<OverlayTile>();
            Player1.GetComponent<SpriteRenderer>().sortingOrder = overlayTile1.GetComponent<SpriteRenderer>().sortingOrder;
            Player2.GetComponent<SpriteRenderer>().sortingOrder = overlayTile2.GetComponent<SpriteRenderer>().sortingOrder;
            Player3.GetComponent<SpriteRenderer>().sortingOrder = overlayTile3.GetComponent<SpriteRenderer>().sortingOrder;

            if (position.x < Active.grid2DLocation.x && position.y < Active.grid2DLocation.y)
            {
                stepsMoved++;
                Debug.Log("StepsMoved es " + stepsMoved);
                position = Active.grid2DLocation;
                battleSystem.CharacterPosition(myUnit);
            }
            if (!isMoving)
            {
                var fullPath = pathfinder.FindPath(Active, overlayTile1, overlayTile2, overlayTile3, inRangeTiles);
                if (fullPath.Count > 0) fullPath.RemoveAt(0);
                path = fullPath.Take(myUnit.movement).ToList();
            }

            if (!isMoving && battleSystem.CurrentUnit == myUnit)
            {
                isMoving = true;
                Debug.Log("isMoving es " + isMoving);
            }
        }

        if (path.Count > 1 && isMoving)
        {
            MoveAlongPath();
        }
        else
        {
            //if (battleSystem._currentUnit.isEnemy == true)
            //{
            //    isMoving = false;
            //    stepsMoved = 0;
            //    Debug.Log("StepsMoved es "+stepsMoved);
            //    mouseController.turnEnded = true;
            //    Debug.Log("isMoving es " + isMoving);
                
            //}
        }
    }

    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;
        Debug.Log("StepsMoved es "+stepsMoved);
        var zIndex = path[0].transform.position.z - 2f;

        Enemy.transform.position = Vector2.MoveTowards(transform.position, path[0].transform.position, step);
        Enemy.transform.position = new Vector3(Enemy.transform.position.x, Enemy.transform.position.y, zIndex);

        if (Vector2.Distance(Enemy.transform.position, path[0].transform.position) < 0.0001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }

        Debug.Log(path.Count);

        if (path.Count == 1 || stepsMoved >= myUnit.movement)
        {
            isMoving = false;
            stepsMoved = 0;
            Debug.Log("StepsMoved es "+stepsMoved);
            mouseController.turnEnded = true;
            Debug.Log("isMoving es " + isMoving);
        }
    }

    public RaycastHit2D? Player1Tile()
    {
        Vector2 origin = new Vector2(Player1.transform.position.x,Player1.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero, 0f, tileLayerMask);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
    
    public RaycastHit2D? Player2Tile()
    {
        Vector2 origin = new Vector2(Player2.transform.position.x,Player2.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero, 0f, tileLayerMask);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
    
    public RaycastHit2D? Player3Tile()
    {
        Vector2 origin = new Vector2(Player3.transform.position.x,Player3.transform.position.y);

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
    }
}
