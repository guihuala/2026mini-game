using UnityEngine;

public sealed class GreyboxHud : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    private GUIStyle title;
    private GUIStyle prompt;

    private void Awake()
    {
        title = new GUIStyle { fontSize = 18, normal = { textColor = Color.white } };
        prompt = new GUIStyle { fontSize = 22, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
    }

    private void OnGUI()
    {
        if (interactor == null) interactor = FindObjectOfType<PlayerInteractor>();
        GUI.Box(new Rect(16, 16, 350, 74), "");
        GUI.Label(new Rect(28, 25, 330, 28), "Paper Diorama · 第一阶段灰盒", title);
        GUI.Label(new Rect(28, 53, 330, 24), "WASD 移动 / E 或 F 交互 / Esc 暂停", title);
        string text = interactor != null ? interactor.CurrentPrompt : string.Empty;
        if (!string.IsNullOrEmpty(text) && (DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying))
            GUI.Box(new Rect(Screen.width * .5f - 210, Screen.height - 90, 420, 50), text, prompt);
    }
}
