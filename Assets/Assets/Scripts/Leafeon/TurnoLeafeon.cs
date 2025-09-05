using UnityEngine;

public class TurnoLeafeon : MonoBehaviour
{
    public bool leafeonTurn = false;
    public GameObject AuraL;
    public GameObject PanelAcciones;
    public PanelAcciones panelScript;
    void Update()
    {
        if (leafeonTurn)
        { 
            gameObject.tag = "Aliado";
            AuraL.SetActive(true);
        }
        else
        {
            gameObject.tag = "Untagged";
            AuraL.SetActive(false);
            AuraL.GetComponent <SpriteRenderer>().sortingOrder = 1;
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
