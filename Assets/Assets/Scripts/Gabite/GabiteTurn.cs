using UnityEngine;

public class GabiteTurn : MonoBehaviour
{
    public bool gabiteTurn = false;
    public GameObject AuraG;
    void Update()
    {
        if (gabiteTurn)
        {
            gameObject.tag = "Enemy";
            AuraG.SetActive(true);
        }
        else
        {
            gameObject.tag = "Untagged";
            AuraG.SetActive(false);
            AuraG.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }
}
