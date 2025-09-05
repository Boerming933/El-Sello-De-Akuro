using System;
using UnityEngine;

public class TurnoVaporeon : MonoBehaviour
{
    public bool vaporeonTurn = false;
    public GameObject AuraV;
    public GameObject PanelAcciones;
    public PanelAcciones panelScript;
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

    private void OnMouseDown()
    {
        if (gameObject.tag == "Aliado")
        {
            panelScript.panelActive = true;
            PanelAcciones.SetActive(true);
        }

    }
}
