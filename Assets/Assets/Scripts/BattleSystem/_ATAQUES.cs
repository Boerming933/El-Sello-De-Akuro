using System.Collections.Generic;
using UnityEngine;

public class ATAQUES : MonoBehaviour
{
    public GameObject PanelBatalla;
    public GameObject PanelAcciones;

    public void PRUEBA()
    {
        Debug.Log("PRUEBA");
        PanelBatalla.SetActive(false);
        PanelAcciones.SetActive(false);
    }


    //Vale Rubia, si lees esto eres puto
    //Mentira
    //Basicamente aca vas a crear una funcion (publica) para CADA ATAQUE, es decir, si cada pj tiene 4 ataques, serian 12 ataques, y por lo tanto 12 funciones
    //cosa que sea facil asignar cada funcion a su boton respectivo.

    //Para esto te tienes que ir a las propiedades del boton, y en la seccion OnClick() le asignas el objeto que tiene este script
    //y en la lista desplegable seleccionas la funcion que quieras.


    //Al final de cada función deberas desactivar el panel de batalla y el panel de acciones con las siguientes lineas:
    //PanelBatalla.SetActive(false);
    //PanelAcciones.SetActive(false);

    //Mucho muy importante, al final de cada funcion (o mas bien al final de la animacion de ataque) debes reactivar el turno del pj

    //Tambien hay que ver que al final de cada funcion, se desactive el botón de batalla del Panel Acciones por el resto del turno (para que solo pueda atacar 1vez por turno)
    //Esto ultimo seguramente lo haga yo, pero si te sale hacerlo, de una.
}
