using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class PlayerKeyInventory : MonoBehaviour
{
    [ShowInInspector]
    private HashSet<string> keys = new HashSet<string>();

    public void AddKey(string keyId)
    {
        keys.Add(keyId);
    }

    public bool HasKey(string keyId)
    {
        return keys.Contains(keyId);
    }

    public void RemoveKey(string keyId)
    {
        keys.Remove(keyId);
    }

    [Button("Add Key (Debug)")]
    public void DebugAddKey(string keyId)
    {
        AddKey(keyId);
    }

    public void ClearKeys()
    {
        keys.Clear();
    }
}
