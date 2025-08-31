using UnityEngine;

public class Turnero : MonoBehaviour
{
    public TurnoVaporeon vaporeon;
    public TurnoUmbreon umbreon;
    public TurnoLeafeon leafeon;

    public int turno = 1;

    private void Update()
    {
        if (turno == 1)
        {
            vaporeon.vaporeonTurn = true;
            umbreon.umbreonTurn = false;
            leafeon.leafeonTurn = false;
        }
        else if (turno == 2)
        {
            vaporeon.vaporeonTurn = false;
            umbreon.umbreonTurn = true;
            leafeon.leafeonTurn = false;
        }
        else if (turno == 3)
        {
            vaporeon.vaporeonTurn = false;
            umbreon.umbreonTurn = false;
            leafeon.leafeonTurn = true;
        }

        if (turno >= 4)    //Pasa a ser 5 cuando se incluya el turno de la IA
        {
            turno = 1;
        }
        
    }
}
