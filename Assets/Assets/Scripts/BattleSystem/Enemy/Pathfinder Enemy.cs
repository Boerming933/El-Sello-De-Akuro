using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PathfinderEnemy
{
    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end1, OverlayTile end2, OverlayTile end3, List<OverlayTile> searcheableTiles)
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
            else if (currentOverlayTile == end3)
            {
                return GetFinishedList(start, end3);
            }

            var neighbourTiles = MapManager.Instance.GetNeighbourTiles(currentOverlayTile, searcheableTiles);

            foreach (var neighbours in neighbourTiles)
            {
                if (neighbours.isBlocked || closeList.Contains(neighbours))
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
                    if (searcheableTiles.Contains(end1))
                    {
                        neighbours.H = GetManhattenDistance(end1, neighbours);
                        neighbours.targetReference = end1;
                    }
                    else
                    {
                        OverlayTile fallbackTarget = end1;
                        float minF = Mathf.Infinity;
                        foreach (var tile in searcheableTiles)
                        {
                            float H = GetManhattenDistance(end1, tile);
                            float F = G + H;
                            if (F < minF)
                            {
                                minF = F;
                                fallbackTarget = tile;
                            }
                        }

                        neighbours.targetReference = fallbackTarget;
                        neighbours.H = GetManhattenDistance(fallbackTarget, neighbours);
                    }
                }
                else if (minDistance == F2)
                {
                    if (searcheableTiles.Contains(end2))
                    {
                        neighbours.H = GetManhattenDistance(end2, neighbours);
                        neighbours.targetReference = end2;
                    }
                    else
                    {
                        float minF = Mathf.Infinity;
                        OverlayTile fallbackTarget = end2;
                        foreach (var tile in searcheableTiles)
                        {
                            float H = GetManhattenDistance(end2, tile);
                            float F = G + H;
                            if (F < minF)
                            {
                                minF = F;
                                fallbackTarget = tile;
                            }
                        }

                        neighbours.targetReference = fallbackTarget;
                        neighbours.H = GetManhattenDistance(fallbackTarget, neighbours);
                    }
                }
                else
                {
                    if (searcheableTiles.Contains(end3))
                    {
                        neighbours.H = GetManhattenDistance(end3, neighbours);
                        neighbours.targetReference = end3;
                    }
                    else
                    {
                        float minF = Mathf.Infinity;
                        OverlayTile fallbackTarget = end3;
                        foreach (var tile in searcheableTiles)
                        {
                            float H = GetManhattenDistance(end3, tile);
                            float F = G + H;
                            if (F < minF)
                            {
                                minF = F;
                                fallbackTarget = tile;
                            }
                        }

                        neighbours.targetReference = fallbackTarget;
                        neighbours.H = GetManhattenDistance(fallbackTarget, neighbours);
                    }
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

    /*
    private List<OverlayTile> GetNeightbourOverlayTiles(OverlayTile currentOverlayTile, List<OverlayTile> searcheableTile)
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
    */
}
