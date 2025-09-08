using System.Diagnostics;
using UnityEngine;

public class PanelAcciones : MonoBehaviour
{
    public MouseControler mouseController;
    public bool panelActive = false;

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
        if (panelActive)
        {
            gameObject.SetActive(true);
            //panelActive = false;
        }
        else
        {
            Hide();
        }

        if (mouseController.showPanelAcciones == true)
        {
            Show();
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
