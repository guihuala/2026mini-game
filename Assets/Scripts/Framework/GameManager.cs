using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : SingletonPersistent<GameManager>
{
    public enum GameState
    {
        Playing, // 游戏进行中
        Paused, // 游戏暂停
        GameOver // 游戏结束
    }

    private GameState currentState;
    private bool levelCompleted;
    private bool isCaughtSequencePlaying;
    private CanvasGroup caughtOverlay;
    private Image caughtBackgroundView;
    private Image caughtImageView;
    private Image caughtFlashView;
    private Image caughtBlackView;

    [Header("怪物抓捕演出")]
    [Tooltip("玩家被怪物抓到时全屏显示的大图。")]
    [SerializeField] private Sprite monsterCaughtImage;
    [Tooltip("抓捕后保持纯黑屏的时间。")]
    [Min(0f)]
    [SerializeField] private float caughtBlackHoldDuration = 0.2f;
    [Tooltip("图片从黑屏中逐渐显现的时间。")]
    [Min(0f)]
    [SerializeField] private float caughtImageRevealDuration = 0.8f;
    [Tooltip("图片完全显现后闪烁的次数。")]
    [Min(0)]
    [SerializeField] private int caughtFlashCount = 3;
    [Min(0.02f)]
    [SerializeField] private float caughtFlashInterval = 0.12f;
    [Min(0f)]
    [SerializeField] private float caughtBlackFadeDuration = 0.35f;

    public GameState CurrentState => currentState;
    public bool IsPaused => currentState == GameState.Paused;
    public bool IsCaughtSequencePlaying => isCaughtSequencePlaying;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 游戏开始时初始化状态
    void Start()
    {
        SetGameState(GameState.Playing);
        RefreshHud(SceneManager.GetActiveScene());
    }

    private void Update()
    {
        if (InputManager.Instance != null &&
            InputManager.Instance.GetActionDown(InputActionType.Pause))
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        levelCompleted = false;
        SetGameState(GameState.Playing);
        RefreshHud(scene);
    }

    private void RefreshHud(Scene scene)
    {
        if (UIManager.Instance == null) return;

        bool isLevel = false;
        LevelCatalog catalog = LevelProgress.Catalog;
        if (catalog != null)
        {
            foreach (LevelDefinition level in catalog.Levels)
            {
                if (level != null && level.sceneName == scene.name)
                {
                    isLevel = true;
                    break;
                }
            }
        }

        if (isLevel)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx("底噪", true);
            if (!UIManager.Instance.IsPanelOpen("HUDPanel"))
                UIManager.Instance.OpenPanel("HUDPanel", null, UIPanelLayer.Bottom);
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.StopSfx("底噪");
            ClosePanelIfOpen("HUDPanel");
        }
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                SetPaused(false);
                ClosePanelIfOpen("SettingPanel");
                ClosePanelIfOpen("PausePanel");
                ClosePanelIfOpen("GameResultPanel");
                break;

            case GameState.Paused:
                SetPaused(true);
                if (UIManager.Instance != null) UIManager.Instance.OpenPanel("PausePanel", null, UIPanelLayer.Top);
                break;

            case GameState.GameOver:
                SetPaused(true);
                if (UIManager.Instance != null)
                {
                    GameResultPanel resultPanel =
                        UIManager.Instance.OpenPanel("GameResultPanel", null, UIPanelLayer.Top) as GameResultPanel;
                    if (resultPanel != null) resultPanel.Configure(levelCompleted);
                }
                break;
        }
    }

    #region 状态控制

    // 游戏开始
    public void StartGame()
    {
        SetGameState(GameState.Playing);
    }

    // 暂停游戏
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing) PauseGame();
        else if (currentState == GameState.Paused) ResumeGame();
    }

    // 恢复游戏
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    // 游戏结束
    public void EndGame()
    {
        levelCompleted = false;
        SetGameState(GameState.GameOver);
    }

    /// <summary>
    /// 播放怪物抓捕演出，并在黑屏后重新加载当前关卡。
    /// 使用不受暂停影响的时间，因此 Time.timeScale 为 0 时仍可正常播放。
    /// </summary>
    public void PlayMonsterCaughtSequence(Sprite[] imageOverrides = null)
    {
        if (isCaughtSequencePlaying) return;
        Sprite[] caughtSprites = HasUsableSprite(imageOverrides)
            ? imageOverrides
            : new[] { monsterCaughtImage };
        StartCoroutine(MonsterCaughtRoutine(caughtSprites));
    }

    private IEnumerator MonsterCaughtRoutine(Sprite[] caughtSprites)
    {
        isCaughtSequencePlaying = true;
        levelCompleted = false;
        currentState = GameState.GameOver;
        SetPaused(true);
        ClosePanelIfOpen("GameResultPanel");
        ClosePanelIfOpen("PausePanel");

        // 先留出一小段可见时间展示相机震动，再盖上抓捕遮罩。
        CameraShake.Shake(0.24f, 0.3f, 30f);
        float shakeLeadTime = 0.14f;
        while (shakeLeadTime > 0f)
        {
            shakeLeadTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        EnsureCaughtOverlay();
        caughtOverlay.gameObject.SetActive(true);
        caughtOverlay.alpha = 1f;
        int currentSpriteIndex = FindNextSpriteIndex(caughtSprites, -1);
        caughtImageView.sprite = currentSpriteIndex >= 0 ? caughtSprites[currentSpriteIndex] : null;
        caughtImageView.enabled = caughtImageView.sprite != null;
        caughtImageView.color = new Color(1f, 1f, 1f, 0f);
        caughtFlashView.color = new Color(1f, 1f, 1f, 0f);
        caughtBlackView.color = Color.black;

        // 第一阶段：UI 层立即全黑。
        float elapsed = 0f;
        while (elapsed < caughtBlackHoldDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 第二阶段：黑色遮罩淡出，同时图片从透明逐渐显现。
        elapsed = 0f;
        while (elapsed < caughtImageRevealDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = caughtImageRevealDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / caughtImageRevealDuration);
            caughtImageView.color = new Color(1f, 1f, 1f, progress);
            caughtBlackView.color = new Color(0f, 0f, 0f, 1f - progress);
            yield return null;
        }

        caughtImageView.color = Color.white;
        caughtBlackView.color = Color.clear;

        // 第三阶段：图片明暗交替，并叠加短促白闪。
        for (int i = 0; i < caughtFlashCount; i++)
        {
            caughtImageView.color = new Color(1f, 1f, 1f, 0.2f);
            caughtFlashView.color = new Color(1f, 1f, 1f, 0.28f);
            yield return new WaitForSecondsRealtime(caughtFlashInterval);

            int nextSpriteIndex = FindNextSpriteIndex(caughtSprites, currentSpriteIndex);
            if (nextSpriteIndex >= 0)
            {
                currentSpriteIndex = nextSpriteIndex;
                caughtImageView.sprite = caughtSprites[currentSpriteIndex];
            }

            caughtImageView.color = Color.white;
            caughtFlashView.color = Color.clear;
            yield return new WaitForSecondsRealtime(caughtFlashInterval);
        }

        caughtFlashView.color = new Color(1f, 1f, 1f, 0f);
        float fadeElapsed = 0f;
        while (fadeElapsed < caughtBlackFadeDuration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            float alpha = caughtBlackFadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(fadeElapsed / caughtBlackFadeDuration);
            caughtBlackView.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        caughtBlackView.color = Color.black;
        caughtImageView.enabled = false;

        AsyncOperation reload = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        while (reload != null && !reload.isDone)
            yield return null;

        // 新场景加载完成后让整个 UI 黑屏层淡出。
        fadeElapsed = 0f;
        while (fadeElapsed < caughtBlackFadeDuration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            float alpha = caughtBlackFadeDuration <= 0f
                ? 0f
                : 1f - Mathf.Clamp01(fadeElapsed / caughtBlackFadeDuration);
            caughtOverlay.alpha = alpha;
            yield return null;
        }

        caughtOverlay.gameObject.SetActive(false);
        isCaughtSequencePlaying = false;
    }

    private static bool HasUsableSprite(Sprite[] sprites)
    {
        if (sprites == null) return false;
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null) return true;
        }
        return false;
    }

    private static int FindNextSpriteIndex(Sprite[] sprites, int currentIndex)
    {
        if (sprites == null || sprites.Length == 0) return -1;

        for (int offset = 1; offset <= sprites.Length; offset++)
        {
            int index = (currentIndex + offset) % sprites.Length;
            if (sprites[index] != null) return index;
        }

        return -1;
    }

    private void EnsureCaughtOverlay()
    {
        if (caughtOverlay != null) return;

        GameObject root = new GameObject("Monster Caught Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        caughtOverlay = root.GetComponent<CanvasGroup>();
        caughtOverlay.blocksRaycasts = true;

        caughtBackgroundView = CreateFullscreenImage(root.transform, "Black Background", Color.black);
        caughtImageView = CreateFullscreenImage(root.transform, "Caught Image", Color.white);
        caughtImageView.preserveAspect = true;
        caughtFlashView = CreateFullscreenImage(root.transform, "Flash", Color.clear);
        caughtBlackView = CreateFullscreenImage(root.transform, "Black Cover", Color.black);

        root.SetActive(false);
    }

    private static Image CreateFullscreenImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public void CompleteLevel()
    {
        levelCompleted = true;
        LevelProgress.CompleteCurrentLevel();
        SetGameState(GameState.GameOver);
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        SetGameState(GameState.Playing);
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(GameScene.MainMenu);
    }

    private void ClosePanelIfOpen(string panelName)
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen(panelName))
        {
            UIManager.Instance.ClosePanel(panelName);
        }
    }

    private void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
    }

    #endregion
}
