using UnityEngine;

public interface ICardOption
{
    string DisplayName { get; }
    string Description { get; }

    void OnSelected();
}
