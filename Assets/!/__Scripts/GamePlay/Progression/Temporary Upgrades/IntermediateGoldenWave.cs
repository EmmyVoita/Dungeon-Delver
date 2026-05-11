using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Golden Value Wave")]
public class IntermediateGoldenWave : UpgradeBase
{
    public override string GetDescription()
    {
        return descriptionTemplate;
    }

    public override void Apply()
    {
        UpgradeManager.Instance.AddTemporaryModifier(
            new GoldenWaveListener()
        );
    }
}
