using UnityEngine;
using System.Collections.Generic;

public class OrbitingShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public GameObject shieldPrefab;
    public int maxShields = 2;
    public float orbitRadius = 2f;
    public float orbitSpeed = 60f;
    public bool rotateClockwise = true;
    [Tooltip("Rotation offset for the initial shield placement (in degrees)")]
    public float startAngleOffset = 0f;

    [Header("References")]
    public Transform player;

    private readonly List<Transform> shields = new List<Transform>();
    public bool IsShieldActive => shields.Count > 0;

    void Start()
    {
        if (player == null)
            player = transform; // fallback if attached to player
    }

    void Update()
    {
        CleanupDestroyedShields();

        if (shields.Count > 0)
            RotateShields();
    }

    // ------------------------------------------------------------
    // 🛡 Called when player uses ability
    // ------------------------------------------------------------
    public void AddShield()
    {
        CleanupDestroyedShields(); // always clean before adding

        // --- Case 1: Not full → spawn new one ---
        if (shields.Count < maxShields)
        {
            GameObject newShield = Instantiate(shieldPrefab, player.position, Quaternion.identity, transform);
            shields.Add(newShield.transform);
            UpdateShieldPositions();
            Debug.Log($"🛡 Added new shield. Count = {shields.Count}/{maxShields}");
            return;
        }

        // --- Case 2: Already full → try to heal a damaged shield ---
        bool healed = false;
        foreach (Transform t in shields)
        {
            if (t == null) continue;

            var shieldObj = t.GetComponentInChildren<OrbitingShieldObject>();

            if (shieldObj != null && shieldObj.CurrentHitsTaken > 0)
            {
                shieldObj.RestoreShield();
                healed = true;
                Debug.Log("💚 Restored a damaged shield to full!");
                break;
            }
        }

        // --- Case 3: All shields already full ---
        if (!healed)
        {
            Debug.Log("🔵 All shields are already full — no action taken.");
            // Optional: Play a short “ping” sound
            //AudioHelpers.PlayMyClipAtPoint(goalSound, AudioChannel.SFX, player.position, 0.7f, pitch: 1.2f);
        }
    }




    // ------------------------------------------------------------
    // 🌀 Evenly space all shields around the player
    // ------------------------------------------------------------
    private void UpdateShieldPositions()
    {
        if (shields.Count == 0) return;

        float angleBetweenShields = 360f / shields.Count;

        for (int i = 0; i < shields.Count; i++)
        {
            if (shields[i] == null) continue;

            float angleDeg = startAngleOffset + i * angleBetweenShields;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * orbitRadius;
            Vector3 targetPos = player.position + offset;

            shields[i].position = targetPos;
            shields[i].up = (shields[i].position - player.position).normalized;
        }
    }
    
    // ------------------------------------------------------------
    // 🚀 Launch all active shields in their current facing directions
    // ------------------------------------------------------------
    public void LaunchAllShields()
    {
        if (shields.Count == 0)
        {
            Debug.Log("⚠️ No shields to launch!");
            return;
        }

        foreach (Transform t in shields)
        {
            if (t == null) continue;
            var shieldObj = t.GetComponentInChildren<OrbitingShieldObject>();
            if (shieldObj != null)
                shieldObj.Launch();
        }

        shields.Clear(); // remove all launched shields from orbit list
        Debug.Log("💨 All shields launched outward!");
    }


    // ------------------------------------------------------------
    // ⚙️ Rotation logic
    // ------------------------------------------------------------
    private void RotateShields()
    {
        float direction = rotateClockwise ? -1f : 1f;
        float angleStep = orbitSpeed * direction * Time.deltaTime;

        foreach (Transform shield in shields)
        {
            if (shield == null) continue;

            shield.RotateAround(player.position, Vector3.forward, angleStep);
            shield.up = (shield.position - player.position).normalized;
        }
    }

    // ------------------------------------------------------------
    // 🧹 Utility
    // ------------------------------------------------------------
    private void CleanupDestroyedShields()
    {
        // Remove destroyed or missing shields
        for (int i = shields.Count - 1; i >= 0; i--)
        {
            if (shields[i] == null)
                shields.RemoveAt(i);
        }
    }

    public void ClearShields()
    {
        foreach (Transform s in shields)
        {
            if (s != null)
                Destroy(s.gameObject);
        }
        shields.Clear();
    }
}
