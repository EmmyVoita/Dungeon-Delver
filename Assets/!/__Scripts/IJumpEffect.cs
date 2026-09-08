using UnityEngine;

public interface IJumpEffect : IRuntimeModifier
{
    bool CanTriggerEffect(JumpData data);
    bool TriggerEffect(JumpData data);
}