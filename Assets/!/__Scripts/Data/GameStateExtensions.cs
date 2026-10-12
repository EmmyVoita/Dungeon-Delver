public static class GameStateExtensions
{
    public static bool IsRoundState(this GameState state)
    {
        return state == GameState.RoundActive ||
               state == GameState.Editor ||
               state == GameState.RoundResultsTally ||
               state == GameState.RoundResultsExit;
    }
}