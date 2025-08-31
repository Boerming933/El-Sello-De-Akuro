using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance {get { return _instance; }}

    public OverlayTile overlayTilePrefab;
    public GameObject OverlayContainer;

    public Dictionary<Vector2Int, OverlayTile> map;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else _instance = this;
    }

    void Start()
    {

        var tileMap = gameObject.GetComponentInChildren<Tilemap>();

        map = new Dictionary<Vector2Int, OverlayTile>();

        BoundsInt bounds = tileMap.cellBounds;

        

        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {

                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector2Int(x, y);

                    if (tileMap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        var OverlayTile = Instantiate(overlayTilePrefab, OverlayContainer.transform);
                        OverlayTile.HideTile();
                        var cellWorldPosition = tileMap.GetCellCenterWorld(tileLocation);

                        OverlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z + 1);
                        OverlayTile.GetComponent<SpriteRenderer>().sortingOrder = tileMap.GetComponent<TilemapRenderer>().sortingOrder + 1;
                        OverlayTile.gridLocation = tileLocation;
                        map.Add(tileKey, OverlayTile);
                    }
                }
            }
        }
    }

    public List<OverlayTile> GetNeighbourTiles(OverlayTile currentOverlayTile, List<OverlayTile> searcheableTiles)
    {
        Dictionary<Vector2Int, OverlayTile> tileToSearch = new Dictionary<Vector2Int, OverlayTile>();

        if (searcheableTiles.Count > 0)
        {
            foreach (var item in searcheableTiles)
            {
                if (!tileToSearch.ContainsKey(item.grid2DLocation))
                {
                    tileToSearch.Add(item.grid2DLocation, item);
                }
            }
        }
        else
        {
            tileToSearch = map;
        }

        List<OverlayTile> neighbours = new List<OverlayTile>();

        //arriba
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y + 1
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if (Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
                neighbours.Add(tileToSearch[locationToCheck]);
        }

        //abajo
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x,
            currentOverlayTile.gridLocation.y - 1
        );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if (Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
                neighbours.Add(tileToSearch[locationToCheck]);
        }

        //izquierda
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x - 1,
            currentOverlayTile.gridLocation.y
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if (Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
                neighbours.Add(tileToSearch[locationToCheck]);
        }

        //derecha
        locationToCheck = new Vector2Int(
            currentOverlayTile.gridLocation.x + 1,
            currentOverlayTile.gridLocation.y
            );

        if (tileToSearch.ContainsKey(locationToCheck))
        {
            if (Mathf.Abs(currentOverlayTile.gridLocation.z - tileToSearch[locationToCheck].gridLocation.z) <= 1)
                neighbours.Add(tileToSearch[locationToCheck]);
        }

        return neighbours;
    }
}
