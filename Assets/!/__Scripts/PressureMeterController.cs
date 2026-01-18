using UnityEngine;

public class PressureMeterController : MonoBehaviour
{
    public Color pressureMeterColor = Color.yellow;
    public BasicFillBar bar;

    [Header("Meter Settings")]
    public float drainRate = 0.15f;   // per second
    public float minFill = 0f;
    public float maxFill = 1f;

    private float currentFill = 1f;
    private bool active = false;
    private System.Action onMeterEmpty;

    public void Show(System.Action onEmpty = null, Vector2? positionOverride = null)
    {
        active = true;
        onMeterEmpty = onEmpty;

        currentFill = 1f;
        bar.Show(999f, null, positionOverride, disableTimer: true, pressureMeterColor);
        bar.SetFillInstant(currentFill);
    }

    void Update()
    {
        if (!active) return;

        // Drain over time
        currentFill -= drainRate * Time.deltaTime;
        currentFill = Mathf.Clamp(currentFill, minFill, maxFill);

        // 🔹 Directly set the bar (no extra tween coroutine)
        bar.SetFillInstant(currentFill);

        // Trigger fail
        if (currentFill <= minFill)
        {
            active = false;
            bar.Hide(); // plays flashing + shaking
            onMeterEmpty?.Invoke();
        }
    }

    public void AddPressure(float fillAmount)
    {
        if (!active) return;

        currentFill += fillAmount;
        currentFill = Mathf.Clamp(currentFill, minFill, maxFill);

        // Immediately apply the value — no tweening
        bar.SetFillInstant(currentFill);
    }


    public void FinishSuccess()
    {
        active = false;
        bar.HideImmediate();
    }
}
