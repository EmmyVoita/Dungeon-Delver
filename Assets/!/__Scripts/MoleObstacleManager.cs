using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
using System.Collections.Generic;


public class MoleObstacleManager : MonoBehaviour
{
    public Sprite dirtSprite;
    public float dirtHeightOffset = -0.25f;
    public static Action DestroyMolesEvent;
    public GameObject instructionCanvasPrefab;
    public string displayMessage = "WHACK THE MOLES";
    public float displayMessageDuration = 2.0f;
    public float moleScale = 1.5f;
    public float randomOffsetRange = 0.4f; // how far variations can be (+/-)
    public AudioClip successSound;
    public AudioClip failSound;
    public float uiverticalOffset = -400f;

    [Header("Timing")]
    public float obstacleDuration = 8f;   // total time the challenge runs
    public float spawnInterval = 0.8f;

    [Header("Meter Settings")]
    public float meterDrainRate = 0.12f;          // handled by PressureMeterController
    public float meterGainGood = 0.25f;           // +pressure on good hit
    public float meterPenaltyEarlyLate = 0.12f;   // -pressure on bad timing

    [Header("References")]
    public GameObject molePrefab;
    public Transform[] molePositions;   // 4 mole spots around the player

    public GameObject pressureMeterPrefab;
    private PressureMeterController pressureMeter;

    private bool active = true;
    private float elapsed = 0f;

    // 🔹 Track last mole index so we don't spawn on same side twice
    private int lastMoleIndex = -1;
    private bool[] moleOccupied;
    private List<GameObject> dirtClumps;

    private IEnumerator ShowInstructionMessage(string message = null)
    {
        bool finished = false;

        var canvas = Instantiate(instructionCanvasPrefab);
        canvas.GetComponent<InstructionCanvas>()
            .ShowMessage(message ?? displayMessage, displayMessageDuration,() => finished = true);

        // Wait here until canvas calls callback
        while (!finished)
            yield return null;
    }


    void Start()
    {
        dirtClumps = new List<GameObject>();
        moleOccupied = new bool[molePositions.Length];

        ObstacleManager.Instance.RegisterObstacle(gameObject);

        StartCoroutine(ObstacleRoutine());
    }

    IEnumerator ObstacleRoutine()
    {
        yield return StartCoroutine(ShowInstructionMessage(displayMessage));

        GameObject obj = Instantiate(pressureMeterPrefab, Vector3.zero, Quaternion.identity);
        pressureMeter = obj.GetComponent<PressureMeterController>();

        // Initialize the pressure meter
        pressureMeter.drainRate = meterDrainRate;

        // Show meter without using countdown timer (disableTimer = true)
        pressureMeter.Show(
            onEmpty: () => OnFail(),
            positionOverride: new Vector2(0, uiverticalOffset)
        );

        while (active && elapsed < obstacleDuration)
        {
            elapsed += spawnInterval;

            SpawnMole();

            yield return new WaitForSeconds(spawnInterval);
        }

        if (active)
            OnSuccess();
    }

    void OnMoleHidden(int index)
    {
        moleOccupied[index] = false;
    }


    void SpawnMole()
    {
        if (!active) return;

        // Find all free positions
        System.Collections.Generic.List<int> freeSlots = new System.Collections.Generic.List<int>();

        for (int i = 0; i < molePositions.Length; i++)
        {
            if (!moleOccupied[i])
                freeSlots.Add(i);
        }

        // No free slots → skip this spawn cycle
        if (freeSlots.Count == 0)
            return;

        // Pick a random free slot
        int index = freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];

        Transform spot = molePositions[index];
        moleOccupied[index] = true; // mark occupied

        // calculate direction offset
        Vector2 center = Vector2.zero;
        Vector2 direction = (new Vector2(spot.position.x, spot.position.y) - center).normalized;

        // spawn mole
        GameObject obj = Instantiate(molePrefab, spot.position, Quaternion.identity);
        MoleObject mole = obj.GetComponent<MoleObject>();
        mole.slotIndex = index; // assign back reference so mole can free itself

        float offset = UnityEngine.Random.Range(-randomOffsetRange, randomOffsetRange);
        obj.transform.position += (Vector3)(direction * offset);

        mole.onHitCallback = OnMoleHit;
        mole.onHiddenCallback = OnMoleHidden;   // NEW CALLBACK
        mole.Activate();
    }


    void OnMoleHit(MoleObject mole, MoleState state)
    {
        if (!active) return;

        if (state == MoleState.Good)
        {
            pressureMeter.AddPressure(meterGainGood);
        }
        else
        {
            pressureMeter.AddPressure(-meterPenaltyEarlyLate);
        }
    }

    void OnSuccess()
    {
        if (!active) return;
        active = false;

        DestroyMolesEvent?.Invoke();

        Debug.Log("MOLE OBSTACLE CLEARED!");

        AudioHelpers.PlayMyClipAtPoint(successSound, AudioChannel.SFX, Camera.main.transform.position);

        // Play the finish animation on the UI bar
        pressureMeter.FinishSuccess();

        StartCoroutine(KillClumps());
        StartCoroutine(Unregister());
    }

    void OnFail()
    {
        if (!active) return;
        active = false;

        DestroyMolesEvent?.Invoke();

        Debug.Log("MOLE OBSTACLE FAILED!");

        AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);

        Player.Instance.DamageSelf(1);

        StartCoroutine(KillClumps());
        StartCoroutine(Unregister());
    }

    private IEnumerator KillClumps()
    {
        foreach(GameObject dirt in dirtClumps)
        {
            SpriteRenderer sr = dirt.GetComponent<SpriteRenderer>();
            sr.DOColor(Color.clear, 0.5f);
            Destroy(dirt, 0.5f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Unregister()
    {
        yield return new WaitForSeconds(2.0f);
        ObstacleManager.Instance.UnregisterObstacle(gameObject);
        Destroy(gameObject, 0.25f);
    }
}
