using System.Collections.Generic;
using UnityEngine;

public class GoalFormController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoalStateVisual goalVisual;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Big Goal")]
    [SerializeField] private PlayerControlState[] allowedStates;
    [SerializeField] private Sprite bigGoalSprite;
    [SerializeField] private Vector3 bigGoalScale = Vector3.one * 1.5f;

    [Header("Colliders")]
    [SerializeField] private Collider2D[] extraColliders;

    private bool isUnlocked; // player owns ability
    private bool isApplied;  // currently active visually
    private HashSet<PlayerControlState> _allowedStates;

    void OnEnable()
    {
        Player.OnControlStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        Player.OnControlStateChanged -= HandleStateChanged;
    }

    void Awake()
    {
        _allowedStates = new HashSet<PlayerControlState>(allowedStates);
        
        foreach (var col in extraColliders)
            col.enabled = false;
    }

    // 🔹 PUBLIC ENTRY POINT
    public void Activate()
    {
        isUnlocked = true;
        TryApply();
    }

    public void Deactivate()
    {
        isUnlocked = false;
        TryApply();
    }

    private void HandleStateChanged(PlayerControlState state)
    {
        TryApply();
    }

    private void TryApply()
    {
        var state = Player.Instance.playerControlState;
        bool shouldApply = isUnlocked && IsStateAllowed(state);

        if (shouldApply && !isApplied)
        {
            ApplyBigForm();
        }
        else if (!shouldApply && isApplied)
        {
            RemoveBigForm();
        }
    }

    private void ApplyBigForm()
    {
        isApplied = true;

        goalVisual.SetOverride(true);
        spriteRenderer.sprite = bigGoalSprite;
        transform.localScale = bigGoalScale;

        foreach (var col in extraColliders)
            col.enabled = true;
    }

    private void RemoveBigForm()
    {
        isApplied = false;

        goalVisual.SetOverride(false);
        goalVisual.RefreshVisual(); // 👈 MUCH better than SendMessage

        foreach (var col in extraColliders)
            col.enabled = false;
    }

    bool IsStateAllowed(PlayerControlState state)
    {
        return _allowedStates.Contains(state);
    }
}