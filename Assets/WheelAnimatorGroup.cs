using UnityEngine;

public class WheelAnimatorGroup : MonoBehaviour
{
    [Header("Wheel Animators")]
    public WheelAnimator[] wheels;

    /// <summary>
    /// Opens all wheel animators.
    /// </summary>
    public void OpenAll()
    {
        if (wheels == null) return;

        foreach (var wheel in wheels)
        {
            if (wheel != null)
                wheel.OpenWheel();
        }
    }

    /// <summary>
    /// Closes all wheel animators.
    /// </summary>
    public void CloseAll(bool killOnEnd = false)
    {
        if (wheels == null) return;

        foreach (var wheel in wheels)
        {
            if (wheel != null)
                wheel.CloseWheel(killOnEnd);
        }
    }

    public void TriggerImpactBounce(float strength = 0.15f, float speed = 10f)
    {
        if (wheels == null) return;

        foreach (var wheel in wheels)
        {
            if (wheel != null)
                wheel.ImpactBounce(strength, speed);
        }
    }

}
