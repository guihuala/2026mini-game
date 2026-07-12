using UnityEngine;

/// <summary>Turns a pixel card toward the camera. Full facing is useful for orbiting showcase cameras.</summary>
[ExecuteAlways]
public sealed class PixelBillboard : MonoBehaviour
{
    [Tooltip("Keep disabled for scenery standing on a flat ground plane.")]
    public bool followVertical = false;

    void LateUpdate()
    {
        var camera = Camera.main;
        if (camera == null) return;
        var direction = camera.transform.position - transform.position;
        if (!followVertical) direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
