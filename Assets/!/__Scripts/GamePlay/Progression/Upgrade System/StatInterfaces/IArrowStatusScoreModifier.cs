public interface IArrowStatusScoreModifier
{
    float ModifyStatusMultiplier(
        ArrowStatus status,
        float currentMultiplier
    );
}
