using UnityEngine;
using TMPro;



[RequireComponent(typeof(RatingTextPresenter))]
public class LiveAccuracyTextUI : MonoBehaviour
{
    [System.Serializable]
    public struct AccuracyGradientTier
    {
        [Range(0f, 1f)]
        public float accuracyThreshold;

        public UnityEngine.Gradient gradient;
        public bool animateGradient;
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI accuracyText;

    [Header("Formatting")]
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "%";

    [Header("Accuracy Gradient Tiers")]
    [SerializeField] private AccuracyGradientTier[] tiers;

    private RatingTextPresenter presenter;
    private int currentTierIndex = -1;

    private void Awake()
    {
        presenter = GetComponent<RatingTextPresenter>();
    }

    private void Update()
    {
        var rm = RoundManager.Instance;
        if (rm == null)
        {
            accuracyText.text = "--%";
            return;
        }

        float accuracy = rm.roundStats.CurrentLevelAccuracy;
        float percent = accuracy * 100f;

        accuracyText.text = $"{prefix}{percent:0}{suffix}";

        int tierIndex = ResolveTierIndex(accuracy);

        if (tierIndex != currentTierIndex)
        {
            currentTierIndex = tierIndex;
            ApplyTier(tiers[tierIndex]);
        }
    }

    // ------------------------------------------------------
    // Tier resolution
    // ------------------------------------------------------

    private int ResolveTierIndex(float accuracy)
    {
        int best = 0;
        float bestThreshold = -1f;

        for (int i = 0; i < tiers.Length; i++)
        {
            if (accuracy >= tiers[i].accuracyThreshold &&
                tiers[i].accuracyThreshold > bestThreshold)
            {
                best = i;
                bestThreshold = tiers[i].accuracyThreshold;
            }
        }

        return best;
    }

    private void ApplyTier(AccuracyGradientTier tier)
    {
        presenter.ShowRating(new RatingDisplayData
        {
            ratingText = accuracyText.text, // text already set
            gradient = tier.gradient,
            animateGradient = tier.animateGradient,
            ratingSound = null,
            effect = null
        });
    }
}
