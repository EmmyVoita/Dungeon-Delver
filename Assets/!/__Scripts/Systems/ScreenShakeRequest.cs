using UnityEngine;

[System.Serializable]
public struct ScreenShakeRequest
{
    public float duration;
    public float magnitude;
    public Vector2 direction;
    public bool directional;
    public bool unscaled;

    public ScreenShakeRequest(
        float duration,
        float magnitude,
        Vector2 direction = default,
        bool directional = false,
        bool unscaled = true)
    {
        this.duration = duration;
        this.magnitude = magnitude;
        this.direction = direction;
        this.directional = directional;
        this.unscaled = unscaled;
    }
}