using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnergyBarFullAnimation : MonoBehaviour
{
    public AbilityBar abilityBar; // Reference to your AbilityBar script
    public Image barImage;             // your UI Image
    public Sprite emptySprite;      // default sprite when not full
    public Sprite[] fullSprites;       // two sprites for the animation
    public float frameRate = 4f;       // 4 frames per second
    public float fullThreshold = 0.99f;

    private Coroutine animRoutine;
    private int currentFrame;

    void OnEnable()
    {
        //Player.OnAbilityChargeChanged += SetFill;
    }
    void OnDisable()
    {
        //Player.OnAbilityChargeChanged -= SetFill;
    }

    private void Update()
    {
    }

    public void SetFill(int previousCharge, int amount)
    {
        /*
        // If bar is full, play animation
        if (abilityBar.currentFill >= fullThreshold)
        {
            if (animRoutine == null)
                animRoutine = StartCoroutine(AnimateFullState());
        }
        else
        {
            if (animRoutine != null)
            {
                StopCoroutine(animRoutine);
                animRoutine = null;
            }
            barImage.sprite = emptySprite; // reset to default
        }
        */
    }

    private IEnumerator AnimateFullState()
    {
        while (true)
        {
            currentFrame = (currentFrame + 1) % fullSprites.Length;
            barImage.sprite = fullSprites[currentFrame];
            yield return new WaitForSeconds(1f / frameRate);
        }
    }
}
