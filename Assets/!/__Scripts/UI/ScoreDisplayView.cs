using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ScoreDisplayView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public Transform popupTarget;
    public GameObject scorePopupPrefab;
    public RectTransform scoreJumpTarget;
    public float jumpScale = 0.4f;
    public float scaleDuration = 0.1f;
    public ScreenShakeRequest shakeRequest;
    public RectTransform scoreRootRect;

    [Header("Popup Settings")]
    public float maxPopupScale = 2.5f;
    public int maxIndex = 60;
    public float popupRadius = 1.0f;
    public Vector2 popUpSpawnAngle = new Vector2(180,270);

    public float digitRollDuration = 0.12f;
    public float digitRollYOffset = 12f;
    public bool flipDigitRollDirection = false;

    [Header("Sound Settings")]
    public float basePitch = 1f;
    public float baseVolume = 0.1f;
    public float pitchStep = 0.02f;
    public float volumeStep = 0.02f;
    public float maxPitch = 2f;
    public float maxVolume = 1.0f;
    public float minBlipDelay = 0.02f;


    [Header("Accent Tally Sound")]
    public bool playAudio = false;
    public int accentComboStart = 30;
    public int accentComboInterval = 10;
    public float accentPitch = 1f;
    public float accentPitchStep = 0.05f;
    public float maxAccentPitch = 2f;
    public float accentVolume = 1f;
    public float accentVolumeStep = 0.05f;
    public float maxAccentVolume = 1.5f;



    [Header("Popup Styles")]
    public float popupStyleScalingFactor = 0.25f;
    public ScorePopupStyle normalPopup;
    public ScorePopupStyle comboPopup;
    public ScorePopupStyle abilityOverflowPopup;
    public ScorePopupStyle goldenPopup;


    [Header("Position")]
    [SerializeField] private Vector2 gameplayPosition;
    [SerializeField] private Vector2 upgradeSelectionPosition;


    private int _displayedScore = 0;
    private int _accentPitchIndex = 0;
    private float _lastBlipTime;
    private string _lastScoreString = "0";
    private bool _suppressLiveUpdates = false;
    private RectTransform _scoreRect;
    private Vector2 _originalAnchoredPos;
    private TMP_TextInfo _textInfo;

    
    
    


    private void OnEnable()
    {
        ScoreManager.OnScoreUpdated += UpdateScoreInstant;
        ScoreTallyController.OnTallyTick += HandleTallyTick;
        ScoreTallyController.OnTallyStart += HandleTallyStart;
        ScoreTallyController.OnTallyComplete += HandleTallyComplete;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }
    private void OnDisable()
    {
        ScoreManager.OnScoreUpdated -= UpdateScoreInstant;
        ScoreTallyController.OnTallyTick -= HandleTallyTick;
        ScoreTallyController.OnTallyStart -= HandleTallyStart;
        ScoreTallyController.OnTallyComplete -= HandleTallyComplete;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previous, GameState newState)
    {
        if(newState == GameState.UpgradeSelection)
        {
            MoveTo(upgradeSelectionPosition);

            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.fontSize = 112;
        }
        else
        {
            MoveTo(gameplayPosition);

            scoreText.alignment = TextAlignmentOptions.Right;
            scoreText.fontSize = 64;
        }
    }

    private void MoveTo(Vector2 target)
    {
        scoreRootRect.DOAnchorPos(
            target,
            0.35f
        )
        .SetEase(Ease.OutCubic);
    }

    private void Awake()
    {
        RectTransform rect = scoreText.rectTransform;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);


        _scoreRect = scoreText.rectTransform;
        _originalAnchoredPos = _scoreRect.anchoredPosition;

        scoreText.text = FormatScore(_displayedScore);
        scoreText.ForceMeshUpdate();
        _textInfo = scoreText.textInfo;
        _lastScoreString = scoreText.text;
    }


    private void UpdateScoreWithRoll(int newScore)
    {
        string newScoreStr = FormatScore(newScore);

        var changedDigits = GetChangedDigitIndices(_lastScoreString, newScoreStr);

        scoreText.text = newScoreStr;
        scoreText.ForceMeshUpdate();
        _textInfo = scoreText.textInfo;

        Vector3[][] cachedBaselineVerts = new Vector3[_textInfo.meshInfo.Length][];

        for (int j = 0; j < _textInfo.meshInfo.Length; j++)
            cachedBaselineVerts[j] =
                (Vector3[])_textInfo.meshInfo[j].vertices.Clone();

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

        _lastScoreString = newScoreStr;
    }





    private void HandleTallyStart(TallyType type)
    {
        // Prevent live score updates from fighting the animation
        _suppressLiveUpdates = true;
        _accentPitchIndex = 0;
    }

    private void HandleTallyComplete(TallyType type)
    {
        // --- Hard reset transform safety ---
        _scoreRect.anchoredPosition = _originalAnchoredPos;
        _suppressLiveUpdates = false;
    }

    void HandleTallyTick(TallyTick tick)
    {
        _displayedScore = ScoreManager.Instance.CurrentScore;

        UpdateScoreWithRoll(_displayedScore);
        SpawnScorePopup(tick.addedScore, tick, ScorePopupKind.NormalHit);


        if (Time.time - _lastBlipTime > minBlipDelay)
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


            SoundEffect soundEffect = AudioLibrary.Instance.Database.tallyBase;
            soundEffect.pitch = pitch;
            soundEffect.volume *= volume;

            AudioHelpers.PlaySoundEffect(soundEffect, transform.position);



            _lastBlipTime = Time.time;
        }
        
 

        bool isAccent = tick.type == TallyType.Combo && ShouldPlayAccent(tick.index, tick.total);
            

        if (isAccent)
        {
            ScreenShakeManager.Instance.Shake(shakeRequest);

            if(playAudio)
            {
                SoundEffect soundEffect = AudioLibrary.Instance.Database.tallyAccent;
                soundEffect.volume *= AccentVolume();
                soundEffect.pitch =  AccentPitch();

                AudioHelpers.PlaySoundEffect(soundEffect, transform.position);
            }

           

            scoreJumpTarget.transform.DOKill();

            scoreJumpTarget.transform.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                scoreJumpTarget.transform
                    .DOScaleY(jumpScale, scaleDuration)
            );

            seq.Append(
                scoreJumpTarget.transform
                    .DOScaleY(1f, scaleDuration * 1.2f)
                    .SetEase(Ease.OutBack)
            );

            BackgroundVisualManager.FlareBottom();

          
            _accentPitchIndex++;
        }
    }
    


    private void UpdateScoreInstant(int score)
    {
        if (_suppressLiveUpdates)
            return;

        _displayedScore = score;
        UpdateScoreWithRoll(_displayedScore);
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
        return Mathf.Min(accentPitch + (_accentPitchIndex * accentPitchStep), maxAccentPitch);
    }

    private float AccentVolume()
    {
        return Mathf.Min(accentVolume + (_accentPitchIndex * accentVolumeStep), maxAccentVolume);
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
        if (charIndex >= _textInfo.characterCount)
            yield break;

        TMP_CharacterInfo charInfo = _textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible)
            yield break;

        int vertexIndex = charInfo.vertexIndex;
        int materialIndex = charInfo.materialReferenceIndex;

        Vector3[] baseline = baselineVerts[materialIndex];
        Vector3[] liveVerts = _textInfo.meshInfo[materialIndex].vertices;

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


    private void SpawnScorePopup(int amount, TallyTick tick, ScorePopupKind kind = ScorePopupKind.Default)
    {
        if (scorePopupPrefab == null || popupTarget == null)
            return;

        float scoreMult = GetScoreMuliplierFor(kind);
        float runtimeScale = ScoreToPopupScale(scoreMult, tick);


        Vector3 spawnPos = GetSemiCircleSpawnPos(popupTarget, popupRadius);

        var popup = Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity).GetComponent<ScorePopup>();
            

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

    float ScoreToPopupScale(float scoreMultiplier, TallyTick tick)
    {
        float scale = Mathf.Pow(
            Mathf.Max(0.01f, scoreMultiplier),
            popupStyleScalingFactor
        );

        scale = Mathf.Clamp(scale, 0.75f, 1.35f);

        float t = Mathf.Clamp01((float)tick.index / maxIndex);

        return scale * Mathf.Lerp(1.0f, maxPopupScale, t);
    }



    // ------------------------------------------------------------------------
    // POPUP HELPER
    // ------------------------------------------------------------------------

    private Vector3 GetSemiCircleSpawnPos(Transform center, float radius)
    {
        float angle = Random.Range(popUpSpawnAngle.x, popUpSpawnAngle.y) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        return center.position + offset;
    }
}
