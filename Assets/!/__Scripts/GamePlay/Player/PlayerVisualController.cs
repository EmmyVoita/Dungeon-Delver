using UnityEngine;


public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Directional Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private Vector2 _lastDirection = Vector2.zero;

    private void Update()
    {
        if (Player.Instance == null)
            return;

        Vector2 direction = Player.Instance.LastFacingDirection;

        if (direction == _lastDirection)
            return;

        _lastDirection = direction;

        UpdateSprite(direction);
    }

    private void UpdateSprite(Vector2 direction)
    {
        // Pick whichever axis has the larger magnitude.
        // This also works if the player ever uses 8-direction input.
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.sprite =
                direction.x > 0
                    ? rightSprite
                    : leftSprite;
        }
        else
        {
            spriteRenderer.sprite =
                direction.y > 0
                    ? upSprite
                    : downSprite;
        }
    }
}

/*
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
*/
