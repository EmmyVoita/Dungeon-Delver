using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ScoreDisplayView : MonoBehaviour
{
    public enum ScoreSoundMode { BlipPerIncrement, ContinuousTone }
    public bool showHideOnGameState = true;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public Transform popupTarget;
    public GameObject scorePopupPrefab;
    public Gradient colorGradient;
    public int maxComboColor = 40;

    [Header("Popup Settings")]
    public Color popupColor = Color.yellow;
    public float popupFlyTime = 0.8f;
    public float popupScale = 1.2f;
    public float popupRadius = 1.0f;

    [Header("Score Roll Animation")]
    public float rollDistance = 12f;
    public float rollDownTime = 0.05f;
    public float rollUpTime = 0.08f;
    public Ease rollDownEase = Ease.InQuad;
    public Ease rollUpEase = Ease.OutQuad;

    public float digitRollDuration = 0.12f;
    public float digitRollYOffset = 12f;
    public bool flipDigitRollDirection = false;

    public float countDelay = 0.1f;

    [Header("Sound Settings")]
    public ScoreSoundMode soundMode = ScoreSoundMode.BlipPerIncrement;
    public AudioClip tallyBlip;
    public AudioSource tallyLoop;
    public float basePitch = 1f;
    public float baseVolume = 0.1f;
    public float pitchStep = 0.02f;
    public float volumeStep = 0.02f;
    public float maxPitch = 2f;
    public float maxVolume = 1.0f;
    public float minBlipDelay = 0.02f;
    public float loopPitchRiseSpeed = 0.5f;

    [Header("Accent Tally Sound")]
    public AudioClip accentTallyClip;
    public int accentComboStart = 30;
    public int accentComboInterval = 10;
    public float accentPitch = 1f;
    public float accentPitchStep = 0.05f;
    public float maxAccentPitch = 2f;
    public float accentVolume = 1f;
    public float accentVolumeStep = 0.05f;
    public float maxAccentVolume = 1.5f;



    [Header("High Combo Effect ✨")]
    public ParticleSystem highComboParticles;
    public int highComboThreshold = 40;



    [Header("Popup Styles")]
    public float popupStyleScalingFactor = 0.25f;
    public ScorePopupStyle normalPopup;
    public ScorePopupStyle comboPopup;
    public ScorePopupStyle abilityOverflowPopup;
    public ScorePopupStyle goldenPopup;


    private int displayedScore = 0;
    private float lastBlipTime;

    private bool suppressLiveUpdates = false;

    private RectTransform scoreRect;
    private Vector2 originalAnchoredPos;

    private string lastScoreString = "0";
    private TMP_TextInfo textInfo;
    private int accentPitchIndex = 0;


    private void OnEnable()
    {
        ScoreManager.OnScoreUpdated += UpdateScoreInstant;
        GameStateManager.OnStateChanged += HandleUIState;
        ScoreTallyController.OnTallyTick += HandleTallyTick;
        ScoreTallyController.OnTallyStart += HandleTallyStart;
        ScoreTallyController.OnTallyComplete += HandleTallyComplete;
        ScoreEvents.OnScorePopupRequested += SpawnScorePopup;
    }
    private void OnDisable()
    {
        ScoreManager.OnScoreUpdated -= UpdateScoreInstant;
        GameStateManager.OnStateChanged -= HandleUIState;
        ScoreTallyController.OnTallyTick -= HandleTallyTick;
        ScoreTallyController.OnTallyStart -= HandleTallyStart;
        ScoreTallyController.OnTallyComplete -= HandleTallyComplete;
        ScoreEvents.OnScorePopupRequested -= SpawnScorePopup;
    }

    private void Awake()
    {
        RectTransform rect = scoreText.rectTransform;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);


        scoreRect = scoreText.rectTransform;
        originalAnchoredPos = scoreRect.anchoredPosition;

        scoreText.text = FormatScore(displayedScore);
        scoreText.ForceMeshUpdate();
        textInfo = scoreText.textInfo;
        lastScoreString = scoreText.text;
    }



    private void HandleTallyStart(TallyType type)
    {
        // Prevent live score updates from fighting the animation
        suppressLiveUpdates = true;

        //ComboManager.Instance.ResetCombo();

        if (soundMode == ScoreSoundMode.ContinuousTone && tallyLoop != null)
        {
            tallyLoop.pitch = 1f;
            tallyLoop.volume = 1f;
            tallyLoop.Play();
        }

        accentPitchIndex = 0;
    }

    private void HandleTallyComplete(TallyType type)
    {
        // --- Stop continuous tone ---
        if (soundMode == ScoreSoundMode.ContinuousTone && tallyLoop != null)
        {
            tallyLoop.DOFade(0f, 0.2f).OnComplete(() =>
            {
                tallyLoop.Stop();
                tallyLoop.volume = 1f;
            });
        }

        // --- Hard reset transform safety ---
        scoreRect.anchoredPosition = originalAnchoredPos;

        suppressLiveUpdates = false;
    }

    void HandleTallyTick(TallyTick tick)
    {
        // --- Update score ---
        //ScoreManager.Instance.AddScore(tick.addedScore, type == TallyType.Combo ? ScoreSource.Combo : ScoreSource.BaseArrow);
        displayedScore = ScoreManager.Instance.CurrentScore;

        // --- Roll animation ---
        string newScoreStr = FormatScore(displayedScore);
        var changedDigits = GetChangedDigitIndices(lastScoreString, newScoreStr);

        scoreText.text = newScoreStr;
        scoreText.ForceMeshUpdate();
        textInfo = scoreText.textInfo;

        // ✅ Cache baseline vertices ONCE
        Vector3[][] cachedBaselineVerts = new Vector3[textInfo.meshInfo.Length][];

        for (int j = 0; j < textInfo.meshInfo.Length; j++)
        {
            cachedBaselineVerts[j] =
                (Vector3[])textInfo.meshInfo[j].vertices.Clone();
        }


        foreach (int index in changedDigits)
        {
            StartCoroutine(
                AnimateDigitRoll(
                    index,
                    digitRollYOffset,
                    digitRollDuration,
                    cachedBaselineVerts
                )
            );
        }

           
        lastScoreString = newScoreStr;

        SpawnScorePopup(tick.addedScore, ScorePopupKind.NormalHit);


         // --- Audio ---
        if (soundMode == ScoreSoundMode.BlipPerIncrement && tallyBlip != null)
        {
            if (Time.time - lastBlipTime > minBlipDelay)
            {
                float pitch = Mathf.Min(
                    basePitch + (tick.index * pitchStep) + (tick.total * 0.01f),
                    maxPitch
                );

                float volume = Mathf.Clamp(
                    baseVolume + (tick.index * volumeStep),
                    0f,
                    maxVolume
                );

                AudioSettingsManager.PlayTallySound(pitch, volume);



                lastBlipTime = Time.time;
            }
        }
        else if (soundMode == ScoreSoundMode.ContinuousTone && tallyLoop != null)
        {
            tallyLoop.pitch = Mathf.Lerp(1f, maxPitch, (float)tick.index / tick.total);
        }

        bool isAccent = tick.type == TallyType.Combo &&
            ShouldPlayAccent(tick.index, tick.total);

        if (isAccent && accentTallyClip != null)
        {
            AudioSettingsManager.PlayAccentTallySound(
                AccentPitch(),
                AccentVolume()
            );
            accentPitchIndex++;
        }


        // --- High combo particles ---
        if (tick.index % highComboThreshold == 0 && tick.index != 0 && highComboParticles != null)
            highComboParticles.Play();
    }
    

    private void HandleUIState(GameState previous, GameState newState)
    {
        if(showHideOnGameState)
        {
            if (newState == GameState.UpgradeSelection)
            {
                scoreText.DOColor(Color.clear, 0.3f);
            }

            if(previous == GameState.UpgradeSelection)
            {
                scoreText.DOColor(Color.white, 0.3f);
            }
        }
    }



    private void UpdateScoreInstant(int score)
    {
        if (suppressLiveUpdates)
            return;

        displayedScore = score;
        scoreText.text = FormatScore(displayedScore);
    }

    private bool ShouldPlayAccent(int comboIndex, int totalCombo)
    {
        if (totalCombo < accentComboStart)
            return false;

        int absoluteCombo = comboIndex + 1;
        return absoluteCombo >= accentComboStart &&
            absoluteCombo % accentComboInterval == 0;
    }

    private float AccentPitch()
    {
        return Mathf.Min(accentPitch + (accentPitchIndex * accentPitchStep), maxAccentPitch);
    }

    private float AccentVolume()
    {
        return Mathf.Min(accentVolume + (accentPitchIndex * accentVolumeStep), maxAccentVolume);
    }



    public static string FormatScore(int value) => value.ToString("N0");



    private List<int> GetChangedDigitIndices(string oldScore, string newScore)
    {
        List<int> indices = new();

        int maxLen = Mathf.Max(oldScore.Length, newScore.Length);
        oldScore = oldScore.PadLeft(maxLen);
        newScore = newScore.PadLeft(maxLen);

        for (int i = 0; i < maxLen; i++)
        {
            if (oldScore[i] != newScore[i] && char.IsDigit(newScore[i]))
                indices.Add(i);
        }

        return indices;
    }

    private IEnumerator AnimateDigitRoll(
        int charIndex,
        float offsetY,
        float duration,
        Vector3[][] baselineVerts
    )
    {
        if (charIndex >= textInfo.characterCount)
            yield break;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible)
            yield break;

        int vertexIndex = charInfo.vertexIndex;
        int materialIndex = charInfo.materialReferenceIndex;

        Vector3[] baseline = baselineVerts[materialIndex];
        Vector3[] liveVerts = textInfo.meshInfo[materialIndex].vertices;

        float t = 0f;

        while (t < duration)
        {
            float eased = Mathf.SmoothStep(1f, 0f, t / duration);
            float scale = Mathf.Lerp(1.08f, 1.0f, 1f - eased);

      


            // Direction now ACTUALLY WORKS
            float dir = flipDigitRollDirection ? -1f : 1f;
            Vector3 offset = Vector3.up * offsetY * eased * dir;

            liveVerts[vertexIndex + 0] = baseline[vertexIndex + 0] + offset;
            liveVerts[vertexIndex + 1] = baseline[vertexIndex + 1] + offset;
            liveVerts[vertexIndex + 2] = baseline[vertexIndex + 2] + offset;
            liveVerts[vertexIndex + 3] = baseline[vertexIndex + 3] + offset;

            Vector3 center =
            (baseline[vertexIndex + 0] +
            baseline[vertexIndex + 2]) * 0.5f;

            for (int i = 0; i < 4; i++)
            {
                Vector3 dir_ = baseline[vertexIndex + i] - center;
                liveVerts[vertexIndex + i] =
                    center + dir_ * scale + offset;
            }

            scoreText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

            t += Time.deltaTime;
            yield return null;
        }

        // ✅ Snap EXACTLY to aligned position
        liveVerts[vertexIndex + 0] = baseline[vertexIndex + 0];
        liveVerts[vertexIndex + 1] = baseline[vertexIndex + 1];
        liveVerts[vertexIndex + 2] = baseline[vertexIndex + 2];
        liveVerts[vertexIndex + 3] = baseline[vertexIndex + 3];

        scoreText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }


    private void SpawnScorePopup(int amount, ScorePopupKind kind = ScorePopupKind.Default)
    {
        if (scorePopupPrefab == null || popupTarget == null)
            return;

        float scoreMult = GetScoreMuliplierFor(kind);
        float runtimeScale = ScoreToPopupScale(scoreMult);

        Vector3 spawnPos = GetSemiCircleSpawnPos(popupTarget, popupRadius);

        var popup = Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity)
            .GetComponent<ScorePopup>();

        

        popup.Initialize(
            amount,
            popupTarget.position,
            ResolvePopupStyle(kind),
            runtimeScale
        );
    }

    private ScorePopupStyle ResolvePopupStyle(ScorePopupKind kind)
    {
        switch (kind)
        {
            case ScorePopupKind.Default:
                return normalPopup;

            case ScorePopupKind.NormalHit:
                return normalPopup;

            case ScorePopupKind.CritHit:
                return normalPopup;

            case ScorePopupKind.Combo:
                return comboPopup;
            case ScorePopupKind.AbilityOverflow:
                return abilityOverflowPopup;
            case ScorePopupKind.Golden:
                return goldenPopup;
            default:
                return normalPopup;
        }
    }


    private float GetScoreMuliplierFor(ScorePopupKind kind)
    {
        switch (kind)
        {
            case ScorePopupKind.NormalHit:
                return UpgradeManager.Instance.ModifyNormalHitValue(1.0f);

            case ScorePopupKind.CritHit:
                return UpgradeManager.Instance.ModifyCritHitValue(1.0f);
            case ScorePopupKind.Default:
                return 1f;
            case ScorePopupKind.Combo:
                return 1f;
            case ScorePopupKind.AbilityOverflow:
                return 1f;
            case ScorePopupKind.Golden:
                return 1f; // Golden crit multiplier base
            default:
                return 1f;
        }
    }

    float ScoreToPopupScale(float scoreMultiplier)
    {
        float scale = Mathf.Pow(Mathf.Max(0.01f, scoreMultiplier), popupStyleScalingFactor);
        return Mathf.Clamp(scale, 0.75f, 1.35f);
    }



    // ------------------------------------------------------------------------
    // POPUP HELPER
    // ------------------------------------------------------------------------

    private Vector3 GetSemiCircleSpawnPos(Transform center, float radius)
    {
        float angle = UnityEngine.Random.Range(200f, 340f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * 0.5f, 0f) * radius;
        return center.position + offset;
    }
}
