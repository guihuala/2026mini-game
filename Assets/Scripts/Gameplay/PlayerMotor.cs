using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public sealed class PlayerMotor : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody body;
    private Vector3 desiredVelocity;

    public Vector3 Facing { get; private set; } = Vector3.forward;
    public bool CanMove => Time.timeScale > 0f && !ExplorationControlLock.IsLocked && (DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying);

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        Vector2 input = InputManager.Instance != null ? InputManager.Instance.Move : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (!CanMove) input = Vector2.zero;
        if (input.sqrMagnitude > 1f) input.Normalize();
        desiredVelocity = new Vector3(input.x, 0f, input.y) * moveSpeed;
        if (input.sqrMagnitude > 0.001f) Facing = new Vector3(input.x, 0f, input.y).normalized;
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + desiredVelocity * Time.fixedDeltaTime);
    }

    public void Warp(Vector3 position, Vector3 facing)
    {
        body.position = position;
        transform.position = position;
        if (facing.sqrMagnitude > 0.01f) Facing = new Vector3(facing.x, 0f, facing.z).normalized;
    }
}
