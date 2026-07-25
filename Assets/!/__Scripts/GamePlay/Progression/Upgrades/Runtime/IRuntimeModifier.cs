using Unity.VisualScripting;

public interface IRuntimeModifier
{
    int Priority { get; }
    IRuntimeModifier Clone();
    void OnActivate();
    void OnDestroy();
}