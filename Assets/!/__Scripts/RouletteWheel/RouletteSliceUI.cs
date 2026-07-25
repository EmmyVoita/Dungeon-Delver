using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteSliceUI : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private TMP_Text amountText;

    public void Initialize(RewardDefinition reward, float angleDegrees, float radialDistance)
    {
        rewardIcon.sprite = reward.Icon;
        rewardIcon.enabled = reward.Icon != null;

        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        rect.localEulerAngles = new Vector3(rect.localEulerAngles.x,rect.localEulerAngles.y, angleDegrees - 90f);

        rect.anchoredPosition = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)) * radialDistance;

        amountText.text = FormatAmount(reward);
    }

    private string FormatAmount(RewardDefinition reward)
    {
        if (reward.Amount > 0)
            return $"{reward.Amount}";

        return reward.Amount.ToString();
    }
}