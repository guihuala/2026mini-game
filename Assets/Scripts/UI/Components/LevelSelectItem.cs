using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text numberText;
    [SerializeField] private GameObject lockIcon;

    public void Bind(LevelDefinition level, int index, bool unlocked, Action<LevelDefinition> onSelected)
    {
        if (numberText != null) numberText.text = (index + 1).ToString("00");

        if (lockIcon != null) lockIcon.SetActive(!unlocked);
        if (button != null)
        {
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected?.Invoke(level));
        }
    }
}
