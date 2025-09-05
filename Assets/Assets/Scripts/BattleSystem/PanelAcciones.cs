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
        mouseController.canMove = true;
        panelActive = false;
        mouseController.showPanelAcciones = false;
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


}
