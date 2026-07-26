using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectPanel : BasePanel
{
    [SerializeField] private Transform levelContainer;
    [SerializeField] private LevelSelectItem levelItemPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Text emptyText;

    private readonly List<LevelSelectItem> _items = new List<LevelSelectItem>();

    protected override void Awake()
    {
        base.Awake();
        if (backButton != null) backButton.onClick.AddListener(Close);
    }

    private void Start()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        foreach (LevelSelectItem item in _items)
            if (item != null) Destroy(item.gameObject);
        _items.Clear();

        LevelCatalog catalog = LevelProgress.Catalog;
        bool valid = catalog != null && levelContainer != null && levelItemPrefab != null;
        if (emptyText != null) emptyText.gameObject.SetActive(!valid || catalog.Levels.Count == 0);
        if (!valid) return;

        for (int i = 0; i < catalog.Levels.Count; i++)
        {
            LevelDefinition level = catalog.Get(i);
            if (level == null) continue;
            LevelSelectItem item = Instantiate(levelItemPrefab, levelContainer);
            item.Bind(level, i, LevelProgress.IsUnlocked(level.id), OnLevelSelected);
            _items.Add(item);
        }
    }

    private void OnLevelSelected(LevelDefinition level)
    {
        if (level != null && SceneLoader.Instance != null)
            SceneLoader.Instance.LoadLevel(level.id);
    }

    private void Close()
    {
        if (UIManager.Instance != null) UIManager.Instance.ClosePanel("LevelSelectPanel");
    }
}
