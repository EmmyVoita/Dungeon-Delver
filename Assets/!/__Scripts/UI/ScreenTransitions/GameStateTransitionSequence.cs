
using System.Collections.Generic;

[System.Serializable]
public class GameStateTransitionSequence
{
    public GameState from;
    public GameState to;
    public List<TransitionStep> steps;
}