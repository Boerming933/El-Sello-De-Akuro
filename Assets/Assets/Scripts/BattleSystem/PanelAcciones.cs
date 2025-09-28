using System.Diagnostics;
using UnityEngine;


public class PanelAcciones : MonoBehaviour
{
    public MouseControler mouseController;
    public bool panelActive = false;
    public CharacterInfo ownerCharacter;
    public GameObject panelBatalla;
    public GameObject letreroPj;

    public void Atacar()
    {
        mouseController.canAttack = true;

        if (mouseController.myUnit.Name == "Riku Takeda")
        {
            bool current = mouseController.animatorSamurai.GetBool("idleBatalla");
            mouseController.animatorSamurai.SetBool("idleBatalla", !current);
        }

        if (panelBatalla.gameObject.activeInHierarchy)
        {
            panelBatalla.SetActive(false);
            //letreroPj.SetActive(true);
        }
        else
        {
            panelBatalla.SetActive(true);
            //letreroPj.SetActive(false);
        }
        
    }

    public void Moverse()
    {
        if (mouseController.myUnit.Name == "Riku Takeda")
        {
            mouseController.animatorSamurai.SetBool("idleBatalla", false);
        }

        mouseController.StartMoveMode();
        panelActive = false;
        mouseController.showPanelAcciones = false;
        panelBatalla.SetActive(false);
    }

    public void Update()
    {
        
        // Sólo debo estar visible si:
        //  - este panel pertenece al personaje activo
        //  - Y además el flag mouseController.showPanelAcciones es true
        bool shouldShow = mouseController.CurrentCharacter == ownerCharacter
                       && mouseController.showPanelAcciones;

        // UnityEngine.Debug.Log($"[PanelAcciones] Owner={ownerCharacter.name} | " +
        //       $"Current={mouseController.CurrentCharacter?.name} | " +
        //       $"showPanelAcciones={mouseController.showPanelAcciones} | " +
        //       $"shouldShow={shouldShow}");

        if (shouldShow)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        panelActive = true;
        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        panelActive = false;
    }
    


}
