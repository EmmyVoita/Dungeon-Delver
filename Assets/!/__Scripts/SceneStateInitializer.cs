using UnityEngine;

public class SceneStateInitializer : MonoBehaviour
{
    [SerializeField]
    private GameState initialState;

    private void Start()
    {
        GameStateManager.Instance
            ?.SetStateForceUpdate(initialState);
    }
}