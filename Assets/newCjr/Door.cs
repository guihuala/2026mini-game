using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    [SerializeField] private string keyId;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var inventory = collision.collider.GetComponent<PlayerKeyInventory>();
        if (inventory != null && inventory.HasKey(keyId))
        {
            inventory.RemoveKey(keyId);
            Destroy(gameObject);
        }
    }
}
