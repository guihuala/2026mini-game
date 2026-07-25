using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text numberText;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image previewImage;
    [SerializeField] private GameObject lockIcon;

    public void Bind(LevelDefinition level, int index, bool unlocked, Action<LevelDefinition> onSelected)
    {
        if (numberText != null) numberText.text = (index + 1).ToString("00");
        if (titleText != null) titleText.text = level.displayName;
        if (descriptionText != null) descriptionText.text = unlocked ? level.description : "尚未解锁";
        if (previewImage != null)
        {
            previewImage.sprite = level.preview;
            previewImage.enabled = level.preview != null;
        }
        if (lockIcon != null) lockIcon.SetActive(!unlocked);
        if (button != null)
        {
            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected?.Invoke(level));
        }
    }
}
