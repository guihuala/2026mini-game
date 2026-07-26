using UnityEngine;
using VInspector;

/// <summary>
/// 不使用 A*，按 Inspector 中配置的路径点顺序移动。
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Monster))]
public class FixedPathMonster : MonsterMovement
{
    public enum PathMode
    {
        Loop,
        PingPong,
        Once
    }

    [Header("路径")]
    [Tooltip("怪物会依次前往这些路径点。路径点使用世界坐标，可放在场景中的空物体下统一管理。")]
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private PathMode pathMode = PathMode.Loop;

    [Header("移动")]
    [Min(0f)]
    [SerializeField] private float speed = 2f;
    [Min(0.001f)]
    [SerializeField] private float arriveDistance = 0.05f;
    [Min(0f)]
    [SerializeField] private float waitAtPoint;
    [Tooltip("启用后，进入场景便开始沿路径移动。")]
    [SerializeField] private bool playOnStart = true;

    private Rigidbody2D rb;
    private Vector3 spawnPosition;
    private int currentPointIndex;
    private int direction = 1;
    private float waitTimer;
    private bool isPaused = true;

    public override bool IsPaused => isPaused;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        spawnPosition = transform.position;
    }

    private void Start()
    {
        if (playOnStart)
            StartMoving();
    }

    private void FixedUpdate()
    {
        if (IsPaused || pathPoints == null || pathPoints.Length == 0)
            return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            rb.velocity = Vector2.zero;
            return;
        }

        Transform point = pathPoints[currentPointIndex];
        if (point == null)
        {
            AdvanceToNextPoint();
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = point.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            speed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, targetPosition) <= arriveDistance)
        {
            rb.MovePosition(targetPosition);
            waitTimer = waitAtPoint;
            AdvanceToNextPoint();
        }
    }

    [Button("StartPath")]
    public override void StartMoving()
    {
        if (!HasUsablePath())
        {
            Debug.LogWarning($"固定路径怪物 {name} 没有配置有效路径点。", this);
            return;
        }

        isPaused = false;
    }

    [Button("Pause")]
    public override void Pause()
    {
        isPaused = true;
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    [Button("Resume")]
    public override void Resume()
    {
        StartMoving();
    }

    public override void ResetToSpawn()
    {
        isPaused = true;
        currentPointIndex = 0;
        direction = 1;
        waitTimer = 0f;

        rb.velocity = Vector2.zero;
        rb.position = spawnPosition;
        transform.position = spawnPosition;

        if (playOnStart)
            StartMoving();
    }

    private void AdvanceToNextPoint()
    {
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Pause();
            return;
        }

        if (pathPoints.Length == 1)
        {
            if (pathMode == PathMode.Once)
                Pause();
            return;
        }

        switch (pathMode)
        {
            case PathMode.Loop:
                currentPointIndex = (currentPointIndex + 1) % pathPoints.Length;
                break;

            case PathMode.PingPong:
                currentPointIndex += direction;
                if (currentPointIndex >= pathPoints.Length)
                {
                    direction = -1;
                    currentPointIndex = pathPoints.Length - 2;
                }
                else if (currentPointIndex < 0)
                {
                    direction = 1;
                    currentPointIndex = 1;
                }
                break;

            case PathMode.Once:
                if (currentPointIndex >= pathPoints.Length - 1)
                    Pause();
                else
                    currentPointIndex++;
                break;
        }
    }

    private bool HasUsablePath()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return false;

        foreach (Transform point in pathPoints)
        {
            if (point != null)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.9f);
        Vector3? previous = null;

        foreach (Transform point in pathPoints)
        {
            if (point == null)
                continue;

            Gizmos.DrawWireSphere(point.position, 0.15f);
            if (previous.HasValue)
                Gizmos.DrawLine(previous.Value, point.position);
            previous = point.position;
        }

        if (pathMode == PathMode.Loop && previous.HasValue && pathPoints[0] != null)
            Gizmos.DrawLine(previous.Value, pathPoints[0].position);
    }
}
