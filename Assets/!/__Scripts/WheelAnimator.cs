using UnityEngine;
using System.Collections;

public class WheelAnimator : MonoBehaviour
{
    [Header("Settings")]
    public float scaleUpSize = 1.2f;     
    public float animationSpeed = 4f;    
    public float rotationSpeed = 90f;
    public bool playOnEnable = false;    
    [SerializeField] private float growthFactor = 1f;   // multiplies base size permanently
    [SerializeField] private float growthPerBounce = 0.05f; // tweak: grows 5% each bounce


    [Header("Breathing Pulse Settings")]
    public float pulseAmount = 0.05f;      // how much it grows/shrinks
    public float pulseSpeed = 2f;          // pulse frequency

    private Coroutine activeRoutine;
    private Coroutine pulseRoutine;

    private bool isActive = false;
    public Transform wheelTransform;
        private Coroutine bounceRoutine;


    void OnEnable()
    {
        if (playOnEnable)
            OpenWheel();
    }

    void Update()
    {
        if (isActive)
        {
            wheelTransform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
        }
    }

    public void OpenWheel()
    {
        wheelTransform.gameObject.SetActive(true);
        isActive = true;

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);

        activeRoutine = StartCoroutine(AnimateWheel(Vector3.one * scaleUpSize, false));
    }

    public void CloseWheel(bool killOnEnd = false)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);

        activeRoutine = StartCoroutine(AnimateWheel(Vector3.zero, true, killOnEnd));
    }





    public void ImpactBounce(float strength = 0.15f, float speed = 10f)
    {
        if (!isActive) return;

        // Stop pulse so it doesn't override bounce
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        if (bounceRoutine != null)
            StopCoroutine(bounceRoutine);

        bounceRoutine = StartCoroutine(BounceRoutine(strength, speed));
    }


    private IEnumerator BounceRoutine(float strength, float speed)
    {
        // Use base scale * permanent growth
        Vector3 baseScale = Vector3.one * scaleUpSize * growthFactor;

        Vector3 currentScale = wheelTransform.localScale; 
        Vector3 targetUp = baseScale * (1f + strength);

        float t = 0f;

        // Scale up
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * speed;
            wheelTransform.localScale = Vector3.Lerp(currentScale, targetUp, t);
            yield return null;
        }

        // Increase growth factor AFTER the upward punch
        growthFactor += growthPerBounce;

        // New base scale (bigger permanently)
        Vector3 newBaseScale = Vector3.one * scaleUpSize * growthFactor;

        // Scale down to the NEW base
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * speed;
            wheelTransform.localScale = Vector3.Lerp(targetUp, newBaseScale, t);
            yield return null;
        }

        // Restart breathing pulse with new base size
        pulseRoutine = StartCoroutine(PulseLoop());
    }




    IEnumerator AnimateWheel(Vector3 targetScale, bool disableAfter, bool killOnEnd = false)
    {
        Vector3 startScale = wheelTransform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * animationSpeed;
            wheelTransform.localScale =
                Vector3.Lerp(startScale, targetScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        wheelTransform.localScale = targetScale;

        if (killOnEnd)
        {
            Destroy(gameObject);
            yield break;
        }

        if (disableAfter)
        {
            isActive = false;
            wheelTransform.gameObject.SetActive(false);
        }
        else
        {
            // Start subtle breathing pulse
            pulseRoutine = StartCoroutine(PulseLoop());
        }
    }

    IEnumerator PulseLoop()
    {
        while (isActive)
        {
            Vector3 baseScale = Vector3.one * scaleUpSize * growthFactor;

            float pulse = Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
            wheelTransform.localScale = baseScale * (1f + pulse);

            yield return null;
        }
    }

}
