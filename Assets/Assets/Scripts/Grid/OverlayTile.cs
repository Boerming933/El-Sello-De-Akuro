using System;
using UnityEngine;

public class OverlayTile : MonoBehaviour
{
    public Int32 G;
    public Int32 H;

    public Int32 F { get { return G + H; } }

    public OverlayTile targetReference;

    public bool isBlocked;

    public OverlayTile previous;

    public Vector3Int gridLocation;

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }
    
    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
    }
}
