using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    public static MapManager Instance { get { return _instance; } }

    public OverlayTile overlayTilePrefab;
    public GameObject OverlayContainer;

    public Dictionary<Vector2Int, OverlayTile> map;
    public Tilemap _tilemap;
    

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
        _tilemap = GetComponentInChildren<Tilemap>();
        //var tileMap = gameObject.GetComponentInChildren<Tilemap>();

        map = new Dictionary<Vector2Int, OverlayTile>();

        BoundsInt bounds = _tilemap.cellBounds;



        for (int z = bounds.max.z; z >= bounds.min.z; z--)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x < bounds.max.x; x++)
                {

                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector2Int(x, y);

                    if (_tilemap.HasTile(tileLocation) && !map.ContainsKey(tileKey))
                    {
                        var OverlayTile = Instantiate(overlayTilePrefab, OverlayContainer.transform);
                        OverlayTile.HideTile();
                        var cellWorldPosition = _tilemap.GetCellCenterWorld(tileLocation);

                        OverlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y+0.75f, cellWorldPosition.z+1);
                        OverlayTile.GetComponent<SpriteRenderer>().sortingOrder = _tilemap.GetComponent<TilemapRenderer>().sortingOrder+1;
                        OverlayTile.gridLocation = tileLocation;
                        map.Add(tileKey, OverlayTile);
                    }
                }
            }
        }
    }

    public bool TryGetOverlayTileAtWorldPos(Vector3 worldPos, out OverlayTile overlay)
    {
        overlay = null;
        if (_tilemap == null) return false;

        // Convertimos worldPos a celda en 3D y luego a clave 2D
        Vector3Int cell = _tilemap.WorldToCell(worldPos);
        Vector2Int key = new Vector2Int(cell.x, cell.y);
        return map.TryGetValue(key, out overlay);
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
    
    public void HideAllTiles()
    {
        if (map == null) return;

        foreach (var kv in map)
        {
            kv.Value.HideTile();
        }
    }
}
