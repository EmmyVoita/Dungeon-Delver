using UnityEngine;

public class LifeShieldEffect : UpgradeEffectBase
{
    public int healAmount = 4;
    private bool used = false;

    void OnEnable()
    {
        // Subscribe to the door opened event
        Player.OnDamageTaken += TriggerItem;
    }
    void OnDisable()
    {
        // Unsubscribe from the event to prevent memory leaks
        Player.OnDamageTaken -= TriggerItem;
    }

    private void TriggerItem(int currentHealth)
    {
        if (used) return;

        if (currentHealth <= 0)
        {
            Player.Instance.Health = Mathf.Min(Player.Instance.MaxHealth, Player.Instance.Health + healAmount);
            ConsumableUIAnimator.Instance.PlayUseAnimation(iconReference.GetComponent<UnityEngine.UI.Image>());
            //Player.Instance.CastShockwave();
            Destroy(iconReference.gameObject);
            used = true;
        }  
    }

    public override void Apply(Player player)
    {
        used = false;
    }
}
