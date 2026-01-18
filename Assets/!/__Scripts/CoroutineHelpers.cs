using System;
using System.Collections;
using UnityEngine;

public static class CoroutineHelpers
{
    public static IEnumerator WaitUntilOrTimeout(
        Func<bool> condition,
        float timeout,
        Action onTimeout = null
    )
    {
        float startTime = Time.time;

        while (!condition())
        {
            if (Time.time - startTime > timeout)
            {
                onTimeout?.Invoke();
                yield break;
            }

            yield return null;
        }
    }

    public static IEnumerator WaitForConfirm(GameState requiredState)
    {
        bool confirmed = false;

        void Handler()
        {
            if (GameStateManager.Instance.CurrentState == requiredState)
                confirmed = true;
        }

        InputBindingManager.OnConfirmPressed += Handler;

        // Wait until confirmed
        yield return new WaitUntil(() => confirmed);

        // Always clean up
        InputBindingManager.OnConfirmPressed -= Handler;
    }
}
