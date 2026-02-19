using UnityEngine;

public class ScalePulseY : MonoBehaviour
{
    public float pulseAmount = 0.2f;
    public float pulseSpeed = 4f;

    private Vector3 baseScale;
    private float t;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        t += Time.deltaTime * pulseSpeed;

        float pulse = Mathf.Sin(t) * pulseAmount;
        transform.localScale = new Vector3(
            baseScale.x,
            baseScale.y * (1f + pulse),
            baseScale.z
        );
    }
}
