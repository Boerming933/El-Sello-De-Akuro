using System.Collections.Generic;
using UnityEngine;

public class ListaDeBotonesEscena : MonoBehaviour
{
    [Header("Lista de botones ya existentes en la escena")]
    public List<GameObject> botonesEnEscena = new List<GameObject>();

    void Start()
    {
        foreach (GameObject boton in botonesEnEscena)
        {
            if (boton != null)
            {
                boton.SetActive(true); // O cualquier lógica que necesites
            }
        }
    }
}
