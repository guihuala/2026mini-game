using UnityEngine;

public sealed class FlagGate : MonoBehaviour
{
    [SerializeField] private string requiredFlag = "greybox.gate_open";
    [SerializeField] private GameObject closedVisual;
    private Collider gateCollider;

    private void Awake()
    {
        gateCollider = GetComponent<Collider>();
        if (closedVisual == null) closedVisual = gameObject;
    }

    private void Update()
    {
        bool open = DialogueRuntimeState.HasFlag(requiredFlag);
        if (gateCollider != null) gateCollider.enabled = !open;
        if (closedVisual != gameObject) closedVisual.SetActive(!open);
        else
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = !open;
        }
    }
}
