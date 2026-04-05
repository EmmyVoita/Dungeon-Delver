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

    private void HandleStateChanged(Player.PlayerControlState state)
    {
        switch (state)
        {
            case Player.PlayerControlState.Normal:
                spriteRenderer.sprite = normalSprite;
                break;

            case Player.PlayerControlState.Shooter:
                spriteRenderer.sprite = shooterSprite;
                break;

            case Player.PlayerControlState.LockedShooter:
                spriteRenderer.sprite = lockedShooterSprite;
                break;
        }
    }
}
