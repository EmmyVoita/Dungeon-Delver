using System.Collections;
using UnityEngine;

public class PerfectFinish : UpgradeEffectBase
{
    public float bpmBonus = 10f;
    public int healthAmount = 1;
    private bool used = false;
    [SerializeField] private AudioClip itemActivationSound;

    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleRoundEnd;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleRoundEnd;
    }

    private void HandleRoundEnd(GameState previousState, GameState newState)
    {
        if(newState != GameState.ItemActivations) return;
        
        // Only trigger if the player hit every arrow
        if (RoundManager.Instance.roundStats.RoundAccuracy == 1f)
        {
            RoundManager.Instance.StartCoroutine(ActivateSequence());
        }
    }

    private IEnumerator ActivateSequence()
    {
        // 👁️ Show big item popup animation
        Sprite itemSprite = iconReference?.GetComponent<UnityEngine.UI.Image>()?.sprite;
        if (itemSprite != null)
        {
            ItemActivationManager.Instance.EnqueueItemActivation(itemSprite, itemActivationSound);
        }

        // You can optionally wait until this item has finished displaying 
        // before continuing by waiting for the manager to finish its queue.
        yield return new WaitUntil(() => !ItemActivationManager.Instance.IsActive);

        Debug.Log("💫 Perfect Finish Activated!");
        Player.Instance.IncreaseMaxHealth(healthAmount);
    }

    public override void Apply(Player player)
    {
        //RoundManager.Instance.ApplyTempBPMBonus(bpmBonus);
    }
}
