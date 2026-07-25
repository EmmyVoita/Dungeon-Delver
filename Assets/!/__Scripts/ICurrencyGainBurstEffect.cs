public interface ICurrencyGainSequenceEffect : IRuntimeModifier
{
    int RequiredInstances { get; }
    float TimeWindow { get; }

    void OnSequenceCompleted();
}