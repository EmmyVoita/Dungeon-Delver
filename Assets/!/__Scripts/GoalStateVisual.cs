using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoalStateVisual : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite shooterSprite;
    public Sprite lockedShooterSprite;

    private SpriteRenderer sRend;

    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
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
        // Sync initial state
        HandleStateChanged(Player.Instance.playerControlState);
    }

    private void HandleStateChanged(Player.PlayerControlState state)
    {
        switch (state)
        {
            case Player.PlayerControlState.Normal:
                sRend.sprite = normalSprite;
                break;

            case Player.PlayerControlState.Shooter:
                sRend.sprite = shooterSprite;
                break;

            case Player.PlayerControlState.LockedShooter:
                sRend.sprite = lockedShooterSprite;
                break;
        }
    }
}
