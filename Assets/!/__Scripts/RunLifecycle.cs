using System;

public static class RunLifecycle
{
    public static event Action OnRunAbandoned;
    public static event Action OnRunRestart;

    public static void AbadonRun()
    {
        OnRunAbandoned?.Invoke();
    }

    public static void RestartRun()
    {
        OnRunRestart?.Invoke();
    }
}