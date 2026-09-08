using UnityEngine;

public class StartPracticeButton : MonoBehaviour
{
    [SerializeField] private PracticeMenuNavigator practiceMenuSelector;

    public void OnClick()
    {
        practiceMenuSelector.OnConfirm();
    }
}