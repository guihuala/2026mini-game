public static class DemoQuestChain
{
    public const string QuestId = "demo.gate_repair";
    public const string NotStarted = "not_started";
    public const string CollectFlowers = "collect_flowers";
    public const string Calibrate = "calibrate";
    public const string ReportBack = "report_back";
    public const string Completed = "completed";
    public const string GateOpenedFlag = "greybox.gate_open";
    public const string MinigameCompletedFlag = "greybox.minigame_complete";
    public const string EndingUnlockedFlag = "greybox.ending_unlocked";

    public static readonly QuestDefinition Definition = new QuestDefinition(
        QuestId,
        "修复方向校准台",
        new QuestObjectiveDefinition("talk_gatekeeper", "与守门人交谈", QuestObjectiveType.Talk, GateOpenedFlag),
        new QuestObjectiveDefinition("collect_flowers", "收集三朵纸花", QuestObjectiveType.Number,
            QuestObjectiveExamples.FlowerCountId, 3f),
        new QuestObjectiveDefinition("calibrate_station", "完成方向校准", QuestObjectiveType.Minigame, MinigameCompletedFlag),
        new QuestObjectiveDefinition("report_gatekeeper", "回去向守门人复命", QuestObjectiveType.Talk, EndingUnlockedFlag));

    public static string State => DialogueRuntimeState.GetQuestState(QuestId);

    public static void SyncLegacyState()
    {
        if (!string.IsNullOrEmpty(State)) return;
        if (DialogueRuntimeState.HasFlag(EndingUnlockedFlag)) DialogueRuntimeState.SetQuestState(QuestId, Completed);
        else if (DialogueRuntimeState.HasFlag(MinigameCompletedFlag)) DialogueRuntimeState.SetQuestState(QuestId, ReportBack);
        else if (DialogueRuntimeState.HasFlag(GateOpenedFlag))
            DialogueRuntimeState.SetQuestState(QuestId,
                DialogueRuntimeState.GetNumber(QuestObjectiveExamples.FlowerCountId) >= 3f ? Calibrate : CollectFlowers);
        else DialogueRuntimeState.SetQuestState(QuestId, NotStarted);
    }

    public static void Accept()
    {
        Definition.TryComplete("talk_gatekeeper");
        DialogueRuntimeState.SetQuestState(QuestId, CollectFlowers);
    }

    public static void CompleteCalibration()
    {
        SyncProgress();
        Definition.TryComplete("calibrate_station");
        DialogueRuntimeState.SetQuestState(QuestId, ReportBack);
    }

    public static void Complete()
    {
        Definition.TryComplete("report_gatekeeper");
        DialogueRuntimeState.SetQuestState(QuestId, Completed);
    }

    public static string GetObjectiveText()
    {
        SyncLegacyState();
        SyncProgress();
        switch (State)
        {
            case CollectFlowers:
                return $"委托 2/3  收集纸花 {DialogueRuntimeState.GetNumber(QuestObjectiveExamples.FlowerCountId):0}/3";
            case Calibrate: return "委托 2/3  把纸花带到校准台并完成校准";
            case ReportBack: return "委托 3/3  回去向守门人复命";
            case Completed: return "委托完成  前往出口";
            default: return "委托 1/3  与守门人交谈";
        }
    }

    public static void SyncProgress()
    {
        if (State == CollectFlowers && DialogueRuntimeState.GetNumber(QuestObjectiveExamples.FlowerCountId) >= 3f)
            DialogueRuntimeState.SetQuestState(QuestId, Calibrate);
    }
}
