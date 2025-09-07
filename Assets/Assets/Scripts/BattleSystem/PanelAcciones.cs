using UnityEngine;

public class PanelAcciones : MonoBehaviour
{
    public MouseControler mouseController;
    public bool panelActive = false;

    public void Atacar()
    {
        mouseController.canAttack = true;
        panelActive = false;
        mouseController.showPanelAcciones = false;

    }

    public void Moverse()
    {
        mouseController.StartMoveMode();
        panelActive = false;
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
            gameObject.SetActive(false);
        }

        if (mouseController.showPanelAcciones == true)
        {
            gameObject.SetActive(true);
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
