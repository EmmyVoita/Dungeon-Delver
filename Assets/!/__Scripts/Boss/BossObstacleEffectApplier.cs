using UnityEngine;

public class BossObstacleEffectApplier : MonoBehaviour
{
    private void OnEnable()
    {
        BossContext.OnEffectEnabled += HandleEffectEnabled;
        ObstacleManager.OnObstacleSpawned += HandleObstacleSpawned;
    }

    private void OnDisable()
    {
        BossContext.OnEffectEnabled -= HandleEffectEnabled;
        ObstacleManager.OnObstacleSpawned -= HandleObstacleSpawned;
    }

    // 1️⃣ Effect turns on → apply to existing obstacles
    private void HandleEffectEnabled(BossEffectType effect)
    {
        if (effect != BossEffectType.ModifyObstacles)
            return;

        foreach (var obstacle in ObstacleManager.Instance.ActiveObstacles)
        {
            ApplyToObstacle(obstacle);
        }
    }

    // 2️⃣ New obstacle spawns → apply if effect is active
    private void HandleObstacleSpawned(GameObject obstacle)
    {
        if (!BossContext.HasEffect(BossEffectType.ModifyObstacles))
            return;

        ApplyToObstacle(obstacle);
    }

    private void ApplyToObstacle(GameObject obstacle)
    {
        var shooter = obstacle.GetComponentsInChildren<WallShooter>();
        if (shooter != null)
        {
            foreach (var s in shooter)
            {
                if(s.LifetimeSetting != WallShooter.LifetimeMode.SelfManaged)
                    s.StartChallenge();
            }
        }
    }
}
