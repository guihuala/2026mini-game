public static class QuestObjectiveExamples
{
    public const string BellItemId = "example.item.lost_bell";
    public const string FlowerCountId = "example.flower.count";
    public const string BeaconFlagId = "example.beacon.lit";

    public static void ResetState()
    {
        DialogueRuntimeState.SetItem(BellItemId, false);
        DialogueRuntimeState.SetNumber(FlowerCountId, 0f);
        DialogueRuntimeState.SetFlag(BeaconFlagId, false);
    }
}
