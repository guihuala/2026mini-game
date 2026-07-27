using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterTrigger : MonoBehaviour
{
    [SerializeField] private MonsterChase monster;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHidingState hidingState = other.GetComponentInParent<PlayerHidingState>();
        if (hidingState != null && hidingState.IsHidden) return;
        if (monster != null)
        {
            CameraShake.Shake(0.2f, 0.1f, 22f);
            monster.StartChase();
        }
    }
}
