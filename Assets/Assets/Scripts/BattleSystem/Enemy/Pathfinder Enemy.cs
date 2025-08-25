using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PathfinderEnemy
{
  public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end)
  {
    List<OverlayTile> openList = new List<OverlayTile>();
    List<OverlayTile> closeList = new List<OverlayTile>();

    openList.Add(start);

    while (openList.Count > 0)
    {
      OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();

      openList.Remove(currentOverlayTile);
      closeList.Add(currentOverlayTile);

      if (currentOverlayTile == end)
      {
        return GetFinishedList(start, end);
      }

      var neighbourTiles = GetNeightbourOverlayTiles(currentOverlayTile);

      foreach (var neighbours in neighbourTiles)
      {
        if (neighbours.isBlocked || closeList.Contains(neighbours) || Mathf.Abs(currentOverlayTile.gridLocation.z - neighbours.gridLocation.z) > 1)
        {
          continue;
        }

        neighbours.G = GetManhattenDistance(start, neighbours);
        neighbours.H = GetManhattenDistance(end, neighbours);

        neighbours.previous = currentOverlayTile;

        if (!openList.Contains(neighbours))
        {
          openList.Add(neighbours);
        }

      }
    }
    
    return new List<OverlayTile>();
  }

  private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end)
  {
    List<OverlayTile> finishedList = new List<OverlayTile>();
    OverlayTile currentTile = end;

    while (currentTile != start)
    {
      finishedList.Add(currentTile);
      currentTile = currentTile.previous;
    }

    finishedList.Reverse();

    return finishedList;     
    
  }

    private int GetManhattenDistance(OverlayTile start, OverlayTile neighbours)
    {
    return Mathf.Abs(start.gridLocation.x - neighbours.gridLocation.x) + Mathf.Abs(start.gridLocation.y - neighbours.gridLocation.y);
    }

    private List<OverlayTile> GetNeightbourOverlayTiles(OverlayTile currentOverlayTile)
  {
    var map = MapManager.Instance.map;
    List<OverlayTile> neighbours = new List<OverlayTile>();

    // Top
    Vector2Int locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x, currentOverlayTile.gridLocation.y + 1);
    if (map.ContainsKey(locationToCheck))
    {
      neighbours.Add(map[locationToCheck]);
    }

    //Bottom
    locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x, currentOverlayTile.gridLocation.y - 1);

    if (map.ContainsKey(locationToCheck))
    {
      neighbours.Add(map[locationToCheck]);
    }

    //Right
    locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x + 1, currentOverlayTile.gridLocation.y);

    if (map.ContainsKey(locationToCheck))
    {
      neighbours.Add(map[locationToCheck]);
    }

    //Left
    locationToCheck = new Vector2Int(currentOverlayTile.gridLocation.x - 1, currentOverlayTile.gridLocation.y);

    if (map.ContainsKey(locationToCheck))
    {
      neighbours.Add(map[locationToCheck]);
    }

    return neighbours;
    
  }
}
