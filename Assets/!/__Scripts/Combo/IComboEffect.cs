using UnityEngine;

[System.Serializable]
public abstract class ComboEffect : ScriptableObject
{
    public virtual void Initialize() {}

    public abstract bool ShouldTrigger(int comboCount);

    public abstract void Execute(int comboCount);
}