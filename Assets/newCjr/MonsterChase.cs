using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MonsterChase : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float repathCooldown = 0.5f;

    private Seeker seeker;
    [SerializeField] Rigidbody2D rb;
    private List<Vector3> waypoints = new List<Vector3>();
    [SerializeField] int currentIndex;
    private Coroutine chaseRoutine;

    private void Awake()
    {
        seeker = GetComponent<Seeker>();
        if (seeker == null) seeker = gameObject.AddComponent<Seeker>();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.constraints = RigidbodyConstraints2D.None;
    }

    private void Start()
    {
        if (target != null)
            chaseRoutine = StartCoroutine(ChaseLoop());
    }

    private IEnumerator ChaseLoop()
    {
        while (target != null)
        {
            RequestPath(target.position);

            float timeout = 2f;
            while (waypoints.Count == 0)
            {
                timeout -= Time.deltaTime;
                if (timeout <= 0f) break;
                yield return null;
            }

            if (waypoints.Count > 0)
            {
                yield return MoveAlongPath();
            }
            else
            {
                yield return new WaitForSeconds(repathCooldown);
            }
        }
    }

    private void RequestPath(Vector3 targetPos)
    {
        if (AstarPath.active == null) return;

        targetPos.z = 0;
        Vector3 startPos = transform.position;
        startPos.z = 0;
        seeker.StartPath(startPos, targetPos, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (p.error) return;

        waypoints = new List<Vector3>(p.vectorPath);
        currentIndex = 0;
    }

    private IEnumerator MoveAlongPath()
    {
        currentIndex = 0;

        while (currentIndex < waypoints.Count)
        {
            Vector3 nowtarget = waypoints[currentIndex];
            Vector3 current = transform.position;
            
            current.z = 0;
            nowtarget.z = 0;
            
            
            if (Vector2.Distance(nowtarget, current) < 0.1f)
            {
                currentIndex++;
                if (currentIndex >= waypoints.Count)
                    break;
                nowtarget = waypoints[currentIndex];
                current = transform.position;
            }

            current.z = 0;
            nowtarget.z = 0;

            Vector3 nextPosition = Vector2.MoveTowards(current, nowtarget, speed * Time.fixedDeltaTime);
            nextPosition.z = 0;
            Debug.Log(nextPosition);
            transform.position = nextPosition;

            yield return new WaitForFixedUpdate();
        }

        waypoints.Clear();
        currentIndex = 0;
    }
}
