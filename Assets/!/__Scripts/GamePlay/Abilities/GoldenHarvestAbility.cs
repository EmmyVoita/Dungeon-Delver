using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

public class GoldenHarvestAbility : AbilityBase
{
    [Header("Golden Harvest Settings")]
    public AudioClip harvestDestroySound;
    public AudioClip harvestEndSound;
    public AudioClip harvestEndSound2;
    public float activeDuration = 3f;     // duration of counting window
    public float goldenArrowMultiplier = 1.5f;
    public int maxStoredArrows = 20;      // safety cap
    public float releaseInterval = 0.1f;

    public PlayerRingTimer ringTimerPrefab;
    public GameObject polarEffectPrefab;
    public VisualEffect harvestHitEffect;
    public VisualEffect harvestFinishEffect;
    public VisualEffect releaseEffect;

    private int caughtCount = 0;
    private bool isActive = false;

    void OnEnable()
    {
        ArrowBase.OnArrowResolved += HandleArrowScored;
        //ArrowBase.OnArrowHitGlobal += HandleArrowScored;
        //releaseEffect.Stop();
        //harvestHitEffect.Stop();
        //harvestFinishEffect.Stop();
    }

    void Awake()
    {
        //releaseEffect.Stop();
        //harvestHitEffect.Stop();
        //harvestFinishEffect.Stop();
    }

    void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowScored;
        //ArrowBase.OnArrowHitGlobal -= HandleArrowScored;
    }

    public override void Activate(Quaternion rotation)
    {
        if (isActive) return;
        //AudioHelpers.PlaySoundEffect(activateSound, Player.Instance.transform.position);
        StartCoroutine(GoldenHarvestRoutine());
    }

    private IEnumerator GoldenHarvestRoutine()
    {
        var effect = Instantiate(polarEffectPrefab, Player.Instance.transform.position, Quaternion.identity, Player.Instance.transform);
        
        var group = effect.GetComponent<WheelAnimatorGroup>();

        if (group != null)
        {
            group.OpenAll();
        }
        else
        {
            Debug.LogWarning("WheelAnimatorGroup missing on polarEffectPrefab!");
        }

        isActive = true;
        caughtCount = 0;

        // Start visual effect
        Player.Instance.goal.GetComponentInChildren<Goal>().EnterHarvestMode();

        // Optional: spawn VFX or sound to indicate start
        Debug.Log("🌾 Golden Harvest Activated!");

        yield return new WaitForSeconds(activeDuration);

        isActive = false;

        // Give buff equal to caughtCount
        int finalAmount = Mathf.Clamp(caughtCount, 0, maxStoredArrows);

        Debug.Log($"🌟 Golden Harvest ended, converting {finalAmount} arrows to gold.");

        Player.Instance.goal.GetComponentInChildren<Goal>().ExitHarvestMode();

        if (group != null)
        {
            group.CloseAll(true);
        }

        AudioHelpers.PlayMyClipAtPoint(harvestEndSound, AudioChannel.SFX, Player.Instance.transform.position);
        AudioHelpers.PlayMyClipAtPoint(harvestEndSound2, AudioChannel.SFX, Player.Instance.transform.position);



        if (finalAmount > 0)
        {
            BuffHelpers.GetOrCreateGoldenEffect(
                finalAmount
            );

            if (harvestFinishEffect != null)
            {
                harvestFinishEffect.SendEvent("OnPlay");
            }

            // Handle release effect
            yield return StartCoroutine(HandleReleaseEffect());
        }
    }
    
    private IEnumerator HandleReleaseEffect()
    {
        int finalAmount = Mathf.Clamp(caughtCount, 0, maxStoredArrows);
        for (int i = 0; i < finalAmount; i++)
        {
            if (releaseEffect != null)
            {
                releaseEffect.SendEvent("OnPlay");
            }
            yield return new WaitForSeconds(releaseInterval);
        }

        yield return null;
    }

    private void HandleArrowScored(ArrowResolvedData data)
    {
        if (!isActive) return;

        // Bounce the wheels on each catch
        var group = Player.Instance.GetComponentInChildren<WheelAnimatorGroup>();
        if (group != null)
            group.TriggerImpactBounce(0.3f, 12f); // tweak strength/speed to taste

        // Pitch scaling sound
        float pitch = 1f + (caughtCount * 0.05f);
        pitch = Mathf.Clamp(pitch, 1f, 1.45f);

        if (harvestDestroySound != null)
        {
            /*
            AudioHelpers.PlayClipWithVariation(
                harvestDestroySound,
                AudioChannel.SFX,
                Camera.Main.transform.position,
                pitch,
                0f,
                1f
            );
            */
        }


        // Hit effect
        if (harvestHitEffect != null)
            harvestHitEffect.Play();

        // Count arrow
        if (data.goalType == Goal.GoalType.Normal || data.goalType == Goal.GoalType.Critical)
            caughtCount++;
    }


}
