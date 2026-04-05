using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreModfiersUI : MonoBehaviour
{
    public TextMeshProUGUI normalModifierText;
    public TextMeshProUGUI critModifierText;
    public TextMeshProUGUI comboModifierText;

    private void OnEnable()
    {
        //GameStateManager.OnStateChanged += UpdateUI;
        UpgradeManager.OnScoreContextChanged += UpdateUI;
    }

    private void OnDisable()
    {
        //GameStateManager.OnStateChanged -= UpdateUI;
        UpgradeManager.OnScoreContextChanged -= UpdateUI;
    }

    private void Awake()
    {
        //UpdateUI();
    }

    private void UpdateUI(GameState previous, GameState current)
    {
        //UpdateUI();
    }

    private void UpdateUI(LiveScoreState state)
    {
        normalModifierText.text = $"x{state.NormalArrowTotalModifier:F2}";
        critModifierText.text = $"x{state.CritArrowTotalModifier:F2}";
        comboModifierText.text = $"x{state.ComboTotalModifier:F2}";

        /*
        if (ScoreRules.Instance != null)
        {
            normalModifierText.text = $"x{ScoreRules.Instance.NormalArrowTotalModifier:F2}";
            critModifierText.text = $"x{ScoreRules.Instance.CritArrowTotalModifier:F2}";
            comboModifierText.text = $"x{ScoreRules.Instance.ComboTotalModifier:F2}";
        }
        */
    }
}