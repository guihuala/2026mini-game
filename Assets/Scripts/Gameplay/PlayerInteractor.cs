using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float radius = 1.45f;
    [SerializeField] private LayerMask interactionMask = ~0;
    private readonly Collider[] hits = new Collider[24];
    private IInteractable current;

    public string CurrentPrompt => current != null ? current.GetInteractionPrompt(this) : string.Empty;

    private void Update()
    {
        if (ExplorationControlLock.IsLocked) { current = null; return; }
        current = FindBest();
        if (current == null || DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;
        bool pressed = InputManager.Instance != null
            ? InputManager.Instance.GetActionDown(InputActionType.Interact)
            : Input.GetKeyDown(KeyCode.E);
        if (pressed && current.CanInteract(this)) current.Interact(this);
    }

    private IInteractable FindBest()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hits, interactionMask, QueryTriggerInteraction.Collide);
        IInteractable best = null;
        float bestScore = float.MaxValue;
        var seen = new HashSet<IInteractable>();
        for (int i = 0; i < count; i++)
        {
            var candidate = hits[i].GetComponentInParent<IInteractable>();
            if (candidate == null || !seen.Add(candidate) || !candidate.CanInteract(this)) continue;
            float score = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (score < bestScore) { best = candidate; bestScore = score; }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
