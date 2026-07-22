using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestObjectiveType
{
    Talk,
    Flag,
    CollectItem,
    Number,
    Minigame
}

[Serializable]
public sealed class QuestObjectiveDefinition
{
    [SerializeField] private string objectiveId;
    [SerializeField] private string description;
    [SerializeField] private QuestObjectiveType type;
    [SerializeField] private string targetId;
    [Min(1f)] [SerializeField] private float requiredAmount = 1f;
    [SerializeField] private string completionEffects;

    public string ObjectiveId => objectiveId;
    public string Description => description;
    public QuestObjectiveType Type => type;
    public string TargetId => targetId;
    public float RequiredAmount => requiredAmount;

    public QuestObjectiveDefinition() { }

    public QuestObjectiveDefinition(string id, string text, QuestObjectiveType objectiveType,
        string target, float amount = 1f, string effects = null)
    {
        objectiveId = id;
        description = text;
        type = objectiveType;
        targetId = target;
        requiredAmount = Mathf.Max(1f, amount);
        completionEffects = effects;
    }

    public bool IsCompleted()
    {
        switch (type)
        {
            case QuestObjectiveType.CollectItem:
                return DialogueRuntimeState.HasItem(targetId);
            case QuestObjectiveType.Number:
                return DialogueRuntimeState.GetNumber(targetId) >= requiredAmount;
            default:
                return DialogueRuntimeState.HasFlag(targetId);
        }
    }

    public void Complete()
    {
        switch (type)
        {
            case QuestObjectiveType.CollectItem:
                DialogueRuntimeState.SetItem(targetId, true);
                break;
            case QuestObjectiveType.Number:
                DialogueRuntimeState.SetNumber(targetId,
                    Mathf.Max(requiredAmount, DialogueRuntimeState.GetNumber(targetId)));
                break;
            default:
                DialogueRuntimeState.SetFlag(targetId, true);
                break;
        }

        DialogueRuntimeState.ApplyEffects(completionEffects);
    }

    public string GetProgressText()
    {
        if (type != QuestObjectiveType.Number) return IsCompleted() ? "1/1" : "0/1";
        float current = Mathf.Min(requiredAmount, DialogueRuntimeState.GetNumber(targetId));
        return $"{current:0}/{requiredAmount:0}";
    }
}

[Serializable]
public sealed class QuestDefinition
{
    [SerializeField] private string questId;
    [SerializeField] private string title;
    [SerializeField] private List<QuestObjectiveDefinition> objectives = new List<QuestObjectiveDefinition>();

    public string QuestId => questId;
    public string Title => title;
    public IReadOnlyList<QuestObjectiveDefinition> Objectives => objectives;
    public bool IsCompleted => CurrentObjectiveIndex >= objectives.Count;

    public int CurrentObjectiveIndex
    {
        get
        {
            for (int i = 0; i < objectives.Count; i++)
                if (!objectives[i].IsCompleted()) return i;
            return objectives.Count;
        }
    }

    public QuestObjectiveDefinition CurrentObjective => IsCompleted ? null : objectives[CurrentObjectiveIndex];

    public QuestDefinition() { }

    public QuestDefinition(string id, string questTitle, params QuestObjectiveDefinition[] steps)
    {
        questId = id;
        title = questTitle;
        objectives = steps != null ? new List<QuestObjectiveDefinition>(steps) : new List<QuestObjectiveDefinition>();
    }

    public bool TryComplete(string objectiveId)
    {
        QuestObjectiveDefinition current = CurrentObjective;
        if (current == null || current.ObjectiveId != objectiveId) return false;
        current.Complete();
        DialogueRuntimeState.SetQuestState(questId, IsCompleted ? "completed" : $"objective_{CurrentObjectiveIndex + 1}");
        return true;
    }
}
