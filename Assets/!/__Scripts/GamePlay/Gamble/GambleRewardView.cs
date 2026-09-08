using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GambleRewardView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private TextMeshProUGUI label;

    public GambleReward Reward { get; private set;}

    public void Setup(GambleReward reward, float width)
    {
        Reward = reward;
        backgroundRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);

        icon.sprite = reward.icon;
        label.text = reward.amount == 0 ? "" : reward.amount.ToString();
        backgroundImage.color = reward.backgroundColor;
    }
}