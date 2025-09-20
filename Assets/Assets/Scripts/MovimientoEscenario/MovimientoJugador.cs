using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rb;
    Vector3 input;

    void Start()
    {
        GetComponent<Transform>();
    }

    void Update()
    {
        input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

        if (input.magnitude > 1f) input.Normalize();
        transform.Translate(input * speed * Time.deltaTime, Space.World);
    }

}
