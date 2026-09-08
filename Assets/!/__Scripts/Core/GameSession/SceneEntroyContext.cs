public enum SceneEntryType
{
    Default,
    ReturnFromTutorial,
    ReturnFromGame,
    ReturnFromPractice
}

public static class SceneEntryContext
{
    public static SceneEntryType EntryType { get; set; } = SceneEntryType.Default;

    public static void Clear()
    {
        EntryType = SceneEntryType.Default;
    }
}