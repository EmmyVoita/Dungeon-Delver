using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoalStateVisual : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite shooterSprite;
    public Sprite lockedShooterSprite;

    [Header("Scale Per State")]
    public Vector3 normalScale = Vector3.one;
    public Vector3 shooterScale = Vector3.one * 1.1f;
    public Vector3 lockedShooterScale = Vector3.one * 0.9f;

    [Header("Scale Transition")]
    public float scaleLerpSpeed = 8f;

    private SpriteRenderer sRend;
    private Vector3 targetScale;

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        targetScale = transform.localScale;
    }

    void OnEnable()
    {
        Player.OnControlStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        Player.OnControlStateChanged -= HandleStateChanged;
    }

    void Start()
    {
        HandleStateChanged(Player.Instance.playerControlState);
        transform.localScale = targetScale;
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

            case Player.PlayerControlState.Shooter:
                sRend.sprite = shooterSprite;
                targetScale = shooterScale;
                break;

            case Player.PlayerControlState.LockedShooter:
                sRend.sprite = lockedShooterSprite;
                targetScale = lockedShooterScale;
                break;
        }
    }
}