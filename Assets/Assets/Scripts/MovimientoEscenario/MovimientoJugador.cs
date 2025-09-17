using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [SerializeField] private float speed;

    void Start()
    {
        GetComponent<Transform>();
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

        if (input.magnitude > 1f)
        input.Normalize();
        transform.Translate(input * speed * Time.deltaTime);
    }
}
