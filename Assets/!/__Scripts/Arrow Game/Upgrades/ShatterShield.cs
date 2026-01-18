using System.Collections;
using TMPro;
using UnityEngine;

public class GlassShieldEffect : UpgradeEffectBase
{
    //public int healAmount = 2;
    public int spawnCount = 2;
    public float waitTime = 0.3f;
    private bool used = false;
    public Color colorOverride = Color.magenta;

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
        Debug.Log("Glass Shield Triggered");
        StartCoroutine(SpawnArrowsInCircle());
        ConsumableUIAnimator.Instance.PlayUseAnimation(iconReference.GetComponent<UnityEngine.UI.Image>());
        Destroy(iconReference.gameObject);
        used = true;
    }

    private IEnumerator SpawnArrowsInCircle()
    {
        int offset = Random.Range(0, 8); // Random offset to vary the starting angle
        for (int i = 0; i < spawnCount; i++)
        {
            float angle = (i + offset) * 45f; // 360 degrees / 8 arrows = 45 degrees apart
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            // Assuming speed is 5 and type 1 is the glass arrow
            //ArrowSpawner.Instance.SpawnArrow(direction, 5f, 0, colorOverride);

            yield return new WaitForSeconds(waitTime);
        }
    }

    public override void Apply(Player player)
    {
        used = false;
        //player.health = Mathf.Min(player.maxHealth, player.health + healAmount);
        //Destroy(this); // only needed once
    }
}

