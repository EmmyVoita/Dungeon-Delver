using UnityEngine;

public class GameStateGameOverTest: MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            GameStateManager.Instance.SetState(GameState.GameOver);
        }

        
    }
}