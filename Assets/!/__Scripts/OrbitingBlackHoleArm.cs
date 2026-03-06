using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class OrbitingBlackHoleArm : MonoBehaviour
{
    [Header("Center Target")]
    public Vector3 centerTarget;

    [Header("Scene References")]
    public Transform blackHoleVisual;   // sprite/art for black hole (optional)
    public Transform armPivot;          // pivot at black hole position (required)
    public GameObject spikyBallPrefab;  // should include collider + DamageEffect

    [Header("Orbit (Black Hole movement)")]
    public float orbitRadius = 2.6f;
    public float orbitAngularSpeed = 120f; // degrees per second
    public bool clockwiseOrbit = true;
    public float orbitStartAngle = 0f;     // degrees

    [Header("Arm (Spike line)")]
    public SoundEffect buildSound;
    public int armSegmentCount = 7;
    public float buildSegmentDelay = 0.1f;
    public float segmentSpacing = 0.45f;
    public float armRotationSpeed = 200f; // degrees per second
    public bool clockwiseArm = true;
    public float armStartAngle = 0f;      // degrees

    [Header("Tuning")]
    public float visualSpinSpeed = 180f; // optional spin for black hole sprite

    private readonly List<Transform> segments = new();
    private float orbitAngle;
    private float armAngle;

    private bool armBuilt = false;

    private void Awake()
    {
        orbitAngle = orbitStartAngle;
        armAngle = armStartAngle;

        Vector3 center = centerTarget;
        float r = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = center + new Vector3(Mathf.Cos(r), Mathf.Sin(r), 0f) * orbitRadius;

        transform.position = orbitPos;

        StartCoroutine(BuildArm());
        SyncTransformsImmediate();
    }

    private void Update()
    {
        if (centerTarget == null)
            return;

       

        // 1) Orbit the black hole around the player/center
        float orbitDir = clockwiseOrbit ? -1f : 1f;

        if(armBuilt)
            orbitAngle += orbitAngularSpeed * orbitDir * Time.deltaTime;

        Vector3 center = centerTarget;
        float r = orbitAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = center + new Vector3(Mathf.Cos(r), Mathf.Sin(r), 0f) * orbitRadius;

        transform.position = orbitPos;

        if(!armBuilt) 
            return;

        // 2) Rotate the spike arm around the black hole
        float armDir = clockwiseArm ? -1f : 1f;
        armAngle += armRotationSpeed * armDir * Time.deltaTime;

        armPivot.localRotation = Quaternion.Euler(0f, 0f, armAngle);

        // 3) Optional: spin black hole sprite for juice
        if (blackHoleVisual != null)
            blackHoleVisual.Rotate(0f, 0f, visualSpinSpeed * Time.deltaTime);

        // Keep armPivot located at the black hole position (local origin)
        // (If armPivot is a child at localPosition zero, you're good.)
    }

    private IEnumerator BuildArm()
    {
        // Destroy any existing children segments (if you reuse prefab instances in editor)
        for (int i = armPivot.childCount - 1; i >= 0; i--)
        {
            Destroy(armPivot.GetChild(i).gameObject);
        }
        segments.Clear();

        // Place segments along +X axis in local space of armPivot
        for (int i = 0; i < armSegmentCount; i++)
        {
            GameObject seg = Instantiate(spikyBallPrefab, armPivot);
            seg.transform.localPosition = new Vector3((i + 1) * segmentSpacing, 0f, 0f);
            seg.transform.localRotation = Quaternion.identity;

            segments.Add(seg.transform);

            AudioHelpers.PlaySoundEffect(buildSound, this.transform.position);

            yield return new WaitForSeconds(buildSegmentDelay);
        }

        armBuilt = true;
    }

    private void SyncTransformsImmediate()
    {
        // Ensure armPivot is at origin relative to this obstacle
        if (armPivot != null)
            armPivot.localPosition = Vector3.zero;

        if (blackHoleVisual != null)
            blackHoleVisual.localPosition = Vector3.zero;
    }
}