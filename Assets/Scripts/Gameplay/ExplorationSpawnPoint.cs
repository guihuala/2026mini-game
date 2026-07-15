using UnityEngine;

public sealed class ExplorationSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointId = "Start";
    public string Id => spawnPointId;
}
