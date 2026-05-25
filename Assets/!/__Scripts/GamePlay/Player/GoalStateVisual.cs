using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoalStateVisual : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite basicJumpSprite;
    public Sprite shooterSprite;
    public Sprite lockedShooterSprite;
    public Sprite laneDodgerSprite;

    [Header("Scale Per State")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 basicJumpScale = Vector3.one;
    public Vector3 shooterScale = Vector3.one * 1.1f;
    public Vector3 lockedShooterScale = Vector3.one * 0.9f;
    public Vector3 laneDodgerScale = Vector3.one;

    [Header("Scale Transition")]
    public float scaleLerpSpeed = 8f;

    private SpriteRenderer sRend;
    private Vector3 targetScale;
    private bool isOverridden = false;

    public void SetOverride(bool active)
    {
        isOverridden = active;
    }

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        targetScale = transform.localScale;
    }

    void OnEnable()
    {
        Player.OnControlStateChanged += HandleStateChanged;
        GameStateManager.OnStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        Player.OnControlStateChanged -= HandleStateChanged;
        GameStateManager.OnStateChanged -= HandleGameStateChanged;
    }

    void Start()
    {
        HandleStateChanged(Player.Instance.playerControlState);
        transform.localScale = targetScale;
    }

    public void RefreshVisual()
    {
        HandleStateChanged(Player.Instance.playerControlState);
    }

    void Update()
    {
        // Smoothly interpolate toward target scale
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleLerpSpeed
        );
    }

    private void HandleStateChanged(Player.PlayerControlState state)
    {
        switch (state)
        {
            case Player.PlayerControlState.Normal:
                sRend.sprite = normalSprite;
                targetScale = normalScale;
                break;

            case Player.PlayerControlState.BasicJump:
                sRend.sprite = basicJumpSprite;
                targetScale = basicJumpScale;
                break;

            case Player.PlayerControlState.Shooter:
                sRend.sprite = shooterSprite;
                targetScale = shooterScale;
                break;

            case Player.PlayerControlState.LockedShooter:
                sRend.sprite = lockedShooterSprite;
                targetScale = lockedShooterScale;
                break;

            case Player.PlayerControlState.LaneDodger:
                sRend.sprite = laneDodgerSprite;
                targetScale = laneDodgerScale;
                break;

            default:
                sRend.sprite = normalSprite;
                targetScale = normalScale;
                Debug.LogError("When handling state change in goal state visual, the state was found to be null.");
            break;
        }
    }

    private void HandleGameStateChanged(GameState previousState, GameState newState)
    {
        if (isOverridden) return;

        if(newState == GameState.DeathSequence && previousState != newState)
        {
            sRend.DOColor(Color.clear,0.3f);
        }
    }
}