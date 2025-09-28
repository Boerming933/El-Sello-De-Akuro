using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/BlockedRuleTile", fileName = "NewBlockedRuleTile")]
public class BlockedRuleTile : RuleTile<BlockedRuleTile.Neighbor>
{
    [Header("¿Se puede pasar por aquí?")]
    public bool isBlocked = false;

    [System.Serializable]
    public class Neighbor : RuleTile.TilingRule.Neighbor { }
}