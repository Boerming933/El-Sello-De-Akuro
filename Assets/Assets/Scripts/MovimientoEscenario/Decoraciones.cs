using UnityEngine;

public class Decoraciones : MonoBehaviour
{
    public GameObject player;
    public float yDiff;

    void Update()
    {
        if (player.transform.position.y > transform.position.y + yDiff)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -3f);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        }
    }
}
