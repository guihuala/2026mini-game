using UnityEngine;
using UnityEngine.UI;

public sealed class GreyboxHudPanel : BasePanel
{
    [SerializeField] private Text promptText;
    [SerializeField] private GameObject promptRoot;

    public void SetPrompt(string value)
    {
        bool visible = !string.IsNullOrEmpty(value);
        if (promptRoot != null) promptRoot.SetActive(visible);
        if (promptText != null) promptText.text = value ?? string.Empty;
    }
}
