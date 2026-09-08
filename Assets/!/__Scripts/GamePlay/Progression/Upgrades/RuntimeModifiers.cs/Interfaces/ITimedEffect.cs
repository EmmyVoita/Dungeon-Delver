public interface ITimedEffect : IRuntimeModifier
{
    void Tick(float deltaTime);
}