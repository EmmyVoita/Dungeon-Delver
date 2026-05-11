using UnityEngine;
using UnityEngine.UI;

public class WorldNodeView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private GameObject bossIcon;

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite bossSprite;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color bossColor;

    public int Index { get; private set; }

    public void Initialize(int index, bool isBoss)
    {
        Index = index;
        sRend.color = isBoss ? bossColor : normalColor;
        sRend.sprite = isBoss ? bossSprite : normalSprite;
        bossIcon.SetActive(isBoss ? true : false);
    }
}