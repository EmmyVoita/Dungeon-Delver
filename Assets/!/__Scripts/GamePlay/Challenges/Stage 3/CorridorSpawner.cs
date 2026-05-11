using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class CorridorSpawner : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("ObstacleSettings")]
    [SerializeField] private float obstacleDuration = 12f;


    [Header("File")]
    [SerializeField] private string fileName = "Patterns/corridor1";

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2 origin = Vector2.zero;

    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;

    [Header("Scrolling")]
    [SerializeField] private float maxScrollSpeed = 5f;
    [SerializeField] private float rampScrollSpeedDuration = 8f;
    [SerializeField] private AnimationCurve easeCurve;

    
    [Header("IBossReactive")]
    [SerializeField] private float reverseAtTime = -1f;
    [SerializeField] private float reverseScrollSpeedMult = 1.5f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;

    private float _elapsed = 0f;
    private bool _hasReversed = false;
    private bool _doTryReverse = true;


    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool _updateMovement;
    private float _scrollSpeed;
    private float _direction = 1f;

    private float _speedMultiplier = 1f;
    private Tween currentTween;

    void Start()
    {
        _scrollSpeed = 0f;

        Begin();
    }

    private IEnumerator ObstacleSequence()
    {
        BuildFromFile();
        _updateMovement = true;

        currentTween = DOTween.To(() => _scrollSpeed, x => _scrollSpeed = x, maxScrollSpeed, rampScrollSpeedDuration).SetEase(easeCurve);

        yield return new WaitForSeconds(obstacleDuration);
        _updateMovement = false;
        
        CleanUp();

        End();
    }

    public void Reverse()
    {
        // kill any existing tween
        currentTween?.Kill();
    
        AudioHelpers.PlaySoundEffect(reverseSound, transform.position);

        currentTween = DOTween.Sequence()
            // 🔹 Slow down to 0
            .Append(DOTween.To(
                () => _speedMultiplier,
                x => _speedMultiplier = x,
                0f,
                0.2f
            ).SetEase(Ease.OutQuad))

            // 🔹 Flip direction
            .AppendCallback(() =>
            {
                _direction *= -1;
            })

            // 🔹 Speed back up
            .Append(DOTween.To(
                () => _speedMultiplier,
                x => _speedMultiplier = x,
                reverseScrollSpeedMult * _scrollSpeed,
                rampScrollSpeedDuration
            ).SetEase(Ease.InQuad));
    }

    

    void Update()
    {
        if(!_updateMovement) return;
        // Move entire corridor left
        transform.position += Vector3.left * _scrollSpeed * _speedMultiplier * Time.deltaTime * _direction;

        if(!BossManager.Instance.IsBossActive) return;

        if (!IsActive || !_doTryReverse) return;

        _elapsed += Time.deltaTime;

        if (!_hasReversed && reverseAtTime >= 0f && _elapsed >= reverseAtTime)
        {
            Reverse();
            _hasReversed = true;
        }
    }

    public void OnBossEffect(BossEffectType effect)
    {
        if (effect == BossEffectType.ReverseChallenge)
        {
            _doTryReverse = true;
        }
    }

    void BuildFromFile()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if (textAsset == null)
        {
            Debug.LogError($"File not found: {fileName}");
            return;
        }

        string[] lines = textAsset.text.Split('\n');

        int height = lines.Length;

        for (int y = 0; y < height; y++)
        {
            string line = lines[y].Trim();

            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];

                if (c == '#')
                {
                    Vector3 pos = new Vector3(
                        origin.x + x * cellSize,
                        origin.y - y * cellSize,
                        0f
                    );

                    GameObject obj = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                    spawnedObjects.Add(obj);
                }
            }
        }
    }

    protected override void CleanUp()
    {
        foreach(var obj in spawnedObjects)
        {
            Destroy(obj);
        }
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);
        StartCoroutine(ObstacleSequence());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}