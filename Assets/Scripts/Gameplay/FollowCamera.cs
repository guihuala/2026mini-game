using UnityEngine;

public sealed class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothing = 7f;
    private Vector3 offset;
    private Transform explorationTarget;

    private void Start()
    {
        if (target == null)
        {
            var player = FindObjectOfType<PlayerMotor>();
            if (player != null) target = player.transform;
        }
        if (target != null)
        {
            explorationTarget = target;
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position + offset, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));
    }

    public void Focus(Transform focusTarget)
    {
        if (focusTarget != null) target = focusTarget;
    }

    public void RestoreExplorationTarget()
    {
        if (explorationTarget != null) target = explorationTarget;
    }
}
