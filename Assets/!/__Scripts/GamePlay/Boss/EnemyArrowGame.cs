using UnityEngine;
using System;

public class EnemyArrowGame : MonoBehaviour
{
    public static EnemyArrowGame Instance { get; private set; }

    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    public static event Action<float> OnEnemyHealthChanged; // normalized health event

    void OnEnable()
    {
        ArrowBase.OnArrowResolved += HandleArrowDeath;
    }

    void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowDeath;
    }

    void HandleArrowDeath(ArrowResolvedData data)
    {
        switch(data.goalType)
        {
            case Goal.GoalType.Normal:
                TakeDamage(1);
                break;
            case Goal.GoalType.Critical:
                TakeDamage(2);
                break;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnEnemyHealthChanged?.Invoke(GetNormalizedHealth());
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnEnemyHealthChanged?.Invoke(GetNormalizedHealth());

        if (currentHealth <= 0)
            Die();
    }

    float GetNormalizedHealth()
    {
        return (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log("💀 Enemy defeated!");
    }

    public void ResetHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth;
        OnEnemyHealthChanged?.Invoke(1f);
    }
}
