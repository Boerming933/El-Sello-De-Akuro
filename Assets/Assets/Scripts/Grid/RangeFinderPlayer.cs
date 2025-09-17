using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RangeFinderPlayer
{
    BattleSystem battleSystem;
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

        var occupiedEnemy = battleSystem.PositionEnemy;
        var occupiedPlayer = battleSystem.PositionPlayer;

        return inRangeTiles.Distinct().Where(tile => !occupiedEnemy.Contains(tile)).ToList();
    }
}
