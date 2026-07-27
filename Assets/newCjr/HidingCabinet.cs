using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HidingCabinet : MonoBehaviour
{
    private PlayerHidingState hiddenPlayer;
    private HideoutVisionMask visionMask;
    private readonly HashSet<Collider2D> playerContacts = new HashSet<Collider2D>();

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerContacts.Add(other);
        GameObject playerObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;
        hiddenPlayer = playerObject.GetComponent<PlayerHidingState>();
        if (hiddenPlayer == null)
            hiddenPlayer = playerObject.AddComponent<PlayerHidingState>();

        hiddenPlayer.SetHidden(this, true);
        EnsureVisionMask().SetVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hiddenPlayer == null) return;
        playerContacts.Remove(other);
        if (playerContacts.Count > 0) return;

        hiddenPlayer.SetHidden(this, false);
        EnsureVisionMask().SetVisible(false);
        hiddenPlayer = null;
    }

    private HideoutVisionMask EnsureVisionMask()
    {
        if (visionMask != null) return visionMask;
        GameObject maskObject = new GameObject("Cabinet Vision Mask");
        visionMask = maskObject.AddComponent<HideoutVisionMask>();
        return visionMask;
    }
}
