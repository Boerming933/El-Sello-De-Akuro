using UnityEngine;

public class Zoom : MonoBehaviour
{
    private Vector3 originalCameraPosition;
    private Vector3 lastMousePosition;
    private bool isZoomedIn = false;
    private bool isReturning = false;
    private bool isFollowingTarget = true;

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
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            MoveCamera();
            isFollowingTarget = false; //mueves la cámara, deja de seguir
        }

        HandleZoom();

        if (zoomHintUI != null && targetToFollow != null)
        {
            float dist = Vector2.Distance(
                new Vector2(cam.transform.position.x, cam.transform.position.y),
                new Vector2(targetToFollow.position.x, targetToFollow.position.y)
            );

            zoomHintUI.SetActive(!isFollowingTarget && dist > 0.2f);
        }

        //Volver al personaje solo si isFollowingTarget está activo
        if (isFollowingTarget && targetToFollow != null)
        {
            Vector3 targetPos = new Vector3(targetToFollow.position.x, targetToFollow.position.y, cam.transform.position.z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * followSpeed);
        }

        //Z = seguir al personaje
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ForceZoomOut();
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

            isFollowingTarget = false;
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

    public void ForceZoomOut()
    {
        var currentUnit = GameObject.FindGameObjectWithTag("Aliado");
        if (currentUnit != null)
        {
            targetToFollow = currentUnit.transform;
            isFollowingTarget = true; 
        }
    }

    public void SetTarget(Transform newTarget)
    {
        targetToFollow = newTarget;
        isFollowingTarget = true;
    }

    public void ClearTarget()
    {
        targetToFollow = null;
        isFollowingTarget = false;
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