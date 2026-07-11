using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("State Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite shooterSprite;
    [SerializeField] private Sprite lockedShooterSprite;

    private void OnEnable()
    {
        Player.OnControlStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        Player.OnControlStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        HandleStateChanged(Player.Instance.playerControlState);
    }

    private void HandleStateChanged(PlayerControlState state)
    {
        switch (state)
        {
            case PlayerControlState.Normal:
                spriteRenderer.sprite = normalSprite;
                break;

            case PlayerControlState.Shooter:
                spriteRenderer.sprite = shooterSprite;
                break;

            case PlayerControlState.LockedShooter:
                spriteRenderer.sprite = lockedShooterSprite;
                break;
        }
    }
}
