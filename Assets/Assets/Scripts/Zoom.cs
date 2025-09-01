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
    public float returnSpeed = 3f;

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        Camera cam = Camera.main;

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
        }
        else if (scroll < 0) 
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

            if (isZoomedIn)
            {
                isReturning = true;

                if (Mathf.Approximately(cam.orthographicSize, maxZoom))
                {
                    isZoomedIn = false;
                    isReturning = false;
                    cam.transform.position = originalCameraPosition;
                }
            }
        }

       
        if (isReturning)
        {
            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                originalCameraPosition,
                Time.deltaTime * returnSpeed
            );
        }
    }
}
