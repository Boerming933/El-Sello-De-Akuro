using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Zoom : MonoBehaviour
{
    private Vector3 originalCameraPosition;
    private Vector3 lastMousePosition;
    private bool isZoomedIn = false;
    private bool isReturning = false;
    private bool isFollowingTarget = true;

    public float zoomSpeed = 0.6f;
    public float triggerZoomSpeed = 3f;
    public float zoomFactor = 1.8f;
    public float minZoom = 1.2f;
    public float maxZoom = 5f;
    public float returnSpeed = 5f;

    public float[] CameraLimitX;
    public float[] CameraLimitY;

    private float joystickSensitivity;
    public float sensitivity;

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
        if (Input.GetKeyDown(KeyCode.P)) SceneManager.LoadScene(2);
        
        Vector2 rightStickInput = new Vector2(
        Input.GetAxis("RightStickX"),
        Input.GetAxis("RightStickY")
        );
        joystickSensitivity = 5 * sensitivity;

        if (rightStickInput.magnitude > 0.1f)
        {
            MoveCameraWithJoystick(rightStickInput);
            isFollowingTarget = false;
        }

        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            MoveCamera();
            isFollowingTarget = false; //mueves la c�mara, deja de seguir
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

        //Volver al personaje solo si isFollowingTarget est� activo
        if (isFollowingTarget && targetToFollow != null)
        {
            Vector3 targetPos = new Vector3(targetToFollow.position.x, targetToFollow.position.y, cam.transform.position.z);
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * followSpeed);
            transform.position = ClampCameraPosition(transform.position);
        }

        //Z = seguir al personaje
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            ForceZoomOut();
        }
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;

        float JoystickScroll = Input.GetAxis("L2") - Input.GetAxis("R2");

        if (Mathf.Approximately(scroll, 0f) && Mathf.Approximately(JoystickScroll, 0f))
        {
            return;
        }

        float zoomInput;
        float currentSpeed;

        if (!Mathf.Approximately(scroll, 0f))
        {
            zoomInput = scroll;
            currentSpeed = zoomSpeed;
        }
        else
        {
            zoomInput = JoystickScroll;
            currentSpeed = triggerZoomSpeed;
        }

        if (zoomInput > 0)
        {
            if (!isZoomedIn)
            {
                originalCameraPosition = cam.transform.position;
                isZoomedIn = true;
            }

            isReturning = false;

            isFollowingTarget = false;
        }
        else if (zoomInput < 0)
        {
            if (isZoomedIn && Mathf.Approximately(cam.orthographicSize, maxZoom))
            {
                isZoomedIn = false;
                isReturning = false;
            }
        }
        
        cam.orthographicSize -= zoomInput * currentSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
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

        transform.position = ClampCameraPosition(transform.position);
    }

    void MoveCameraWithJoystick(Vector2 input)
    {
        // Convertir input del stick en desplazamiento en el mundo
        Vector3 right = cam.transform.right;
        Vector3 up = cam.transform.up;

        Vector3 moveDirection = (right * input.x + up * -input.y) * joystickSensitivity * Time.deltaTime;

        transform.position += moveDirection;        
        
        transform.position = ClampCameraPosition(transform.position);

    }
    
    Vector3 ClampCameraPosition(Vector3 pos)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        
        float minX = CameraLimitX[0] + halfWidth;
        float maxX = CameraLimitX[1] - halfWidth;
        float minY = CameraLimitY[0] + halfHeight;
        float maxY = CameraLimitY[1] - halfHeight;

        if (minX > maxX)
        {
            float centerX = (CameraLimitX[0] + CameraLimitX[1]) * 0.5f;
            minX = maxX = centerX;
        }
        if (minY > maxY)
        {
            float centerY = (CameraLimitY[0] + CameraLimitY[1]) * 0.5f;
            minY = maxY = centerY;
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

}