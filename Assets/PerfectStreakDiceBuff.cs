using UnityEngine;
using System.Collections;

public class PerfectStreakBuff : UpgradeEffectBase
{
    [Header("Settings")]
    public bool requireNoDamage = true;
    public float timeModifier = 1.2f;   // e.g. 1.2 = slightly faster, 0.8 = slow-mo

    [Header("Reward")]
    public AudioClip successSound;
    public AudioClip failSound;
    public PlayerRingTimer ringTimerPrefab;

    private bool failed = false;

    // Store previous modifier in case something else was modifying time
    private float oldModifier;
    private Coroutine timeModifierCoroutine;


    // ----------------------------------------------------------
    // APPLY BUFF
    // ----------------------------------------------------------
    public override void Apply(Player target)
    {
        base.Apply(target);
        failed = false;

        Debug.Log("💫 Perfect Streak Challenge Started!");
        Player.OnDamageTaken += OnPlayerDamaged;

        // Start the main challenge routine
        StartCoroutine(ChallengeRoutine());
    }

    // ----------------------------------------------------------
    // CHALLENGE ROUTINE
    // ----------------------------------------------------------
    private IEnumerator ChallengeRoutine()
    {
        // Spawn the circular timer around the player
        PlayerRingTimer ringTimer = Instantiate(ringTimerPrefab, Player.Instance.transform);
        ringTimer.Show(duration);

        // Optionally adjust time during the streak
        //timeModifierCoroutine = StartCoroutine(TemporaryTimeModifier(timeModifier, duration));
        
          // Save the current modifier (not full Time.timeScale)
        oldModifier = TimeManager.Instance.GetCurrentScale();

        Debug.Log($"⏱️ Applying temporary time modifier: {timeModifier} for {duration} seconds.");

        //TimeManager.Instance.SetModifier(timeModifier, 0.25f); // smoothly apply modifier

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // use unscaled time to ignore slowdown
            yield return null;

            if (failed)
            {
                Debug.Log("❌ Perfect Streak failed!");
                AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);
                ringTimer.Hide();
                Remove();
                yield break;
            }
        }

        // If we finish the loop, success!
        Debug.Log("🏆 Perfect Streak challenge completed!");
        

          ringTimer.Hide();
    }

    // ----------------------------------------------------------
    // DAMAGE FAIL CONDITION
    // ----------------------------------------------------------
    private void OnPlayerDamaged(int damage)
    {
        failed = true;
    }

    // ----------------------------------------------------------
    // REMOVE BUFF
    // ----------------------------------------------------------
    public override void Remove()
    {
        Player.OnDamageTaken -= OnPlayerDamaged;
        base.Remove();

        //TimeManager.Instance.SetModifier(1f, 0.5f); // smoothly revert to normal

        if (failed)
        {
            Debug.Log("💀 Perfect Streak failed — applying penalty.");
            AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);
            Player.Instance.Health -= 2; // optional small punishment
        }
        else
        {
            // Optional success reward — double combo
            ComboManager.Instance.AddHit(ComboManager.Instance.GetCurrentComboCount);
            AudioHelpers.PlayMyClipAtPoint(successSound, AudioChannel.SFX, Camera.main.transform.position);
            Debug.Log("✅ Perfect Streak buff ended successfully.");
        }
    }
}
