using System.Collections.Generic;

[System.Serializable]
public struct KeyPromptVisibleState
{
    public GameState targetState;
    public List<KeyPromptType> keyPrompts;
}
