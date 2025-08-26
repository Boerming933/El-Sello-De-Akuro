using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PathfinderEnemy
{
  public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end1, OverlayTile end2, OverlayTile end3)
  {
    List<OverlayTile> openList = new List<OverlayTile>();
    List<OverlayTile> closeList = new List<OverlayTile>();

    openList.Add(start);

    while (openList.Count > 0)
    {
      OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();

      openList.Remove(currentOverlayTile);
      closeList.Add(currentOverlayTile);

      if (currentOverlayTile == end1)
      {
        return GetFinishedList(start, end1);        
      }
      else if (currentOverlayTile == end2)
      {
        return GetFinishedList(start, end2);
      }
      else if(currentOverlayTile == end3)
      {
        return GetFinishedList(start, end3);
      }

      var neighbourTiles = GetNeightbourOverlayTiles(currentOverlayTile);

      foreach (var neighbours in neighbourTiles)
      {
        if (neighbours.isBlocked || closeList.Contains(neighbours) || Mathf.Abs(currentOverlayTile.gridLocation.z - neighbours.gridLocation.z) > 1)
        {
          continue;
        }

        int G = neighbours.G = GetManhattenDistance(start, neighbours);

        int P1H = GetManhattenDistance(end1, neighbours);
        int P2H = GetManhattenDistance(end2, neighbours);
        int P3H = GetManhattenDistance(end3, neighbours);

        int F1 = G + P1H;
        int F2 = G + P2H;
        int F3 = G + P3H;

        int minDistance = Mathf.Min(F1, F2, F3);

        if (minDistance == F1)
        {
          neighbours.H = GetManhattenDistance(end1, neighbours);
          Debug.Log("Objetivo actual es Vaporeon");
          neighbours.targetReference = end1;
        }
        else if (minDistance == F2)
        {
          neighbours.H = GetManhattenDistance(end2, neighbours);
          Debug.Log("Objetivo actual es Umbreon");
          neighbours.targetReference = end2;
        }
        else
        {
          neighbours.H = GetManhattenDistance(end3, neighbours);
          Debug.Log("Objetivo actual es Leafeon");
          neighbours.targetReference = end3;
        }

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

  public int GetManhattenDistance(OverlayTile start, OverlayTile neighbours)
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
