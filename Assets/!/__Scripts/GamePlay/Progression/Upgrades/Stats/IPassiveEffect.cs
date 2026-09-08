public interface IPassiveEffect : IRuntimeModifier
{
    bool ShouldStart();
    void Start();
    void Stop();
    void Update();
}
