using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Key : MonoBehaviour
{
    [SerializeField] private string keyId;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var inventory = other.GetComponent<PlayerKeyInventory>();
        if (inventory != null)
        {
            inventory.AddKey(keyId);
            Destroy(gameObject);
        }
    }
}
