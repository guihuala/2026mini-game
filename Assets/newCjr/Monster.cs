using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    
    private MonsterMovement movement;
    [Tooltip("留空时使用 GameManager 上配置的默认抓捕大图。")]
    [SerializeField] private Sprite caughtImageOverride;
    private bool hasCaughtPlayer;

    private void Awake()
    {
        movement = GetComponent<MonsterMovement>();
        GetComponent<Rigidbody2D>().gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnTouchPlayer();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            OnTouchPlayer();
        }
    }

    private void OnTouchPlayer()
    {
        if (hasCaughtPlayer) return;
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        hasCaughtPlayer = true;
        Debug.Log($"怪物 {name} 碰到了玩家！");

        foreach (MonsterMovement monster in FindObjectsOfType<MonsterMovement>(true))
            monster.Pause();

        GameManager.Instance.PlayMonsterCaughtSequence(caughtImageOverride);
    }
}
