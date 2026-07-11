using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DG.Tweening;

public class UpgradeIconTab : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private InputActionType openKey = InputActionType.Interact;

    [Header("Slide Animation")]
    [SerializeField] private float hiddenX = -300f;
    [SerializeField] private float hiddenY = -300f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float shownX = 0f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutBack;


    private bool _isOpen = false;
     private bool _isClosi = false;


    void Start()
    {
        CloseImmediate();

        rect.anchoredPosition = new Vector2(hiddenX, hiddenY);
    }

    void Update()
    {
        if(InputBindingManager.Instance.GetKeyDown(openKey) && !_isOpen)
        {
            Open();
        }

        if(!InputBindingManager.Instance.GetKeyInput(openKey) && _isOpen)
        {
            Close();
        }
    }


    public void Open()
    {
        rect.DOKill();

        rect.anchoredPosition = new Vector2(hiddenX, hiddenY);

        rect.DOAnchorPosY(shownY, duration).SetEase(ease);
        rect.DOAnchorPosX(shownX, duration).SetEase(ease);

        _isOpen = true;
    }


    void Close()
    {
        rect.DOKill();

        rect.DOAnchorPosY(hiddenY, duration)
            .SetEase(Ease.InBack);

        rect.DOAnchorPosX(hiddenX, duration)
            .SetEase(Ease.InBack);

        _isOpen = false;
    }

    void CloseImmediate()
    {
        _isOpen = false;
    }
}