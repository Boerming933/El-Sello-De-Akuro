using UnityEngine;

public class ANTILAG : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Aliado"))
        {
            Destroy(gameObject, 0.05f);
        }
    }
}
