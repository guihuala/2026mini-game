using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Pairing")]
    [Tooltip("The portal at the other end. Pair both portals with each other.")]
    [SerializeField] private Portal pairedPortal;
    [Tooltip("Optional exit position. Uses the paired portal's transform when left empty.")]
    [SerializeField] private Transform exitPoint;

    [Header("Teleport")]
    [Min(0f)]
    [SerializeField] private float teleportCooldown = 0.35f;
    [SerializeField] private bool preserveVelocity = true;

    private static readonly Dictionary<int, float> PlayerCooldowns = new Dictionary<int, float>();

    private Collider2D triggerCollider;

    public Portal PairedPortal => pairedPortal;
    public bool IsPaired => pairedPortal != null && pairedPortal != this;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!TryGetPlayer(other, out Transform player, out Rigidbody2D playerBody))
            return;

        int playerId = player.gameObject.GetInstanceID();
        if (!CanTeleport(playerId))
            return;

        Teleport(player, playerBody, playerId);
    }

    private bool CanTeleport(int playerId)
    {
        if (!IsPaired || !pairedPortal.isActiveAndEnabled)
            return false;

        if (PlayerCooldowns.TryGetValue(playerId, out float readyTime))
        {
            if (Time.time < readyTime)
                return false;

            PlayerCooldowns.Remove(playerId);
        }

        return true;
    }

    private void Teleport(Transform player, Rigidbody2D playerBody, int playerId)
    {
        Transform destination = pairedPortal.exitPoint != null
            ? pairedPortal.exitPoint
            : pairedPortal.transform;

        float cooldown = Mathf.Max(teleportCooldown, pairedPortal.teleportCooldown);
        PlayerCooldowns[playerId] = Time.time + cooldown;

        if (playerBody != null)
        {
            Vector2 velocity = playerBody.velocity;
            playerBody.position = destination.position;
            playerBody.velocity = preserveVelocity ? velocity : Vector2.zero;
        }
        else
        {
            player.position = destination.position;
        }

        Physics2D.SyncTransforms();
    }

    private static bool TryGetPlayer(
        Collider2D other,
        out Transform player,
        out Rigidbody2D playerBody)
    {
        playerBody = other.attachedRigidbody;
        player = playerBody != null ? playerBody.transform : other.transform;

        if (!other.CompareTag("Player") && !player.CompareTag("Player"))
            return false;

        return true;
    }

    public void ResetPortal()
    {
        PlayerCooldowns.Clear();
    }

    private void OnValidate()
    {
        Collider2D portalCollider = GetComponent<Collider2D>();
        if (portalCollider != null)
            portalCollider.isTrigger = true;

        if (pairedPortal == this)
            pairedPortal = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!IsPaired)
            return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        Gizmos.DrawLine(transform.position, pairedPortal.transform.position);
    }
}
