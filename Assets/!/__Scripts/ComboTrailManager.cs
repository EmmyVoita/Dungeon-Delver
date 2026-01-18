using UnityEngine;

public class ComboTrailManager : MonoBehaviour
{
    [Header("References")]
    public Transform starTransform;
    public ParticleMover trailPrefab;
    public Transform comboUI;
    public Transform scoreUI;

    [Header("Combo Settings")]
    private float comboProgress = 0f;
    public float growSpeed = 5f;
    public float maxScale = 2f;
    public float minComboForFly = 5f;

    private bool active = false;

    void Awake()
    {
        SpawnTrail();
    }

    void Update()
    {
        // if (!active) return;

        /*
        comboProgress = Mathf.Clamp01(
            (float)ComboManager.Instance.GetComboCount() / ComboManager.Instance.maxAnimComboCount
        );
        */

        float targetScale = Mathf.Lerp(1f, maxScale, comboProgress);

        starTransform.localScale = Vector3.Lerp(
            starTransform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * growSpeed
        );
    }

    public void EndCombo()
    {
        active = false;
        SpawnTrail();

        // if (comboProgress >= minComboForFly)
    }

    public void SpawnComboEffect()
    {
        SpawnTrail();
    }

    public void SpawnTrail()
    {
        ParticleMover trail = Instantiate(trailPrefab, transform);
        trail.Initialize(comboUI, scoreUI, 0.5f);
    }
}
