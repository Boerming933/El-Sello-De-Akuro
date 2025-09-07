using UnityEngine;

public class Turnable : MonoBehaviour
{
    public GameObject aura;
    //public GameObject panelAcciones;
    public PanelAcciones panelScript;

    /// <summary>
    /// Invocar cuando comience el turno de este personaje.
    /// </summary>
    public void ActivateTurn()
    {
        // Etiquetado para interceptar clicks sólo a este
        if (gameObject.tag == "Enemy")
        {
            gameObject.tag = "Enemy";
        }
        else
        {
            gameObject.tag = "Aliado";
        }
        if (aura != null) aura.SetActive(true);
        //if (panelAcciones != null) panelAcciones.SetActive(true);
        if (panelScript != null) panelScript.Show();
    }

    /// <summary>
    /// Invocar cuando termine el turno de este personaje.
    /// </summary>
    public void DeactivateTurn()
    {
        // Quitar tag para que no sea seleccionable
        if (gameObject.tag == "Enemy")
        {
            gameObject.tag = "Enemy";
        }
        else
        {
            gameObject.tag = "Untagged";
        }
        
        if (aura != null)
        {
            aura.SetActive(false);
            aura.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        //if (panelAcciones != null) panelAcciones.SetActive(false);
        if (panelScript != null) panelScript.Hide();
    }
}
