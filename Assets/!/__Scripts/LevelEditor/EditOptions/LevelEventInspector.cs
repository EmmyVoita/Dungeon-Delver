using UnityEngine;
using UnityEngine.UI;

public abstract class LevelEventInspector : MonoBehaviour
{
    [SerializeField] protected GameObject panel;

    [Header("Buttons")]
    [SerializeField] protected Button undoButton;
    [SerializeField] protected Button deleteButton;

    public bool IsOpen => panel.activeSelf;

    public virtual void Show()
    {
        panel.SetActive(true);
    }

    public virtual void Hide()
    {
        panel.SetActive(false);
    }
}