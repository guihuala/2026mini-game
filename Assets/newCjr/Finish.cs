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

    private void OnPlayerReachFinish()
    {
        foreach (var monster in FindObjectsOfType<MonsterChase>())
            monster.Pause();
    }
}
