using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Finish : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerReachFinish();
    }

    private bool triggered;

    private void OnPlayerReachFinish()
    {
        if (triggered) return;
        triggered = true;

        foreach (var monster in FindObjectsOfType<MonsterChase>(true))
            monster.Pause();
    }
}
