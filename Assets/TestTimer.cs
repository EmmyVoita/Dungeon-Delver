using TMPro;
using UnityEngine;

public class TestTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private bool running;
    private float startTime;
    private double startDSP;

    void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        if (!running)
            return;

        float elapsedTime = Time.time - startTime;
        double elapsedDSP = AudioSettings.dspTime - startDSP;

        text.text =
            $"Time: {elapsedTime:0.000}\n" +
            $"DSP:  {elapsedDSP:0.000}";
    }

    private void HandleStateChanged(GameState previous, GameState current)
    {
        if (current == GameState.RoundActive)
        {
            startTime = Time.time;
            startDSP = AudioSettings.dspTime;
            running = true;

            Debug.Log(
                $"⏱ ROUND ACTIVE\n" +
                $"Time.time start = {startTime}\n" +
                $"DSP start       = {startDSP}"
            );
        }
        else if (current == GameState.RoundResultsTally)
        {
            running = false;
        }
    }
}
