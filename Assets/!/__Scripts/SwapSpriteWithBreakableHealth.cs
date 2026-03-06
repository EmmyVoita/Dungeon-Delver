using System.Collections.Generic;
using UnityEngine;

public class SwapSpriteWithBreakableHealth : MonoBehaviour
{
    [System.Serializable]
    public class HealthSprite
    {
        public Sprite sprite;
        public float threshold;
    }

    [SerializeField] private FallingBreakable fallingBreakable;
    [SerializeField] private List<HealthSprite> healthSprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float lastHealth = -1f;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float currentHealth = fallingBreakable.Health;

        // Only update if health changed
        if (Mathf.Approximately(currentHealth, lastHealth))
            return;

        lastHealth = currentHealth;

        UpdateSprite(currentHealth);
    }

    void UpdateSprite(float health)
    {
        if (healthSprites == null || healthSprites.Count == 0)
            return;

        // Assume list is sorted from lowest threshold to highest
        Sprite chosen = healthSprites[healthSprites.Count - 1].sprite;

        foreach (var entry in healthSprites)
        {
            if (health <= entry.threshold)
            {
                chosen = entry.sprite;
                break;
            }
        }

        spriteRenderer.sprite = chosen;
    }
}