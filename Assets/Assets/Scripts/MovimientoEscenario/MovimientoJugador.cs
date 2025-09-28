using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rb;
    Vector3 input;

    private Vector3 position;
    public Zoom zoom;

    void Start()
    {
        GetComponent<Transform>();
        position = transform.position;
    }

    void Update()
    {
        input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

        if (input.magnitude > 1f) input.Normalize();
        transform.Translate(input * speed * Time.deltaTime, Space.World);
        if (position != transform.position)
        {
            zoom.ForceZoomOut();
            Camera.main.orthographicSize = 2.5f;
        }
        position = transform.position;

    }
}
