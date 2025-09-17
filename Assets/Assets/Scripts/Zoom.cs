using UnityEngine;

public class Zoom : MonoBehaviour
{
    private Vector3 originalCameraPosition;
    private bool isZoomedIn = false;
    private bool isReturning = false;

    public float zoomSpeed = 0.6f;
    public float zoomFactor = 1.8f;
    public float minZoom = 1.2f;
    public float maxZoom = 5f;
    public float returnSpeed = 5f;

    public Transform targetToFollow;     // El personaje activo
    public float followSpeed = 5f;

    public GameObject zoomHintUI;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleZoom();

        if (zoomHintUI != null)
        {
            zoomHintUI.SetActive(targetToFollow == null);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ForceZoomOut();
        }

        // Solo seguir al objetivo si está asignado
        if (targetToFollow != null)
        {
            Vector3 targetPos = new Vector3(targetToFollow.position.x, targetToFollow.position.y, cam.transform.position.z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * followSpeed);
        }


    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            if (!isZoomedIn)
            {
                originalCameraPosition = cam.transform.position;
                isZoomedIn = true;
            }

            isReturning = false;

            Vector3 mouseWorldBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

            Vector3 mouseWorldAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 diff = mouseWorldBeforeZoom - mouseWorldAfterZoom;
            cam.transform.position += diff * zoomFactor;

            // Al hacer zoom in, desactivar seguimiento
            targetToFollow = null;
        }
        else if (scroll < 0)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

            if (isZoomedIn && Mathf.Approximately(cam.orthographicSize, maxZoom))
            {
                isZoomedIn = false;
                isReturning = false;
            }
        }
        isReturning = false;
    }

    void ForceZoomOut()
    {
        // Solo restaurar seguimiento, NO cambiar zoom ni posición
        var currentUnit = GameObject.FindGameObjectWithTag("Aliado");
        if (currentUnit != null)
        {
            targetToFollow = currentUnit.transform;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        targetToFollow = newTarget;
    }

    public void ClearTarget()
    {
        targetToFollow = null;
    }
}
