using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class KeepUISize : MonoBehaviour
{
    public Camera targetCamera;         // La cámara que controla el zoom
    public float baseCameraSize = 5f;   // Tamaño de la cámara al diseño original
    private Vector3 baseScale;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        baseScale = transform.localScale;
    }

    void Update()
    {
        float scaleFactor = targetCamera.orthographicSize / baseCameraSize;
        transform.localScale = baseScale * scaleFactor;
    }
}