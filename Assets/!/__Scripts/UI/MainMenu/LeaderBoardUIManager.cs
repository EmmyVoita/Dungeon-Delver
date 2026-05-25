using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class LeaderBoardUIManager : BaseMenu
{
    [Header("References")]
    [SerializeField] private RectTransform parentTransform;
    [SerializeField] private GameObject listPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Audio")]
    [SerializeField] private SoundEffect addItemSound;
    [SerializeField] private float pitchStep = 0.025f;

    [Header("Display Settings")]
    [SerializeField] private int showAmount = 10;
    [SerializeField] private float delayAmount = 0.25f;
    [SerializeField] private float fadeInOutDuration = 0.25f;

    [Header("Timing Ease")]
    [SerializeField] private Ease delayEase = Ease.OutCubic;
    [SerializeField] private Vector2 delayScaling = new Vector2(0.1f,1.2f);

    void Awake()
    {
        lockInput = true;
    }

    public override void OnOpen()
    {
        base.OnOpen();

        canvasGroup.DOFade(1, fadeInOutDuration);

        StartCoroutine(BuildList());
    }

    public override void OnClose()
    {
        base.OnClose();

        canvasGroup.DOFade(0, fadeInOutDuration);
    }

    private void Update()
    {
        if(!isActive) return;
        
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
            MenuManager.Instance.RequestMenuTransition(MenuState.Main);
            return;
        }
    }

    private IEnumerator BuildList()
    {
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }

        SaveData saveData = ScoreManager.Instance.SaveData;
        List<RunRecord> topRuns = GetTopRuns(saveData, showAmount);

        if (topRuns.Count == 0)
        {
            Debug.Log("No runs to display");
            yield break;
        }

        int count = topRuns.Count;

        for (int i = 0; i < count; i++)
        {
            var record = topRuns[i];

            GameObject obj = Instantiate(listPrefab, parentTransform);

            RunRecordItem item = obj.GetComponent<RunRecordItem>();
            item.Initialize(record, i + 1);

            // Normalized position (0 → 1)
            float t = count <= 1 ? 1f : (float)i / (count - 1);

            // DOTween ease evaluation
            float easedT = DOVirtual.EasedValue(0f, 1f, t, delayEase);

            // Delay scaling
            float delay = delayAmount * Mathf.Lerp(delayScaling.x, delayScaling.y, easedT);

            // Optional: curved pitch
            float pitchMult = Mathf.Lerp(1.0f, 1.0f + pitchStep * count, easedT);

            AudioHelpers.PlaySoundEffect(addItemSound, transform.position, pitchMult);

            yield return new WaitForSecondsRealtime(delay);
        }
    }

    public static List<RunRecord> GetTopRuns(SaveData saveData, int count = 10)
    {
        return saveData.leaderboard
            .OrderByDescending(r => r.score)
            .Take(count)
            .ToList();
    }

    public static List<RunRecord> GetTopRunsForAbility(AbilityType targetType, SaveData saveData, int count = 10)
    {
        return saveData.leaderboard
            .OrderByDescending(r => r.score)
            .Where(r => r.abilityUsed == targetType)
            .Take(count)
            .ToList();
    }
}