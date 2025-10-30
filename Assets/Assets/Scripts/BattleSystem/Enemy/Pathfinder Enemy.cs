using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PathfinderEnemy
{
    public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end1, OverlayTile end2, OverlayTile end3, List<OverlayTile> searcheableTiles)
    {
        var targets = new[] { end1, end2, end3 }
        .Where(t => t != null)
        .Distinct()
        .ToList();

        if (targets.Count == 0)
        {
            Debug.LogWarning("[PathfinderEnemy] no hay objetivos válidos");
            return null;
        }

        List<OverlayTile> openList = new List<OverlayTile>();
        List<OverlayTile> closeList = new List<OverlayTile>();

        start.G = 0;
        start.H = MinHeuristic(start, targets);
        start.previous = null;

        openList.Add(start);

        while (openList.Count > 0)
        {
            OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).ThenBy(x => x.H).First();

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
                if (neighbours == null) continue;
                if (!targets.Contains(neighbours) && neighbours.isBlocked || closeList.Contains(neighbours))
                {
                    continue;
                }

                int tentativeG = currentOverlayTile.G + 1;

                bool isBetter = false;

                if (!openList.Contains(neighbours))
                {
                    openList.Add(neighbours);
                    isBetter = true;
                }
                else if (tentativeG < neighbours.G)
                {
                    isBetter = true;
                }

                if (isBetter)
                {
                    neighbours.previous = currentOverlayTile;
                    neighbours.G = tentativeG;
                    neighbours.H = MinHeuristic(neighbours, targets); // Heurística mínima a cualquier objetivo
                }

            }
        }

        return new List<OverlayTile>();
    }

    private int MinHeuristic(OverlayTile from, List<OverlayTile> targets)
    {
        int best = int.MaxValue;
        foreach (var t in targets)
        {
            int d = GetManhattenDistanceSafe(from, t);
            if (d < best) best = d;
        }
        return best == int.MaxValue ? 0 : best;
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

        if (currentTile == start) finishedList.Reverse();
        else finishedList.Clear(); // ruta inconsistente

        return finishedList;
    }

    public int GetManhattenDistanceSafe(OverlayTile a, OverlayTile b)
    {
        if (a == null || b == null) return int.MaxValue / 2; 
        var da = a.gridLocation;
        var db = b.gridLocation;
        return Mathf.Abs(da.x - db.x) + Mathf.Abs(da.y - db.y);
    }
}