using UnityEngine;

public sealed class GreyboxPauseInput : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.Instance == null || !InputManager.Instance.GetActionDown(InputActionType.Pause)) return;
        if (GameManager.Instance == null) return;
        if (Time.timeScale > 0f) GameManager.Instance.PauseGame();
        else GameManager.Instance.ResumeGame();
    }
}
