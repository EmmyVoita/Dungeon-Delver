using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Golden Value Wave")]
public class IntermediateGoldenWave : UpgradeBase
{
    public override string GetFormattedDescription()
    {
        return descriptionTemplate;
    }

    public override void Apply()
    {
        /*
        StatModifierManager.Instance.AddTemporaryModifier(
            new GoldenWaveListener()
        );
        */
    }
}
