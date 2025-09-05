using System;
using UnityEngine;

public class TurnoUmbreon : MonoBehaviour
{
    public bool umbreonTurn = false;
    public GameObject AuraU;
    public GameObject PanelAcciones;
    public PanelAcciones panelScript;
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

    private void OnMouseDown()
    {
        if (gameObject.tag == "Aliado")
        {
            panelScript.panelActive = true;
            PanelAcciones.SetActive(true);
        }

    }
}
