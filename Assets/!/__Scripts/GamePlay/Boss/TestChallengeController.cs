using UnityEngine;

public class TestChallengeController : MonoBehaviour
{
    ChallengeTestMode testMode = ChallengeTestMode.Off;

    private void Start()
    {
        GameStateManager.Instance.SetState(GameState.Practice);
    }
}