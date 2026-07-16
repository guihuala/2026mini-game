using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI面板的基类
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected bool hasRemoved = false; // 标记面板是否已被移除
    protected string panelName; // 面板名称
    protected CanvasGroup canvasGroup; // 用于管理透明度和交互

    [Header("动画层级")]
    [Tooltip("只缩放面板内容，不缩放全屏遮罩。留空时会在运行时自动创建。")]
    [SerializeField] protected RectTransform animationContent;
    [Tooltip("独立淡入淡出的全屏遮罩。留空时优先使用面板根节点上的 Graphic。")]
    [SerializeField] protected Graphic backdropGraphic;
    
    [Header("动画设置")]
    [SerializeField] protected float fadeInDuration = 0.5f; // 淡入持续时间
    [SerializeField] protected float fadeOutDuration = 0.5f; // 淡出持续时间
    [SerializeField] protected Ease fadeInEase = Ease.OutQuad; // 淡入缓动类型
    [SerializeField] protected Ease fadeOutEase = Ease.InQuad; // 淡出缓动类型
    [SerializeField] protected bool scaleAnimation = true; // 是否启用缩放动画
    [SerializeField] protected Vector2 scaleFrom = new Vector2(0.8f, 0.8f); // 初始缩放值
    [SerializeField] protected float scaleDuration = 0.3f; // 缩放动画持续时间

    private CanvasGroup contentCanvasGroup;
    private float backdropTargetAlpha;
    private Sequence panelSequence;

    protected virtual void Awake()
    {
        // 获取 CanvasGroup 组件
        canvasGroup = GetComponent<CanvasGroup>();
        BuildAnimationLayers();
    }

    private void BuildAnimationLayers()
    {
        if (backdropGraphic == null)
        {
            backdropGraphic = GetComponent<Graphic>();
        }

        if (animationContent == null)
        {
            int originalChildCount = transform.childCount;
            Transform[] originalChildren = new Transform[originalChildCount];
            for (int i = 0; i < originalChildCount; i++)
            {
                originalChildren[i] = transform.GetChild(i);
            }

            GameObject contentObject = new GameObject("AnimatedContent", typeof(RectTransform), typeof(CanvasGroup));
            animationContent = contentObject.GetComponent<RectTransform>();
            animationContent.SetParent(transform, false);
            animationContent.anchorMin = Vector2.zero;
            animationContent.anchorMax = Vector2.one;
            animationContent.offsetMin = Vector2.zero;
            animationContent.offsetMax = Vector2.zero;
            animationContent.localScale = Vector3.one;

            // Move existing direct children into the animation layer. A separately assigned
            // child backdrop stays outside and therefore never inherits the content scale.
            for (int i = 0; i < originalChildren.Length; i++)
            {
                Transform child = originalChildren[i];
                if (backdropGraphic != null && child == backdropGraphic.transform) continue;
                child.SetParent(animationContent, false);
            }
        }

        contentCanvasGroup = animationContent.GetComponent<CanvasGroup>();
        if (contentCanvasGroup == null)
        {
            contentCanvasGroup = animationContent.gameObject.AddComponent<CanvasGroup>();
        }

        backdropTargetAlpha = backdropGraphic != null ? backdropGraphic.color.a : 0f;
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="name">面板名称</param>
    public virtual void OpenPanel(string name)
    {
        panelName = name;

        // 激活面板
        gameObject.SetActive(true);

        KillPanelAnimation();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        contentCanvasGroup.alpha = 0f;
        if (backdropGraphic != null)
        {
            Color color = backdropGraphic.color;
            color.a = 0f;
            backdropGraphic.color = color;
        }
        
        // 如果启用缩放动画，设置初始缩放值
        if (scaleAnimation)
        {
            animationContent.localScale = scaleFrom;
        }

        // 使用 DOTween 播放渐显动画（不受时间缩放影响）
        panelSequence = DOTween.Sequence();
        panelSequence.SetUpdate(UpdateType.Normal, true); // 设置为不受时间缩放影响

        // 遮罩只淡入，内容独立淡入并缩放。
        if (backdropGraphic != null)
        {
            panelSequence.Join(backdropGraphic.DOFade(backdropTargetAlpha, fadeInDuration)
                .SetEase(fadeInEase)
                .SetUpdate(UpdateType.Normal, true));
        }

        panelSequence.Join(contentCanvasGroup.DOFade(1f, fadeInDuration)
            .SetEase(fadeInEase)
            .SetUpdate(UpdateType.Normal, true));
            
        // 如果启用缩放动画，添加缩放动画
        if (scaleAnimation)
        {
            panelSequence.Join(animationContent.DOScale(Vector3.one, scaleDuration)
                .SetEase(fadeInEase)
                .SetUpdate(UpdateType.Normal, true));
        }


        panelSequence.OnComplete(() =>
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        });
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    public virtual void ClosePanel()
    {
        hasRemoved = true;
        SetInteractable(false);
        KillPanelAnimation();

        // 使用 DOTween 播放渐隐动画，并在动画完成后销毁对象（不受时间缩放影响）
        panelSequence = DOTween.Sequence();
        panelSequence.SetUpdate(UpdateType.Normal, true); // 设置为不受时间缩放影响

        if (backdropGraphic != null)
        {
            panelSequence.Join(backdropGraphic.DOFade(0f, fadeOutDuration)
                .SetEase(fadeOutEase)
                .SetUpdate(UpdateType.Normal, true));
        }

        panelSequence.Join(contentCanvasGroup.DOFade(0f, fadeOutDuration)
            .SetEase(fadeOutEase)
            .SetUpdate(UpdateType.Normal, true));
            
        // 如果启用缩放动画，添加缩放动画
        if (scaleAnimation)
        {
            panelSequence.Join(animationContent.DOScale(scaleFrom, Mathf.Min(fadeOutDuration, scaleDuration))
                .SetEase(fadeOutEase)
                .SetUpdate(UpdateType.Normal, true));
        }
        
        // 动画完成后销毁对象
        panelSequence.OnComplete(() =>
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        });
    }
    
    /// <summary>
    /// 立即关闭面板（无动画）
    /// </summary>
    public virtual void ClosePanelImmediate()
    {
        hasRemoved = true;
        KillPanelAnimation();
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 设置面板交互状态
    /// </summary>
    /// <param name="interactable">是否可交互</param>
    public virtual void SetInteractable(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
    }

    private void KillPanelAnimation()
    {
        if (panelSequence != null && panelSequence.IsActive()) panelSequence.Kill();
        panelSequence = null;
        if (animationContent != null) animationContent.DOKill();
        if (contentCanvasGroup != null) contentCanvasGroup.DOKill();
        if (backdropGraphic != null) backdropGraphic.DOKill();
    }

    protected virtual void OnDestroy()
    {
        KillPanelAnimation();
    }
}
