using UnityEngine;

public class IntermediateEffectOption : ICardOption
{
    private IntermediateEffectSO effect;

    public IntermediateEffectOption(IntermediateEffectSO effect)
    {
        this.effect = effect;
    }

    public Sprite Icon => effect.icon;
    public string DisplayName => effect.displayName;
    public string Description => effect.GetDescription();

    public void OnSelected()
    {
        effect.Apply();
    }
}
