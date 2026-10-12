using UnityEngine;

public class LevelInspectorController : MonoBehaviour
{
    public static LevelInspectorController Instance { get; private set; }

    private LevelEventInspector _currentInspector;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(LevelEventInspector inspector)
    {
        if (_currentInspector != inspector)
        {
            _currentInspector?.Hide();
            _currentInspector = inspector;
        }

        _currentInspector.Show();
    }

    public void HideCurrent()
    {
        _currentInspector?.Hide();
        _currentInspector = null;
    }
}