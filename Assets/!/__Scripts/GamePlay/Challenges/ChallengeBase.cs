using UnityEngine;

public abstract class ChallengeBase : MonoBehaviour
{
    private object _config;
    public object Config => _config;

    [SerializeField] private int priority = 0;
    [SerializeField] private Player.PlayerControlState controlMode = Player.PlayerControlState.BasicJump;

    public Player.PlayerControlState ControlState => controlMode;
    public int Priority => priority;
    public bool IsActive { get; private set; }

    private bool _cleanedUp = false;

    public virtual void Begin(object config = null)
    {
        IsActive = true;
        _config = config;

        ObstacleManager.Instance?.RegisterObstacle(gameObject);
    }

    public virtual void End()
    {
        if (!IsActive) return;

        IsActive = false;

        ObstacleManager.Instance?.UnregisterObstacle(gameObject);
        RunCleanupOnce();
    }

    protected virtual void CleanUp()
    {
    }

    protected virtual void OnDestroy()
    {
        RunCleanupOnce();

        if (IsActive)
        {
            ObstacleManager.Instance?.UnregisterObstacle(gameObject);
        }
    }

    private void RunCleanupOnce()
    {
        if (_cleanedUp) return;
        _cleanedUp = true;
        CleanUp();
    }
}