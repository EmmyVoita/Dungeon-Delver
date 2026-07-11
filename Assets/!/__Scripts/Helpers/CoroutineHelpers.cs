using System;
using System.Collections;
using UnityEngine;

public static class CoroutineHelpers
{
    public static IEnumerator WaitUntilOrTimeout(
        string name,
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
                Debug.LogError($"Wait condition timed out after {timeout} seconds. Name => {name}");
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
            //Debug.Log("Input recieved in coroutine helper function waitforconfirm");

            if (GameStateManager.Instance.CurrentState == requiredState)
                confirmed = true;
            else
            {
                //Debug.Log("Input recieved but not right state");
            }
        }

        InputBindingManager.OnConfirmPressed += Handler;

        // Wait until confirmed
        yield return new WaitUntil(() => confirmed);

        //Debug.LogError("Wait until condition met");

        // Always clean up
        InputBindingManager.OnConfirmPressed -= Handler;
    }

    public static IEnumerator WaitForJump(GameState requiredState)
    {
        bool confirmed = false;

        void Handler()
        {
            if (GameStateManager.Instance.CurrentState == requiredState)
                confirmed = true;
        }

        InputBindingManager.OnJumpPressed += Handler;

        // Wait until confirmed
        yield return new WaitUntil(() => confirmed);

        // Always clean up
        InputBindingManager.OnJumpPressed -= Handler;
    }
}
