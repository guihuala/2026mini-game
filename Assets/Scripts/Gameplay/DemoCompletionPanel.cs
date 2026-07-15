using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class DemoCompletionPanel : BasePanel
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button stayButton;
    private Action mainMenuAction;
    private Action stayAction;

    protected override void Awake()
    {
        base.Awake();
        mainMenuButton.onClick.AddListener(() => mainMenuAction?.Invoke());
        stayButton.onClick.AddListener(() => stayAction?.Invoke());
    }

    public void Configure(Action onMainMenu, Action onStay)
    {
        mainMenuAction = onMainMenu;
        stayAction = onStay;
    }
}
