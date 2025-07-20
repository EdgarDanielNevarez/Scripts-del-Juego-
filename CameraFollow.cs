using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    public Transform target;

    [Header("Configuración de la Cámara")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Zona Muerta (Dead Zone)")]
    public Vector2 deadZoneSize;

    [Header("Zoom para Enfocar")]
    public float zoomInSize = 3f;
    public float zoomSpeed = 2f; 

    private Vector3 shakeOffset = Vector3.zero;
    private float originalSize;
    private Camera cam;
    private Coroutine zoomCoroutine;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalSize = cam.orthographicSize; 
    }

    void LateUpdate()
    {
        if (target == null) return;

        float deltaX = target.position.x - transform.position.x;
        float deltaY = target.position.y - transform.position.y;
        float minX = transform.position.x - deadZoneSize.x / 2;
        float maxX = transform.position.x + deadZoneSize.x / 2;
        float minY = transform.position.y - deadZoneSize.y / 2;
        float maxY = transform.position.y + deadZoneSize.y / 2;
        Vector3 targetPosition = transform.position;

        if (target.position.x < minX) targetPosition.x = target.position.x + deadZoneSize.x / 2;
        else if (target.position.x > maxX) targetPosition.x = target.position.x - deadZoneSize.x / 2;

        if (target.position.y < minY) targetPosition.y = target.position.y + deadZoneSize.y / 2;
        else if (target.position.y > maxY) targetPosition.y = target.position.y - deadZoneSize.y / 2;

        targetPosition.z = target.position.z + offset.z;

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed) + shakeOffset;
    }

    public void TriggerShake(Vector2 direction, float magnitude, float duration)
    {
        StartCoroutine(ShakeCoroutine(direction, magnitude, duration));
    }

    private IEnumerator ShakeCoroutine(Vector2 direction, float magnitude, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float currentMagnitude = Mathf.Lerp(magnitude, 0, elapsed / duration);
            shakeOffset = direction * currentMagnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }

    public void SetZoom(bool zoomIn)
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        float targetSize = zoomIn ? zoomInSize : originalSize;
        zoomCoroutine = StartCoroutine(ZoomCoroutine(targetSize));
    }

    private IEnumerator ZoomCoroutine(float targetSize)
    {
        float t = 0;
        float startingSize = cam.orthographicSize;
        while (t < 1)
        {
            t += Time.deltaTime * zoomSpeed;
            cam.orthographicSize = Mathf.Lerp(startingSize, targetSize, t);
            yield return null;
        }
        cam.orthographicSize = targetSize;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(deadZoneSize.x, deadZoneSize.y, 1));
    }
}

