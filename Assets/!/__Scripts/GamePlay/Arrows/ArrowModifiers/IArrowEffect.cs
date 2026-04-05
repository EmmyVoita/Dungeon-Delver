public interface IArrowEffect
{
    void ApplyToArrow(ArrowBase arrow);
    bool IsExpired { get; }
}
