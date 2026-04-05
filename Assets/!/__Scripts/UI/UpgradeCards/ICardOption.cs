using UnityEngine;

public interface ICardOption
{
    Sprite Icon { get; }
    string DisplayName { get; }
    string Description { get; }

    void OnSelected();
}
