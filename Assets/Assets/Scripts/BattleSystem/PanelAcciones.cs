using System.Diagnostics;
using UnityEngine;


public class PanelAcciones : MonoBehaviour
{
    public MouseControler mouseController;
    public bool panelActive = false;
    public CharacterInfo ownerCharacter;
    public GameObject panelBatalla;

    public void Atacar()
    {
        mouseController.canAttack = true;

        if (panelBatalla.gameObject.activeInHierarchy)
        {
            panelBatalla.SetActive(false);
        }
        else
        {
            panelBatalla.SetActive(true);
        }

    }

    public void Moverse()
    {
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

        UnityEngine.Debug.Log($"[PanelAcciones] Owner={ownerCharacter.name} | " +
              $"Current={mouseController.CurrentCharacter?.name} | " +
              $"showPanelAcciones={mouseController.showPanelAcciones} | " +
              $"shouldShow={shouldShow}");

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
