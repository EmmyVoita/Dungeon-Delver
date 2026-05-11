
using System.Collections.Generic;

[System.Serializable]
public class MenuStateTransitionSequence
{
    public MenuState from;
    public MenuState to;
    public List<TransitionStep> steps;
}