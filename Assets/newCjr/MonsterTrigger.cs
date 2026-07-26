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
        if (monster != null)
            monster.StartChase();
    }
}
