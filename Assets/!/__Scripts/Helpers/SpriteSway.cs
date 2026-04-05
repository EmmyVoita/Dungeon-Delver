using UnityEngine;

public class SpriteSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAngle = 5f;   // Degrees left/right
    [SerializeField] private float swaySpeed = 1f;   // How fast it sways

    private float startRotationZ;

    private void Start()
    {
        startRotationZ = transform.localEulerAngles.z;
    }

    private void Update()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        transform.localRotation = Quaternion.Euler(0f, 0f, startRotationZ + sway);
    }
}
