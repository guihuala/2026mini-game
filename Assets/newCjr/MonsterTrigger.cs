using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterTrigger : MonoBehaviour
{
    [Tooltip("触发后开始移动的怪物。可选择追逐怪（MonsterChase）或路径怪（FixedPathMonster）。")]
    [SerializeField] private MonsterMovement monster;

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
            monster.StartMoving();
        }
    }
}
