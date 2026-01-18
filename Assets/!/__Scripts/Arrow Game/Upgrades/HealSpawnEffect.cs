using UnityEngine;

public class HealSpawnEffect : UpgradeEffectBase
{
    [Header("Heal Spawn Settings")]
    public ArrowBase heartPrefab;
    [Range(0f, 100f)] public float chancePercent = 20f; // % chance to trigger on arrow catch

    void OnEnable()
    {
        //ArrowBase.OnArrowDeath += OnArrowCaught;
    }

    void OnDisable()
    {
        //ArrowBase.OnArrowDeath -= OnArrowCaught;
    }

    void OnArrowCaught()
    {
        float roll = Random.Range(0f, 100f);
        if (roll > chancePercent) return; // fail the roll

        Debug.Log($"Heal Spawn Triggered (rolled {roll:F1} <= {chancePercent:F1}%)");

        // Example: spawn a heart that flies away or heals player
        //ArrowBase heart = Instantiate(heartPrefab, Player.Instance.transform.position, Quaternion.identity);
        //heart.Fire(dir, speed);

        // Optionally heal immediately too:
        // Player.Instance.health = Mathf.Min(Player.Instance.maxHealth, Player.Instance.health + 1);
    }

    public override void Apply(Player player)
    {
        // Optionally modify chancePercent if stacking upgrades
    }
}

