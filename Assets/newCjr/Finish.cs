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

        foreach (var monster in FindObjectsOfType<MonsterMovement>(true))
            monster.Pause();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            Debug.LogError("到达终点时未找到 GameManager，无法完成关卡。", this);
        }
    }
}
