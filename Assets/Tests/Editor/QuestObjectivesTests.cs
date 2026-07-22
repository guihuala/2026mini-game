using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class QuestObjectivesTests
{
    [SetUp]
    public void SetUp() => DialogueRuntimeState.Reset();

    [TearDown]
    public void TearDown() => DialogueRuntimeState.Reset();

    [Test]
    public void FlagObjective_CompletesTargetAndEffects()
    {
        var objective = new QuestObjectiveDefinition("talk", "Talk", QuestObjectiveType.Talk,
            "npc.talked", 1f, "item:key;add:favor=2");

        objective.Complete();

        Assert.That(objective.IsCompleted(), Is.True);
        Assert.That(DialogueRuntimeState.HasItem("key"), Is.True);
        Assert.That(DialogueRuntimeState.GetNumber("favor"), Is.EqualTo(2f));
    }

    [Test]
    public void CollectItemObjective_UsesInventoryState()
    {
        var objective = new QuestObjectiveDefinition("collect", "Collect", QuestObjectiveType.CollectItem, "item.bell");
        objective.Complete();
        Assert.That(DialogueRuntimeState.HasItem("item.bell"), Is.True);
        Assert.That(objective.GetProgressText(), Is.EqualTo("1/1"));
    }

    [Test]
    public void NumberObjective_RequiresConfiguredAmount()
    {
        var objective = new QuestObjectiveDefinition("gather", "Gather", QuestObjectiveType.Number, "flower.count", 3f);
        DialogueRuntimeState.SetNumber("flower.count", 2f);
        Assert.That(objective.IsCompleted(), Is.False);
        Assert.That(objective.GetProgressText(), Is.EqualTo("2/3"));
        objective.Complete();
        Assert.That(DialogueRuntimeState.GetNumber("flower.count"), Is.EqualTo(3f));
    }

    [Test]
    public void InteractableContributions_UpdateItemNumberAndFlagState()
    {
        QuestObjectiveInteractable.ApplyContribution(QuestObjectiveType.CollectItem, "item.test");
        QuestObjectiveInteractable.ApplyContribution(QuestObjectiveType.Number, "count.test", 2f);
        QuestObjectiveInteractable.ApplyContribution(QuestObjectiveType.Number, "count.test", 1f);
        QuestObjectiveInteractable.ApplyContribution(QuestObjectiveType.Flag, "flag.test");

        Assert.That(DialogueRuntimeState.HasItem("item.test"), Is.True);
        Assert.That(DialogueRuntimeState.GetNumber("count.test"), Is.EqualTo(3f));
        Assert.That(DialogueRuntimeState.HasFlag("flag.test"), Is.True);
    }

    [Test]
    public void Interactable_RespectsRequiredCondition()
    {
        GameObject target = new GameObject("Conditional Objective");
        QuestObjectiveInteractable interactable = target.AddComponent<QuestObjectiveInteractable>();
        interactable.Configure("Collect", QuestObjectiveType.Number, "count.test", 1f, false,
            null, "flag:quest.accepted");

        Assert.That(interactable.CanInteract(null), Is.False);
        DialogueRuntimeState.SetFlag("quest.accepted", true);
        Assert.That(interactable.CanInteract(null), Is.True);
        Object.DestroyImmediate(target);
    }

    [Test]
    public void PaperDioramaScene_ContainsFiveStaticObjectiveExamples()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PaperDiorama.unity", OpenSceneMode.Single);
        GameObject root = GameObject.Find("QUEST OBJECTIVE EXAMPLES");

        Assert.That(root, Is.Not.Null);
        QuestObjectiveInteractable[] interactables = root.GetComponentsInChildren<QuestObjectiveInteractable>();
        Assert.That(interactables.Length, Is.EqualTo(5));
        int connectedFlowers = 0;
        foreach (QuestObjectiveInteractable interactable in interactables)
        {
            var serialized = new UnityEditor.SerializedObject(interactable);
            if (serialized.FindProperty("targetId").stringValue != QuestObjectiveExamples.FlowerCountId) continue;
            connectedFlowers++;
            Assert.That(serialized.FindProperty("requiredCondition").stringValue,
                Is.EqualTo("flag:" + DemoQuestChain.GateOpenedFlag));
        }
        Assert.That(connectedFlowers, Is.EqualTo(3));
    }

    [Test]
    public void QuestDefinition_RejectsOutOfOrderCompletion()
    {
        var quest = new QuestDefinition("quest.test", "Test",
            new QuestObjectiveDefinition("first", "First", QuestObjectiveType.Flag, "step.first"),
            new QuestObjectiveDefinition("second", "Second", QuestObjectiveType.Minigame, "step.second"));

        Assert.That(quest.TryComplete("second"), Is.False);
        Assert.That(quest.TryComplete("first"), Is.True);
        Assert.That(quest.CurrentObjective.ObjectiveId, Is.EqualTo("second"));
        Assert.That(quest.TryComplete("second"), Is.True);
        Assert.That(quest.IsCompleted, Is.True);
        Assert.That(DialogueRuntimeState.GetQuestState("quest.test"), Is.EqualTo("completed"));
    }

    [Test]
    public void DemoQuestChain_CollectsFlowersBeforeCalibration()
    {
        Assert.That(DemoQuestChain.Definition.CurrentObjective.ObjectiveId, Is.EqualTo("talk_gatekeeper"));
        DemoQuestChain.Accept();
        Assert.That(DemoQuestChain.State, Is.EqualTo(DemoQuestChain.CollectFlowers));
        Assert.That(DemoQuestChain.Definition.CurrentObjective.ObjectiveId, Is.EqualTo("collect_flowers"));
        QuestObjectiveInteractable.ApplyContribution(QuestObjectiveType.Number,
            QuestObjectiveExamples.FlowerCountId, 3f);
        DemoQuestChain.SyncProgress();
        Assert.That(DemoQuestChain.State, Is.EqualTo(DemoQuestChain.Calibrate));
        Assert.That(DemoQuestChain.Definition.CurrentObjective.ObjectiveId, Is.EqualTo("calibrate_station"));
        DemoQuestChain.CompleteCalibration();
        Assert.That(DemoQuestChain.Definition.CurrentObjective.ObjectiveId, Is.EqualTo("report_gatekeeper"));
        DemoQuestChain.Complete();
        Assert.That(DemoQuestChain.Definition.IsCompleted, Is.True);
    }

    [Test]
    public void DemoQuestChain_RecoversLegacyFlags()
    {
        DialogueRuntimeState.SetFlag(DemoQuestChain.MinigameCompletedFlag, true);
        DemoQuestChain.SyncLegacyState();
        Assert.That(DemoQuestChain.State, Is.EqualTo(DemoQuestChain.ReportBack));
        Assert.That(DemoQuestChain.GetObjectiveText(), Does.Contain("3/3"));
    }
}
