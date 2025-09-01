using System;
using UnityEngine;

public class TurnoUmbreon : MonoBehaviour
{
    public bool umbreonTurn = false;
    public GameObject AuraU;
    void Update()
    {
        if (umbreonTurn)
        {
            gameObject.tag = "Aliado";
            AuraU.SetActive(true);
        }
        else
        {
            gameObject.tag = "Untagged";
            AuraU.SetActive(false);
            AuraU.GetComponent<SpriteRenderer>().sortingOrder = 1;

        }
    }
}
