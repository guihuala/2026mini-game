using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    
    private MonsterMovement movement;
    [SerializeField] private MiniSceneManager _miniSceneManager;
    private void Awake()
    {
        movement = GetComponent<MonsterMovement>();
        _miniSceneManager = Uitiles_test.Find<MiniSceneManager>();
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
        Debug.Log($"怪物 {name} 碰到了玩家！");
        if (movement != null)
            movement.Pause();
        _miniSceneManager.ResetPlayerPos();
        
    }
}
