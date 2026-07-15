using UnityEngine;

public sealed class ExplorationSession : MonoBehaviour
{
    [SerializeField] private PlayerMotor player;
    private float playTime;

    private void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerMotor>();
        RestorePlayer();
    }

    private void Update()
    {
        if (Time.timeScale > 0f) playTime += Time.unscaledDeltaTime;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveNow(null);
    }

    private void OnApplicationQuit() => SaveNow(null);

    public void SaveNow(string nextSpawnPoint)
    {
        SavePlayer(player, nextSpawnPoint);
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.CurrentData.playTimeSeconds += playTime;
        playTime = 0f;
        SaveManager.Instance.SaveGame();
    }

    public static void SavePlayer(PlayerMotor motor, string nextSpawnPoint)
    {
        if (motor == null || SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
        ExplorationSaveData data = SaveManager.Instance.CurrentData.exploration ?? new ExplorationSaveData();
        Vector3 position = motor.transform.position;
        data.positionX = position.x; data.positionY = position.y; data.positionZ = position.z;
        data.facingX = motor.Facing.x; data.facingZ = motor.Facing.z;
        data.hasPosition = true;
        if (!string.IsNullOrEmpty(nextSpawnPoint)) data.spawnPointId = nextSpawnPoint;
        SaveManager.Instance.CurrentData.exploration = data;
    }

    private void RestorePlayer()
    {
        if (player == null || SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
        ExplorationSaveData data = SaveManager.Instance.CurrentData.exploration;
        if (data == null) return;
        Vector3 position = new Vector3(data.positionX, data.positionY, data.positionZ);
        if (!string.IsNullOrEmpty(data.spawnPointId))
        {
            foreach (var spawn in FindObjectsOfType<ExplorationSpawnPoint>())
                if (spawn.Id == data.spawnPointId) { position = spawn.transform.position; break; }
        }
        if (data.hasPosition || !string.IsNullOrEmpty(data.spawnPointId))
            player.Warp(position, new Vector3(data.facingX, 0f, data.facingZ));
    }
}
