using System.Collections.Generic;
using UnityEngine;

public abstract class ChallengeBase : MonoBehaviour
{
    private object _config;
    public object Config => _config;
    
    [Header("Base Settings")]
    [SerializeField] private bool registerWithManager = true;
    [SerializeField] private int priority = 0;
    [SerializeField] private PlayerControlState controlMode = PlayerControlState.BasicJump;
    [SerializeField] private List<ChallengeBase> childChallenges;

    public PlayerControlState ControlState => controlMode;
    public int Priority => priority;
    public bool IsActive { get; private set; }

    private bool _cleanedUp = false;

    public virtual void Begin(object config = null)
    {
        IsActive = true;
        _config = config;

        if(registerWithManager)
            ObstacleManager.Instance?.RegisterObstacle(gameObject);

        foreach (ChallengeBase child in childChallenges)
        {
            if (child == null)
                continue;

            child.Begin();
        }
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