public interface IAbilityDurationModifier : IRuntimeModifier
{
    float ModifyDuration(float currentDuration);
}