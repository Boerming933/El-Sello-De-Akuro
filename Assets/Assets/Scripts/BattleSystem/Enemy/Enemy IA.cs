using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyIA : MonoBehaviour
{
    private PathfinderEnemy pathfinder;
    public float speed;
    private Enemy Enemy;
    public GameObject Player1,Player2,Player3;
    private List<OverlayTile> path;

    private void Start()
    {
        pathfinder = new PathfinderEnemy();
        path = new List<OverlayTile>();
        Enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        var platerTile1 = Player1Tile();
        var platerTile2 = Player2Tile();
        var platerTile3 = Player3Tile();
        var active = ActiveTile();

        if (platerTile1.HasValue || platerTile2.HasValue || platerTile3.HasValue)
        {
            OverlayTile overlayTile1 = platerTile1.Value.collider.GetComponent<OverlayTile>();
            OverlayTile overlayTile2 = platerTile2.Value.collider.GetComponent<OverlayTile>();
            OverlayTile overlayTile3 = platerTile3.Value.collider.GetComponent<OverlayTile>();
            OverlayTile Active = active.Value.collider.GetComponent<OverlayTile>();
            Player1.GetComponent<SpriteRenderer>().sortingOrder = overlayTile1.GetComponent<SpriteRenderer>().sortingOrder;
            Player2.GetComponent<SpriteRenderer>().sortingOrder = overlayTile2.GetComponent<SpriteRenderer>().sortingOrder;
            Player3.GetComponent<SpriteRenderer>().sortingOrder = overlayTile3.GetComponent<SpriteRenderer>().sortingOrder;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                path = pathfinder.FindPath(Active, overlayTile1, overlayTile2, overlayTile3);
            }
        }

        if (path.Count > 1)
        {
            MoveAlongPath();
        }

    }    

    private void MoveAlongPath()
    {
        var step = speed * Time.deltaTime;

        var zIndex = path[0].transform.position.z - 2f;

        Enemy.transform.position = Vector2.MoveTowards(transform.position, path[0].transform.position, step);
        Enemy.transform.position = new Vector3(Enemy.transform.position.x, Enemy.transform.position.y, zIndex);

        if (Vector2.Distance(Enemy.transform.position, path[0].transform.position) < 0.0001f)
        {
            PositionCharacterOnTile(path[0]);
            path.RemoveAt(0);
        }
    }

    public RaycastHit2D? Player1Tile()
    {
        Vector2 origin = new Vector2(Player1.transform.position.x,Player1.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
    
    public RaycastHit2D? Player2Tile()
    {
        Vector2 origin = new Vector2(Player2.transform.position.x,Player2.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
    
    public RaycastHit2D? Player3Tile()
    {
        Vector2 origin = new Vector2(Player3.transform.position.x,Player3.transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }
    
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

    private void PositionCharacterOnTile(OverlayTile tile)
    {
        Enemy.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.0001f, tile.transform.position.z - 2f);
        Enemy.GetComponent<SpriteRenderer>().sortingOrder = tile.GetComponent<SpriteRenderer>().sortingOrder;
        Enemy.activeTile = tile;
    }

}
