using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeIconUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;
    [SerializeField] private TMP_Text xText;

    private int _stackCount;

    public void Initialize(Sprite icon, Material material, int stackCount)
    {
        iconImage.sprite = icon;
        iconImage.material = material;

        SetStackCount(stackCount);
    }

    public void SetStackCount(int count)
    {
        _stackCount = count;

        stackText.text = _stackCount.ToString();

        // Optional: hide count when only one stack
        stackText.gameObject.SetActive(_stackCount > 1);
        xText.gameObject.SetActive(_stackCount > 1);
    }

    public void AddStack(int amount = 1)
    {
        SetStackCount(_stackCount + amount);
    }

    public int GetStackCount()
    {
        return _stackCount;
    }
}