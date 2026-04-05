using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class FoodAssemblyChallenge : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private float timeLimit = 10f;


    [Header("References")]
    [SerializeField] private GameObject instructionCanvasPrefab;
    [SerializeField] private GameObject timerRingPrefab;
    [SerializeField] private GameObject boostButton;
    [SerializeField] private GameObject recipeSlotPrefab;
    [SerializeField] private Transform recipeContainer;     // parent object for 3 slots
    [SerializeField] private List<ConveyorBelt> belts;
    

    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip correctCollectSound;

    [Header("Audio Feedback")]
    [SerializeField] private float baseCollectPitch = 1f;
    [SerializeField] private float pitchStep = 0.08f;
    [SerializeField] private float maxCollectPitch = 1.4f;

    private int correctCollectedCount = 0;



    [Header("Recipe UI Layout")]
    [SerializeField] private bool layoutVertical = false;     // false = horizontal, true = vertical
    [SerializeField] private float slotSpacing = 1.2f;        // distance between slot centers
    [SerializeField] private Vector2 recipeSlotUIOffset = new Vector2(-3f, 0f); // applied to entire group
    [SerializeField] private float slotScale = 1.0f;


    [Header("Recipe UI Intro Animation")]
    [SerializeField] private Transform recipeIntroAnchor; // position ABOVE the player
    [SerializeField] private float introSpread = 1.2f;    // how far apart the three start slots are
    [SerializeField] private float moveDuration = 1.0f;   // how long to animate into place
    [SerializeField] private float beltFadeDuration = 0.5f;
    [SerializeField] private float beltFadeInDuration = 0.3f;


    [Header("Instruction Message Settings")]
    [SerializeField] private float timerVerticalOffset = 400f;
    [SerializeField] private string displayMessage = "COLLECT";
    [SerializeField] private float messageDuration = 2.0f;



    private List<RecipeSlotUI> recipeSlots;
    private List<bool> gotSlots = new List<bool>();
    private bool active = false;
    private GameObject timerRingInstance;
    private bool endState = false;


    public List<ConveyorBelt> ConveyorBelts => belts;
   

    void OnEnable()
    {
        ConveyorItem.onItemCollected += CollectItem;
    }

    void OnDisable()
    {
        ConveyorItem.onItemCollected -= CollectItem;
    }


    void Start()
    {
        endState = false;
        recipeSlots = new List<RecipeSlotUI>();
        StartCoroutine(RoundSequence());
    }

    // Actual Round Sequence
    // ---------------------------------------------------------------------------------------------------

    private IEnumerator RoundSequence()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);

        gotSlots = new List<bool>(belts.Count);
        for (int i = 0; i < belts.Count; i++)
            gotSlots.Add(false);


        BuildRecipeUI();

        // Step 1: Fade in recipe slots above player
        yield return StartCoroutine(FadeInRecipeSlotsSequential());

        // Wait a moment before moving the slots to the side
        yield return new WaitForSeconds(1.0f);
        
        // display instruction message
        StartCoroutine(ShowInstructionMessage(displayMessage));

        // Step 2: Animate recipe UI from above player → left side UI
        //yield return StartCoroutine(AnimateRecipeSlotsIntoPlace());

        yield return new WaitForSeconds(1.0f);


        yield return StartCoroutine(ChallengeRoutine());
        
    }


    // Building UI
    // ---------------------------------------------------------------------------------------------------

    private IEnumerator ShowInstructionMessage(string message = null)
    {
        bool finished = false;

        var canvas = Instantiate(instructionCanvasPrefab);
        canvas.GetComponent<InstructionCanvas>()
            .ShowMessage(message ?? displayMessage, messageDuration,() => finished = true);

        // Wait here until canvas calls callback
        while (!finished)
            yield return null;
    }

    private void BuildRecipeUI()
    {
        Vector3 start = recipeIntroAnchor.position;   // anchor above player

        for (int i = 0; i < belts.Count; i++)
        {
            RecipeSlotUI slot = CreateSlot(belts[i].correctItem.sprite);
            recipeSlots.Add(slot);
            
            // Make invisible
            recipeSlots[i].icon.color = new Color(1, 1, 1, 0);

            // Compute horizontal spread
            float t = (belts.Count == 1) ? 0.5f : (float)i / (belts.Count - 1);  
            float xOffset = Mathf.Lerp(-introSpread, introSpread, t);

            // Apply world position
            recipeSlots[i].transform.position = start + new Vector3(xOffset, 0, 0);
        }
    }

    private RecipeSlotUI CreateSlot(Sprite s)
    {
        // create object
        GameObject obj = Instantiate(recipeSlotPrefab, this.transform.position, Quaternion.identity);

        // set hierarchy
        obj.transform.SetParent(recipeContainer, false);
        RecipeSlotUI ui = obj.GetComponent<RecipeSlotUI>();

        SpriteRenderer iconRend = ui.GetComponentInChildren<SpriteRenderer>();
        iconRend.sprite = s;

        return ui;
    }

    private IEnumerator FadeInRecipeSlotsSequential(float fadeDuration = 0.35f, float delayBetween = 0.15f)
    {
        for (int i = 0; i < recipeSlots.Count; i++)
        {
            float pitch = 1 + (i * 0.1f);
            yield return recipeSlots[i].FadeIn(fadeDuration, pitch);
            yield return new WaitForSeconds(delayBetween);
        }
    }

    
    IEnumerator AnimateRecipeSlotsIntoPlace()
    {
        Vector3 offset = (Vector3)recipeSlotUIOffset;

        int count = recipeSlots.Count;

        // Precompute final local positions for each slot
        List<Vector3> finalPositions = new List<Vector3>(count);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos;

            if (!layoutVertical)
            {
                // Horizontal layout (left → right)
                float t = (count == 1) ? 0.5f : (float)i / (count - 1);
                float x = Mathf.Lerp(-slotSpacing, slotSpacing, t);
                pos = offset + new Vector3(x, 0, 0);
            }
            else
            {
                // Vertical layout (top → bottom)
                float t = (count == 1) ? 0.5f : (float)i / (count - 1);
                float y = Mathf.Lerp(slotSpacing, -slotSpacing, t);
                pos = offset + new Vector3(0, y, 0);
            }

            finalPositions.Add(pos);
        }

        // ---- Animate all slots into final positions ----
        for (int i = 0; i < count; i++)
        {
            RecipeSlotUI slot = recipeSlots[i];

            slot.transform.DOLocalMove(finalPositions[i], moveDuration)
                .SetEase(Ease.OutCubic);

            // Slot pop scale animation
            slot.transform.DOScale(1.1f, 0.2f)
                .SetLoops(2, LoopType.Yoyo);
        }

        yield return new WaitForSeconds(moveDuration + 0.1f);
    }

    // Building Timer UI
    // ---------------------------------------------------------------------------------------------------


    private void InstantiateTimer()
    {
        if(timerRingPrefab != null)
        {
            timerRingInstance = Instantiate(timerRingPrefab, transform.position, Quaternion.identity);

            timerRingInstance?.GetComponent<BasicFillBar>().Show(timeLimit,() => 
            {
                if(!endState)
                Fail();
            },
            new Vector2(0, timerVerticalOffset));
        }
    }

    // Actual Round Logic
    // ---------------------------------------------------------------------------------------------------

    IEnumerator ChallengeRoutine()
    {
        boostButton.GetComponent<BeltBoostButton>().FadeInSprite();

        // Fade belts in sequentially
        foreach (ConveyorBelt belt in belts)
        {
            yield return new WaitForSeconds(beltFadeInDuration);
            belt.FadeInSprites(beltFadeDuration);
        }

        yield return new WaitForSeconds(beltFadeDuration);

        InstantiateTimer();

        // Start all belts
        foreach (ConveyorBelt belt in belts)
            belt.Begin();

        active = true;
        float timer = 0f;

        // Loop while time remains AND not all slots are collected
        while (timer < timeLimit && !AllSlotsCollected())
        {
            timer += Time.deltaTime;
            yield return null;
        }

        active = false;

        // Stop belts
        foreach (ConveyorBelt belt in belts)
            belt.Stop();

        yield return new WaitForSeconds(1.0f);

        if (AllSlotsCollected())
            Success();
        else
            Fail();
    }

    private bool AllSlotsCollected()
    {
        for (int i = 0; i < gotSlots.Count; i++)
            if (!gotSlots[i]) return false;

        return true;
    }


    public void CollectItem(string id, bool correct)
    {
        if (!active) return;

        for (int i = 0; i < belts.Count; i++)
        {
            // Skip already collected ones
            if (gotSlots[i]) 
                continue;

            // Is this the correct item?
            if (id == belts[i].correctItem.id)
            {
                gotSlots[i] = true;
                recipeSlots[i].SetFilled();
                belts[i].SlowStop();

                correctCollectedCount++;

                AudioHelpers.PlayMyClipAtPoint(
                    correctCollectSound,
                    AudioChannel.SFX,
                    Camera.main.transform.position,
                    pitch: GetCollectPitch()
                );


                break;
            }
        }
    }

    private float GetCollectPitch()
    {
        float pitch = baseCollectPitch + correctCollectedCount * pitchStep;
        return Mathf.Min(pitch, maxCollectPitch);
    }



    // Success / Fail Logic
    // ---------------------------------------------------------------------------------------------------

    void Success()
    {
        if(endState) return;
        endState = true;
        HandleCleanup();
        AudioHelpers.PlayMyClipAtPoint(successSound, AudioChannel.SFX, Camera.main.transform.position);
        timerRingInstance.GetComponent<BasicFillBar>().HideSmooth();
        ObstacleManager.Instance.UnregisterObstacle(gameObject, 2);
        Destroy(this.gameObject, 3.0f);
    }

 

    void Fail()
    {
        if(endState) return;
        endState = true;
        HandleCleanup();
        AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);
        Player.Instance.DamageSelf(1);
        ObstacleManager.Instance.UnregisterObstacle(gameObject, 2);
        Destroy(this.gameObject, 3.0f);
    }

    void HandleCleanup()
    {
        foreach (ConveyorBelt belt in belts)
        {
            belt.KillAllItems();
            belt.FadeOutSprites();
        }

        foreach (RecipeSlotUI slot in recipeSlots)
        {
            slot.AnimateDisappear();
        }

        if(boostButton != null)
        boostButton.GetComponent<BeltBoostButton>().OnDeath();
    }
}
