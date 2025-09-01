using UnityEngine;

public class TurnoLeafeon : MonoBehaviour
{
    public bool leafeonTurn = false;
    public GameObject AuraL;
    void Update()
    {
        if (leafeonTurn)
        { 
            gameObject.tag = "Aliado";
            AuraL.SetActive(true);
        }
        else
        {
            gameObject.tag = "Untagged";
            AuraL.SetActive(false);
            AuraL.GetComponent <SpriteRenderer>().sortingOrder = 1;
        }
    }
}
