using UnityEngine;
using UnityEngine.UI;

public class Turnable : MonoBehaviour
{
    public GameObject aura;
    public Button btnBatalla;
    public Button btnMoverse;
    public GameObject letreroPj;
    //public GameObject panelAcciones;
    public PanelAcciones panelScript;
    public GameObject iconoEspacio;
    public GameObject panelBatalla;


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
        

        

        if (letreroPj != null) letreroPj.SetActive(true);
        if (iconoEspacio != null) iconoEspacio.SetActive(true);
        if (panelBatalla != null) panelBatalla.SetActive(false);

        
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
            //gameObject.tag = "Untagged"; This not
            gameObject.tag = "Aliado2";
        }
        
        if (aura != null)
        {
            aura.SetActive(false);
            aura.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        //if (panelAcciones != null) panelAcciones.SetActive(false);
        if (panelScript != null) panelScript.Hide();
        if (letreroPj != null) letreroPj.SetActive(false);
        //if (iconoEspacio != null) iconoEspacio.SetActive(false);
    }

    private void Update()
    {
        // Si el botón atacar está desactivado, ocultamos el panelBatalla
        if (btnBatalla != null && panelBatalla != null)
        {
            if (!btnBatalla.interactable && panelBatalla.activeSelf)
            {
                panelBatalla.SetActive(false);
            }
        }
    }
}
