using UnityEngine;

public class CardParallaxSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 5f;          // degrees
    public float swaySpeed = 2f;           // motion speed
    public float positionAmount = 10f;     // px offset

    private RectTransform rect;
    private bool active = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void SetActive(bool value)
    {
        active = value;

        if (!active)
        {
            rect.localRotation = Quaternion.identity;
            rect.localPosition = new Vector3(rect.localPosition.x, 0f, 0f);
        }
    }

    private void Update()
    {
        if (!active) return;

        float time = Time.unscaledTime * swaySpeed;

        float rotX = Mathf.Sin(time) * swayAmount;
        float rotY = Mathf.Cos(time * 0.8f) * swayAmount;

        float posY = Mathf.Sin(time) * positionAmount;

        rect.localRotation = Quaternion.Euler(rotX, rotY, 0f);

        Vector3 pos = rect.localPosition;
        rect.localPosition = new Vector3(pos.x, posY, pos.z);
    }
}
