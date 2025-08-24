using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PathfinderEnemy
{
    //public List<PlayerTile> FindPath(PlayerTile start, PlayerTile end)
    //{
        List<PlayerTile> openList = new List<PlayerTile>();
        List<PlayerTile> closeList = new List<PlayerTile>();

       // openList.Add(start);

       // while (openList.Count > 0)
        //{
         //   PlayerTile currentPlayerTile = openList.OrderBy(x => x.F).First();

          //  openList.Remove(currentPlayerTile);
           // closeList.Add(currentPlayerTile);

//            if (currentPlayerTile == end)
  //          {
                //finalize our path
    //        }

      //      var neighbourTiles = GetNeighbourTiles(currentPlayerTile);

        //}
   // }

    private object GetNeighbourTiles(PlayerTile currentPlayerTile)
    {
        throw new NotImplementedException();
    }
}
