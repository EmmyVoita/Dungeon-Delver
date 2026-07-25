using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Roulette : MonoBehaviour
{
    
    [SerializeField] private GameObject screwPrefab;
    [SerializeField] private float screwRadialDistance = 500f;
    [SerializeField] private float maxAngularVelocity = 1440f;
    [SerializeField] private float rotationOffset = 0f;
    [SerializeField] private float RotatePower;
    [SerializeField] private float maxStopPower = 200f;
    [SerializeField] private float minStopPower = 50f;
    [SerializeField] private float powerVariation = 0.1f;
    [SerializeField] private SoundEffect tickSound;
    [SerializeField] private float pitchDownStep = 0.01f;
    [SerializeField] private SoundEffect winSound;


    [Header("Building Slices")]
    [SerializeField] private float visualAngleOffset = 0f;
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private RouletteSliceUI slicePrefab;
    [SerializeField] private float radialDistance = 150f;
    [SerializeField] private Material rouletteMat;
    [SerializeField] private string sliceCountPropertyName = "_SliceCount";

    [Header("Reward Pointer")]
    [SerializeField] private float pointerAngle = 90f;
    [SerializeField] private RectTransform pointerVisual;
    [SerializeField] private RectTransform pointerMov;
    [SerializeField] private float pointerDistance = 220f;
    [SerializeField] private float pointerRotationOffset = -90f;

    [SerializeField] private bool drawDebugPointer = true;
    [SerializeField] private float debugRayLength = 3f;

    [Header("Flick Pointer")]
    [SerializeField] private float flickAngle = -15f;
    [SerializeField] private float flickInDuration = 0.1f;
    [SerializeField] private float flickOutDuration = 0.1f;
    [SerializeField] private Ease inEase = Ease.InOutSine;
    [SerializeField] private Ease outEase = Ease.InOutSine;

    [Header("Scale Punch")]
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private float targetScale;
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float elasticity = 1f;





    private Rigidbody2D _rBody;
    int _inRotate;
    private int _lastSlice = -1;
    private int _tickIndex = 0;
    private float _curRotatePower;
    private float _curMaxStopPower;
    private List<RouletteSliceUI> _slices = new();
    private List<GameObject> _screws = new();
    private Sequence _flickSequence;
    private float _pointerRestAngle;
    private IReadOnlyList<RewardDefinition> _rewards;
    private Action<RewardDefinition> _onRewardSelected;
    private Vector3 _rootRestScale;
    private Tween _scalePunchTween;

    float SliceAngle => 360f / RewardCount;
    private int RewardCount => _rewards?.Count ?? 0;

    private void Awake()
    {
        _rBody = GetComponent<Rigidbody2D>();

        if (pointerMov != null)
            _pointerRestAngle = pointerMov.localEulerAngles.z;

        _rootRestScale = rootRect.localScale;
    }

    public void Initialize(IReadOnlyList<RewardDefinition> rewards, Action<RewardDefinition> onRewardSelected)
    {
        _rewards = rewards;
        _onRewardSelected = onRewardSelected;
        BuildSlices();
    }


    private void BuildSlices()
    {

        for (int i = _slices.Count - 1; i >= 0; i--)
        {
            if (_slices[i] != null)
                Destroy(_slices[i].gameObject);
        }

        for (int i = _screws.Count - 1; i >= 0; i--)
        {
            if (_screws[i] != null)
                Destroy(_screws[i].gameObject);
        }

        _slices.Clear();
        _screws.Clear();

        for(int i = 0; i < RewardCount; i++)
        {
            RouletteSliceUI sliceUI = Instantiate(slicePrefab, parentRect);

            float angle =
                rotationOffset +
                visualAngleOffset +
                SliceAngle * i +
                SliceAngle * 0.5f;

            sliceUI.Initialize(_rewards[i], angle + visualAngleOffset, radialDistance);

            _slices.Add(sliceUI);

            GameObject screw = Instantiate(screwPrefab, parentRect);

            RectTransform rect = screw.GetComponent<RectTransform>();

            if(!rect)
            {
                Destroy(screw);
                return;
            }

            float screwAngle =
                rotationOffset +
                visualAngleOffset +
                SliceAngle * i;

            float angleRadians = screwAngle * Mathf.Deg2Rad;

            rect.localEulerAngles = new Vector3(rect.localEulerAngles.x,rect.localEulerAngles.y, screwAngle - 90f);

            rect.anchoredPosition = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians)) * screwRadialDistance;

            _screws.Add(screw);
        }
        
        rouletteMat.SetFloat(sliceCountPropertyName, RewardCount);
    }

    private void PositionPointerVisual()
    {
        if (pointerVisual == null)
            return;

        float radians = pointerAngle * Mathf.Deg2Rad;

        pointerVisual.anchoredPosition = new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        ) * pointerDistance;

        pointerVisual.localRotation = Quaternion.Euler(
            0f,
            0f,
            pointerAngle + pointerRotationOffset
        );
    }


    float t;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            Rotate();
        }

        UpdateTick();


        if(_rBody.angularVelocity > 0)
        {
            float angularVelocity = _rBody.angularVelocity;

            float t = Mathf.InverseLerp(0, maxAngularVelocity, angularVelocity);

            // Less braking at lower speeds
            float brake = Mathf.Lerp(minStopPower, _curMaxStopPower, t);

            _rBody.angularVelocity = Mathf.MoveTowards(
                angularVelocity,
                0,
                brake * Time.deltaTime);
        }

        if(_rBody.angularVelocity ==0 && _inRotate == 1)
        {
            t+=Time.deltaTime;
            if(t>= 0.5f)
            {
                GetReward();
                _inRotate =0;
                t = 0;
            }
        }
    }

    private int GetCurrentPointerSlice()
    {
        if (RewardCount <= 0)
            return -1;

        float wheelRotation = transform.eulerAngles.z;

        float localPointerAngle = Mathf.Repeat(
            pointerAngle
            - wheelRotation
            - rotationOffset,
            360f
        );

        return Mathf.FloorToInt(localPointerAngle / SliceAngle);
    }

    private void UpdateTick()
    {
        int currentSlice = GetCurrentPointerSlice();

        if (currentSlice < 0)
            return;

        if (_lastSlice == -1)
        {
            _lastSlice = currentSlice;
            return;
        }

        if (currentSlice == _lastSlice)
            return;

        float pitchModifier = Mathf.Max(
            0.1f,
            1f - _tickIndex * pitchDownStep
        );

        AudioHelpers.PlaySoundEffect(
            tickSound,
            Camera.main.transform.position,
            pitchModifier
        );

        PlayTickPunch();

        FlickPointer();

        _tickIndex++;
        _lastSlice = currentSlice;
    }

    private void PlayTickPunch()
    {
        _scalePunchTween?.Kill();

        // Prevent interrupted punches from becoming the new starting scale.
        rootRect.localScale = _rootRestScale;

        _scalePunchTween = rootRect.DOPunchScale(
                new Vector3(targetScale, targetScale, 0f),
                duration,
                vibrato,
                elasticity)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rootRect.localScale = _rootRestScale;
                _scalePunchTween = null;
            });
    }

    private void FlickPointer()
    {
        if (pointerMov == null)
            return;

        _flickSequence?.Kill();

        // Reset before every flick so interrupted animations do not accumulate.
        pointerMov.localRotation = Quaternion.Euler(0f, 0f, _pointerRestAngle);

        _flickSequence = DOTween.Sequence()
            .Append(
                pointerMov.DOLocalRotate(
                        new Vector3(0f, 0f, _pointerRestAngle + flickAngle),
                        flickInDuration,
                        RotateMode.Fast)
                    .SetEase(inEase)
            )
            .Append(
                pointerMov.DOLocalRotate(
                        new Vector3(0f, 0f, _pointerRestAngle),
                        flickOutDuration,
                        RotateMode.Fast)
                    .SetEase(outEase)
            )
            .SetLink(pointerMov.gameObject);
    }

    private void OnDestroy()
    {
        _flickSequence?.Kill();
    }

    public void Rotate()
    {
        BuildSlices();
        PositionPointerVisual();

        _curRotatePower = RotatePower * (1 + UnityEngine.Random.Range(-powerVariation, powerVariation));
        _curMaxStopPower = maxStopPower * (1 + UnityEngine.Random.Range(-powerVariation, powerVariation));

        _tickIndex = 0;
        if(_inRotate == 0)
        {
            _rBody.AddTorque(_curRotatePower);
            _inRotate = 1;
        }
    }

    private int GetCurrentRewardIndex()
    {
        if (RewardCount <= 0)
            return -1;

        float wheelRotation = transform.eulerAngles.z;

        // Convert the fixed pointer angle into the wheel's local angle.
        float localPointerAngle = Mathf.Repeat(
            pointerAngle
            - wheelRotation
            - rotationOffset,
            360f
        );

        int index = Mathf.FloorToInt(
            localPointerAngle / SliceAngle
        );

        return Mathf.Clamp(index, 0, RewardCount - 1);
    }

    private void GetReward()
    {
        int rewardIndex = GetCurrentRewardIndex();

        if (rewardIndex < 0)
            return;

        Win(rewardIndex);
    }


    public void Win(int index)
    {
        AudioHelpers.PlaySoundEffect(winSound, Camera.main.transform.position);

        RewardDefinition reward = _rewards[index];

        _onRewardSelected?.Invoke(reward);
    }
}