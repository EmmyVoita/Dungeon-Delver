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

    [Header("Stop Settings")]
    [SerializeField] private float stopAfterTime = 5f;
    [SerializeField] private string stopEventName = "OnStop";


    [Header("Fallback")]
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

    [ContextMenu("Play VFX")]
    public void PlayOneShot()
    {
        if (vfx == null)
        {
            Debug.LogWarning("PlayVFXAndDestroy: No VisualEffect found.");
            Destroy(gameObject);
            return;
        }

        foreach (var effect in vfx)
        {
            effect.SendEvent(playEventName);
        }
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
        StartCoroutine(StopAndDestroy());
    }

    private IEnumerator StopAndDestroy()
    {
        yield return new WaitForSeconds(stopAfterTime);

        foreach (var effect in vfx)
        {
            effect.SendEvent(stopEventName);
        }

        float elapsed = 0f;

        while (elapsed < safetyLifetime)
        {
            bool anyAlive = false;

            foreach (var effect in vfx)
            {
                if (effect.aliveParticleCount > 0)
                {
                    anyAlive = true;
                    break;
                }
            }

            if (!anyAlive)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
