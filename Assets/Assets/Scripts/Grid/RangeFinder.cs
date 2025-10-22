using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RangeFinder
{
    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range)
    {
        var inRangeTiles = new List<OverlayTile>();
        int stepCount = 0;

        inRangeTiles.Add(startingTile);

        var tileForPreviousStep = new List<OverlayTile>();
        tileForPreviousStep.Add(startingTile);

        while (stepCount < range)
        {
            var surroundingTiles = new List<OverlayTile>();

            foreach (var item in tileForPreviousStep)
            {
                surroundingTiles.AddRange(MapManager.Instance.GetNeighbourTiles(item, new List<OverlayTile>()));
            }

            inRangeTiles.AddRange(surroundingTiles);
            tileForPreviousStep = surroundingTiles.Distinct().ToList();
            stepCount++;
        }

        return inRangeTiles.Distinct().ToList();
    }

    public List<OverlayTile> GetTilesInRange(OverlayTile startingTile, int range, AttackData attack)
    {
        if (attack != null && attack.effectShape == AreaShape.Perpendicular)
        {
            var tiles = GetPerpendicularTiles(startingTile, range);
            Debug.Log($"[RangeFinder] Perpendicular tiles for {attack.attackName}: {string.Join(", ", tiles.Select(t => $"({t.grid2DLocation.x},{t.grid2DLocation.y})"))}");
            return tiles;
        }

        return GetTilesInRange(startingTile, range);
    }

    List<OverlayTile> GetPerpendicularTiles(OverlayTile center, int range)
    {
        var result = new List<OverlayTile>();
        if (center == null) return result;
        if (MapManager.Instance == null || MapManager.Instance.map == null) return result;

        var grid = MapManager.Instance.map;
        Vector2Int origin = center.grid2DLocation;

        result.Add(center);

        for (int distance = 1; distance <= range; distance++)
        {
            Vector2Int right = new Vector2Int(distance, 0);
            Vector2Int left = new Vector2Int(-distance, 0);
            Vector2Int up = new Vector2Int(0, distance);
            Vector2Int down = new Vector2Int(0, -distance);

            if (grid.TryGetValue(origin + right, out var tileRight) && tileRight != null)
                result.Add(tileRight);

            if (grid.TryGetValue(origin + left, out var tileLeft) && tileLeft != null)
                result.Add(tileLeft);

            if (grid.TryGetValue(origin + up, out var tileUp) && tileUp != null)
                result.Add(tileUp);

            if (grid.TryGetValue(origin + down, out var tileDown) && tileDown != null)
                result.Add(tileDown);
        }

        return result;
    }
}
