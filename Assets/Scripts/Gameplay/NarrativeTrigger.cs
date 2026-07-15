using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class NarrativeTrigger : MonoBehaviour
{
    [SerializeField] private NarrativeSequenceController sequence;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerMotor>() != null && sequence != null) sequence.Play();
    }
}
