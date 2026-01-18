using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class BPMMultiplierUI : MonoBehaviour
{
    public TextMeshProUGUI multiplierText;
    public Image iconImage;


    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleUIState;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleUIState;
    }


    private void HandleUIState(GameState previous, GameState newState)
    {
        if (newState == GameState.UpgradeSelection)
        {
            multiplierText.DOColor(Color.clear, 0.3f);
            iconImage.DOColor(Color.clear, 0.3f);
        }

        if(previous == GameState.UpgradeSelection)
        {
            multiplierText.DOColor(Color.white, 0.3f);
            iconImage.DOColor(Color.white, 0.3f);
        }
    }

    private void Update()
    {
        float bpmMultiplier = RoundManager.Instance != null ? RoundManager.Instance.RoundBPMMultiplier : 1f;
        multiplierText.text = $"x{bpmMultiplier:F2}";
    }
}
