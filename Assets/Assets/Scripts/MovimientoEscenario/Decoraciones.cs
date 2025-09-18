using UnityEngine;

public class Decoraciones : MonoBehaviour
{
    public GameObject player;

    void Update()
    {
        if (player.transform.position.y > transform.position.y)
        {
            Vector3 RepositionZ = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
            transform.position = RepositionZ;
        }
        else if (transform.position.z < 0)
        {
            Vector3 RepositionZ = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
            transform.position = RepositionZ;
        }
    }
}
