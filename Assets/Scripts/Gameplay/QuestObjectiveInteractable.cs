using UnityEngine;

public sealed class QuestObjectiveInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "E  完成目标";
    [SerializeField] private QuestObjectiveType objectiveType = QuestObjectiveType.Flag;
    [SerializeField] private string targetId;
    [Min(0.01f)] [SerializeField] private float amount = 1f;
    [SerializeField] private string completionEffects;
    [SerializeField] private string requiredCondition;
    [SerializeField] private bool consumeOnInteract;
    [SerializeField] private string completedPrompt = "已完成";

    public bool CanInteract(PlayerInteractor interactor) => DialogueRuntimeState.EvaluateCondition(requiredCondition)
        && !IsCompletedOrConsumed();

    public string GetInteractionPrompt(PlayerInteractor interactor)
    {
        return IsCompletedOrConsumed() ? completedPrompt : prompt;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;
        ApplyContribution(objectiveType, targetId, amount, completionEffects);
        if (consumeOnInteract) gameObject.SetActive(false);
    }

    public void Configure(string interactionPrompt, QuestObjectiveType type, string id,
        float contribution = 1f, bool consume = false, string effects = null, string condition = null)
    {
        prompt = interactionPrompt;
        objectiveType = type;
        targetId = id;
        amount = Mathf.Max(0.01f, contribution);
        consumeOnInteract = consume;
        completionEffects = effects;
        requiredCondition = condition;
    }

    public static void ApplyContribution(QuestObjectiveType type, string id, float contribution = 1f,
        string effects = null)
    {
        if (string.IsNullOrEmpty(id)) return;
        switch (type)
        {
            case QuestObjectiveType.CollectItem:
                DialogueRuntimeState.SetItem(id, true);
                break;
            case QuestObjectiveType.Number:
                DialogueRuntimeState.SetNumber(id,
                    DialogueRuntimeState.GetNumber(id) + Mathf.Max(0f, contribution));
                break;
            default:
                DialogueRuntimeState.SetFlag(id, true);
                break;
        }
        DialogueRuntimeState.ApplyEffects(effects);
    }

    private bool IsCompletedOrConsumed()
    {
        if (objectiveType == QuestObjectiveType.Number) return false;
        if (objectiveType == QuestObjectiveType.CollectItem) return DialogueRuntimeState.HasItem(targetId);
        return DialogueRuntimeState.HasFlag(targetId);
    }
}
