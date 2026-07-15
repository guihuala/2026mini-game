using System.Collections;
using UnityEngine;

public sealed class NpcWaypointMover : MonoBehaviour
{
    [SerializeField] private float speed = 2.2f;
    public bool IsMoving { get; private set; }

    public IEnumerator MoveTo(Transform destination)
    {
        if (destination == null) yield break;
        IsMoving = true;
        Vector3 target = destination.position;
        target.y = transform.position.y;
        while ((transform.position - target).sqrMagnitude > .005f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        IsMoving = false;
    }
}
