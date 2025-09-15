using UnityEngine;
using UnityEngine.UI;

public class AttackButtonProxy : MonoBehaviour
{
    [Tooltip("Drag here the AttackData ScriptableObject for this button")]
    public AttackData attackData;

    [Tooltip("Reference to your scene's AttackController")]
    public AttackController attackController;

    [Tooltip("Panel de batalla (opcional) que quieras ocultar al hacer click")]
    public GameObject panelBatalla;

    public GameObject panelBatallaGeneral;

    public Button botonBatalla;

    [Tooltip("Panel de acciones (optional) con el script PanelAcciones")]
    public PanelAcciones panelAcciones;

    public bool botonDesactivado = false;

    /// <summary>
    /// Asignar este método al OnClick() de tu botón en el Inspector.
    /// </summary>
    public void OnClick()
    {
        botonDesactivado = true;
        // 1) Ocultar paneles
        if (panelBatalla != null)
            panelBatalla.SetActive(false);

        if (panelBatallaGeneral != null)
            panelBatallaGeneral.SetActive(false);

        if (panelAcciones != null)
            panelAcciones.Hide();

        if (botonBatalla != null)
        {
            
            //Button boton = botonBatalla.GetComponent<Button>();
            //ColorBlock cb = boton.colors;
            //cb.normalColor = new Color(0.894f, 0.533f, 0.533f, 1);
            //boton.colors = cb;
            botonBatalla.interactable = false;
            //botonBatalla.GetComponent<Button>().enabled = false;
        }

        if (botonDesactivado)
        { 
            botonBatalla.interactable = false;
        }

        // 2) Disparar la lógica de ataque
        if (attackController != null && attackData != null)
            attackController.StartAttack(attackData);

    }
    public void ShowGeneralBattlePanel()
    {
        if (panelBatallaGeneral != null)
            panelBatallaGeneral.SetActive(true);
    }
}
