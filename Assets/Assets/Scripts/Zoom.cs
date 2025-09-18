using UnityEngine;

public class Zoom : MonoBehaviour
{
    private Vector3 originalCameraPosition;
    private Vector3 lastMousePosition;
    private bool isZoomedIn = false;
    private bool isReturning = false;

    public float zoomSpeed = 0.6f;
    public float zoomFactor = 1.8f;
    public float minZoom = 1.2f;
    public float maxZoom = 5f;
    public float returnSpeed = 5f;

    public Transform targetToFollow;
    public float followSpeed = 5f;

    public GameObject zoomHintUI;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            MoveCamera();
        }

        HandleZoom();

        if (zoomHintUI != null)
        {
            zoomHintUI.SetActive(targetToFollow == null);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ForceZoomOut();
        }

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

            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

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

    void MoveCamera()
    {
        Vector3 currentMouseScreenPos = Input.mousePosition;

        float zDistance = -cam.transform.position.z;

        Vector3 lastWorldPos = cam.ScreenToWorldPoint(
            new Vector3(lastMousePosition.x, lastMousePosition.y, zDistance)
        );
        Vector3 currentWorldPos = cam.ScreenToWorldPoint(
            new Vector3(currentMouseScreenPos.x, currentMouseScreenPos.y, zDistance)
        );

        Vector3 worldDelta = lastWorldPos - currentWorldPos;
        transform.position += worldDelta;

        lastMousePosition = currentMouseScreenPos;
    }
}