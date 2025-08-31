using System;
using UnityEngine;

public class TurnoVaporeon : MonoBehaviour
{
    public bool vaporeonTurn = false;
    public GameObject AuraV;
    void Update()
    {
        if (vaporeonTurn)
        {
            gameObject.tag = "Aliado";
            AuraV.SetActive(true);
        }
        else
        {
            gameObject.tag = "Untagged";
            AuraV.SetActive(false);
            AuraV.GetComponent<SpriteRenderer>().sortingOrder = 1;

        }
    }
}
