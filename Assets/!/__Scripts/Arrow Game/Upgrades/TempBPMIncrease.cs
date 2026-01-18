using System.Collections;
using UnityEngine;

public class TempBPMIncrease : UpgradeEffectBase
{
    public float bpmBonus = .2f;
    public int healAmount = 4;
    private bool used = false;
    [SerializeField]private AudioClip itemActivationSound;

    void OnEnable()
    {
        GameStateManager.OnStateChanged += HealPlayer;
    }
    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HealPlayer;
    }

    private void HealPlayer(GameState previousState, GameState newState)
    {
        if(newState != GameState.ItemActivations) return;   
        if (used) return;
        RoundManager.Instance.StartCoroutine(ActivateSequence());
    }

    /*
    private void HealPlayer()
    {
        if (used) return;
        // Implement healing logic here
        Player.Instance.health = Mathf.Min(Player.Instance.maxHealth, Player.Instance.health + healAmount);
        ConsumableUIAnimator.Instance.PlayUseAnimation(iconReference.GetComponent<UnityEngine.UI.Image>());
        Destroy(iconReference.gameObject);
        used = true;
    }
    */
    
    private IEnumerator ActivateSequence()
    {
        used = true;

        // 👁️ 1. Big display of item
        Sprite itemSprite = iconReference?.GetComponent<UnityEngine.UI.Image>()?.sprite;
         if (itemSprite != null)
        {
            Debug.Log("💫 Temp BPM Increase Heal EnqueItemActivation");
            ItemActivationManager.Instance.EnqueueItemActivation(itemSprite, itemActivationSound);
        }

        // You can optionally wait until this item has finished displaying 
        // before continuing by waiting for the manager to finish its queue.
        yield return new WaitUntil(() => !ItemActivationManager.Instance.IsActive);

        Debug.Log("💫 Temp BPM Increase Activated!");

        // ❤️ 2. Apply effect (heal)
        Player.Instance.HealPlayer(healAmount);

        // 🎬 3. Play small UI animation
        ConsumableUIAnimator.Instance.PlayUseAnimation(iconReference.GetComponent<UnityEngine.UI.Image>());

        // 💀 4. Cleanup
        Destroy(iconReference.gameObject);

        // 🧩 5. Apply gameplay effects
        //RoundManager.Instance.ApplyTempBPMBonus(bpmBonus);
    }

    public override void Apply(Player player)
    {
        RoundManager.Instance.ApplyTempBPMBonus(bpmBonus);
    }
}
