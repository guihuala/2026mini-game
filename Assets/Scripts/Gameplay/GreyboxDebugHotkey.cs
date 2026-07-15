using UnityEngine;

public sealed class GreyboxDebugHotkey : MonoBehaviour
{
    [SerializeField] private PlayerMotor player;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F1) || UIManager.Instance == null) return;
        if (UIManager.Instance.IsPanelOpen("GreyboxDebugPanel"))
        {
            UIManager.Instance.ClosePanel("GreyboxDebugPanel");
            return;
        }
        GreyboxDebugPanel panel = UIManager.Instance.OpenPanel("GreyboxDebugPanel", null, UIPanelLayer.Top) as GreyboxDebugPanel;
        if (panel != null) panel.Configure(player != null ? player : FindObjectOfType<PlayerMotor>());
    }
}
