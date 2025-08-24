using UnityEngine;

public class PlayerTile : MonoBehaviour
{
    public int G;
    public int H;

    public int F { get { return G + H; } }

    public bool isBlocked;

    public OverlayTile previous;
}
