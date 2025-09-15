using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RangeFinderPlayer
{
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range)
    {
        var reachable       = new HashSet<OverlayTile> { startingTile };
        var frontier        = new List<OverlayTile>    { startingTile };
        var emptySearchList = new List<OverlayTile>();
        int step = 0;

        while (step < range)
        {
            var nextFrontier = new List<OverlayTile>();

            foreach (var tile in frontier)
            {
                // Pasamos la lista vacía para que el método busque en `map`
                var neighbours = MapManager.Instance.GetNeighbourTiles(tile, emptySearchList);

                foreach (var neighbour in neighbours)
                {
                    if (reachable.Contains(neighbour) || neighbour.isBlocked) continue;

                    reachable.Add(neighbour);
                    nextFrontier.Add(neighbour);
                }
            }

            frontier = nextFrontier;
            step++;
        }

        return reachable.ToList();

    }
}
