
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUpgradeIcon : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private string countPrefix = "x";
    
    public void Initialize(Sprite icon, int count)
    {
        this.icon.sprite = icon;
        countText.text = $"{countPrefix}{count}";
    }
}
