using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class PlayVFXAndDestroy : MonoBehaviour
{
    [Tooltip("If not set, will try to GetComponent<VisualEffect>()")]
    public List<VisualEffect> vfx;

    [Tooltip("VFX event name to trigger")]
    public string playEventName = "OnPlay";

    [Tooltip("Fallback lifetime if the VFX never reports completion")]
    public float safetyLifetime = 5f;

    private void Awake()
    {
        if (vfx == null || vfx.Count == 0)
            vfx = new List<VisualEffect>(GetComponents<VisualEffect>());
    }

    private void OnEnable()
    {
        Play();
    }

    public void Play()
    {
        if (vfx == null)
        {
            Debug.LogWarning("PlayVFXAndDestroy: No VisualEffect found.");
            Destroy(gameObject);
            return;
        }

        Debug.Log("Playing VFX and scheduling destroy.");

        foreach (var effect in vfx)
        {
            effect.SendEvent(playEventName);
        }
        StartCoroutine(WaitForFinish());
    }

    private IEnumerator WaitForFinish()
    {
        float elapsed = 0f;

        // Wait while particles are alive
        while (true)
        {
            elapsed += Time.deltaTime;

            // Safety escape
            if (elapsed >= safetyLifetime)
                break;

            yield return null;
        }

        Destroy(gameObject);
    }
}
