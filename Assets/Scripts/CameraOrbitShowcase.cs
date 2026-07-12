using UnityEngine;

/// <summary>Presentation orbit for showing the real 3D depth behind the 2.5D scene.</summary>
public sealed class CameraOrbitShowcase : MonoBehaviour
{
    public Transform target;
    [Header("Automatic showcase")]
    public bool autoRotate = true;
    public float autoRotateSpeed = 8f;
    [Header("Interaction")]
    public float dragSensitivity = 4f;
    public float zoomSensitivity = 1.2f;
    public float minPitch = 18f;
    public float maxPitch = 62f;
    public float minDistance = 10f;
    public float maxDistance = 28f;

    float yaw;
    float pitch;
    float distance;

    void Start()
    {
        if (target == null) return;
        var offset = transform.position - target.position;
        distance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
        var angles = Quaternion.LookRotation(-offset.normalized).eulerAngles;
        yaw = angles.y;
        pitch = NormalizeAngle(angles.x);
        ApplyOrbit();
    }

    void LateUpdate()
    {
        if (target == null) return;
        bool dragging = Input.GetMouseButton(0);
        if (dragging)
        {
            yaw += Input.GetAxis("Mouse X") * dragSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * dragSensitivity;
        }
        else if (autoRotate)
        {
            yaw += autoRotateSpeed * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSensitivity, minDistance, maxDistance);
        ApplyOrbit();
    }

    void ApplyOrbit()
    {
        var rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target.position - rotation * Vector3.forward * distance;
        transform.rotation = rotation;

        var camera = GetComponent<Camera>();
        if (camera != null && camera.orthographic)
            camera.orthographicSize = Mathf.Lerp(5.2f, 9.2f, Mathf.InverseLerp(minDistance, maxDistance, distance));
    }

    static float NormalizeAngle(float angle) => angle > 180f ? angle - 360f : angle;
}
