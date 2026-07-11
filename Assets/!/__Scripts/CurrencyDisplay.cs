using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private List<GameState> runLogicStates;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private StatRowData displayData;
    [SerializeField] private float animateDuration = 0.5f;
    [SerializeField] private float tallySoundInterval = 0.05f;
    [SerializeField] private SoundEffect currencyTallySound;
    [SerializeField] private SoundEffect finishSound;
    [SerializeField] private float pitchStep = 0.025f;
    [SerializeField] private int pitchIndex = 0;
    

    private float _lastSoundTime;

    private Tween _currencyTween;
    private int _displayedCurrency;



    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        GameStateManager.OnStateChanged += HandleStateChanged;

        _displayedCurrency = CurrencyManager.Instance.CurrentCurrency;
        RefreshText();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
        GameStateManager.OnStateChanged -= HandleStateChanged;

        _currencyTween?.Kill();
    }

    private void HandleStateChanged(GameState previousState, GameState currentState)
    {
        //currencyText.ForceMeshUpdate();

        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        //if(!runLogicStates.Contains(currentState)) return;

        _displayedCurrency = CurrencyManager.Instance.CurrentCurrency;

        RefreshText();
    }

    private void Start()
    {
        if (CurrencyManager.Instance == null)
            return;

        _displayedCurrency = CurrencyManager.Instance.CurrentCurrency;

        RefreshText();
    }

    private void HandleCurrencyChanged(int newValue)
    {

        if(!runLogicStates.Contains(GameStateManager.Instance.CurrentState)) return;

        _currencyTween?.Kill();

        _currencyTween = DOTween.To(
            () => _displayedCurrency,
            x =>
            {
                _displayedCurrency = x;
                RefreshText();

                if (Time.unscaledTime - _lastSoundTime >= tallySoundInterval)
                {
                    _lastSoundTime = Time.unscaledTime;

                    float pitchMult = newValue > CurrencyManager.Instance.PreviousCurrency ? 
                                      1.0f + pitchIndex * pitchStep :  
                                      1.0f - pitchIndex * pitchStep;

                    AudioHelpers.PlaySoundEffect(
                        currencyTallySound,
                        transform.position,
                        pitchMult
                    );

                    pitchIndex++;
                }
            },
            newValue,
            animateDuration
        )
        .SetEase(Ease.OutQuad)
        .SetLink(gameObject)
        .OnComplete(() =>
        {
            pitchIndex = 0;
            AudioHelpers.PlaySoundEffect(finishSound,transform.position);
        });
    }

    private void RefreshText()
    {
        currencyText.text = $"{displayData.prefixLabel}{_displayedCurrency:N0}";

        currencyText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}