using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;

public enum MoleState { Hidden, Early, Good, Late }

public class MoleObject : MonoBehaviour
{
    public float dirtFadeInOutTime = 1.0f;
    public ParticleSystem popEffect;
    public float hideDelay = 1.0f;
    public Sprite baseSprite;
    public Sprite activeSprite;
    public Sprite hitSprite;
    public Sprite outlineSprite;
    public SpriteRenderer dirtSprite;

    public AudioClip hitSound;
    public AudioClip badHitSound;
    public float popUpTime = 0.15f;
    public float earlyDuration = 0.25f;
    public float goodDuration = 0.2f;
    public float lateDuration = 0.25f;

    public Color earlyColor = Color.yellow;
    public Color goodColor = Color.green;
    public Color lateColor = new Color(1f, 0.4f, 0.4f);

    public Action<MoleObject, MoleState> onHitCallback;

    private SpriteRenderer sRend;
    private Collider2D col;

    public MoleState currentState = MoleState.Hidden;

    public int slotIndex;
    public Action<int> onHiddenCallback;

    private Tween wiggleTween;
    private Coroutine moleRoutine;

    private SpriteRenderer outlineRenderer;
    private Tween outlineTween;
    public float outlineScale = 1.08f;
    public float wiggleAmount = 10.0f;
    public float wiggleDuration = 0.08f;
    private bool canInteract = false;
    public float interactDelay = 0.4f;


    void OnEnable()
    {
        MoleObstacleManager.DestroyMolesEvent += DestroyMole;
    }

    void OnDisable()
    {
        MoleObstacleManager.DestroyMolesEvent -= DestroyMole;
    }

    void DestroyMole()
    {
        col.enabled = false;
        sRend.color = Color.clear;

        if(moleRoutine != null)
            StopCoroutine(moleRoutine);

        if(popEffect != null)
        {
            popEffect.transform.position = transform.position;
            popEffect.Play();
        }

        Destroy(gameObject, 1.0f);
    }

    void Awake()
    {
        dirtSprite.color = Color.clear;

        StartCoroutine(SetCanInteract());
        sRend = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        col.enabled = false;
        sRend.color = Color.clear;

        // Create outline child
        GameObject outlineObj = new GameObject("OutlineRenderer");
        outlineObj.transform.SetParent(transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localScale = Vector3.one * outlineScale;

        outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = outlineSprite;
        outlineRenderer.sortingLayerID = sRend.sortingLayerID;
        outlineRenderer.sortingOrder = sRend.sortingOrder - 1;
        outlineRenderer.color = new Color(1, 1, 1, 0); // invisible until Good phase

    }

    private IEnumerator SetCanInteract()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactDelay);
        canInteract = true;
    }

    public void Activate()
    {
        moleRoutine = StartCoroutine(MoleRoutine());
    }

    private void StopWiggle()
    {
        if (wiggleTween != null && wiggleTween.IsActive())
            wiggleTween.Kill();
    }

    private void EnableOutline()
    {
        // start fully visible
        outlineRenderer.color = new Color(1, 1, 1, 1);

    }

    private void DisableOutline()
    {
        outlineTween?.Kill();
        outlineRenderer.DOFade(0f, 0.1f);
    }



    private IEnumerator MoleRoutine()
    {
                // Pop up
        sRend.color = Color.white;
        this.transform.localScale = Vector3.zero;
        this.transform.DOScale(1f, popUpTime).SetEase(Ease.OutBack);

        
        // EARLY
        currentState = MoleState.Early;
        //sRend.DOColor(earlyColor, 0.1f);
        sRend.sprite = baseSprite;

        dirtSprite.color = Color.white;
        //yield return dirtSprite.DOColor(Color.white, dirtFadeInOutTime).WaitForCompletion();

        col.enabled = true;

        yield return new WaitForSeconds(earlyDuration);

        // GOOD
        currentState = MoleState.Good;
        sRend.sprite = activeSprite;

        // enable outline glow
        EnableOutline();

        // Start wiggle (rotation back and forth)
        StopWiggle();

        wiggleTween = sRend.transform
            .DORotate(new Vector3(0, 0, wiggleAmount), wiggleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Run for the good window
        yield return new WaitForSeconds(goodDuration);

        // stop wiggle before going late
        StopWiggle();
        sRend.transform.rotation = Quaternion.identity;

        // disable outline when done
        DisableOutline();


        // LATE
        currentState = MoleState.Late;
        //sRend.DOColor(lateColor, 0.05f);
        sRend.sprite = baseSprite;
        yield return new WaitForSeconds(lateDuration);

       // HIDE
        currentState = MoleState.Hidden;
        col.enabled = false;

        sRend.DOFade(0f, 0.15f);
        yield return new WaitForSeconds(0.15f);

        // notify manager that this slot is now free
        onHiddenCallback?.Invoke(slotIndex);

        yield return dirtSprite.DOColor(Color.clear, dirtFadeInOutTime).WaitForCompletion();

        // destroy mole
        Destroy(gameObject);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (currentState == MoleState.Hidden) return;
        if(!canInteract) return;

        onHitCallback?.Invoke(this, currentState);

        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        if(moleRoutine != null)
            StopCoroutine(moleRoutine);

        col.enabled = false;

         // stop wiggle before going late
        StopWiggle();
        sRend.transform.rotation = Quaternion.identity;
        
        if(currentState == MoleState.Good)
        {
            // Successful hit
            AudioHelpers.PlayClipWithVariation(hitSound, AudioChannel.SFX, Camera.main.transform.position, pitchRange: 0.1f);
            sRend.color = Color.green;
        }
        else
        {
            // Early or Late hit
            AudioHelpers.PlayClipWithVariation(badHitSound, AudioChannel.SFX, Camera.main.transform.position, pitchRange: 0.1f);
            sRend.color = Color.red;
        }


        sRend.sprite = hitSprite;
        yield return new WaitForSeconds(hideDelay);

        sRend.color = Color.clear;

        if(popEffect != null)
        {
            popEffect.transform.position = transform.position;
            popEffect.Play();
        }

        yield return dirtSprite.DOColor(Color.clear, dirtFadeInOutTime).WaitForCompletion();

        yield return new WaitForSeconds(0.5f);

        // Hide the mole immediately
        onHiddenCallback?.Invoke(slotIndex);
        Destroy(gameObject);
    }
}
