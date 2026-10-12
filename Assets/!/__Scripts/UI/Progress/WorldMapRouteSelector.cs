using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WorldMapRouteSelector : MonoBehaviour
{
    //[Header("GameState")]
    //[SerializeField] private GameState nextState = GameState.PreRoundCountdown;

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;

    [Header("Core")]
    [SerializeField] private int baseOptionCount = 3;
    [SerializeField] private bool allowRerolling = false;

    [Header("References")]
    [SerializeField] private RectTransform optionsContainer;
    [SerializeField] private GameObject worldMapOptionPrefab;
    [SerializeField] private WorldMapController controller;
    [SerializeField] private WorldMapViewer mapViewer;
    [SerializeField] private KeyPromptController promptController;
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventoryOpenSettings openSettings;
    [SerializeField] private RectTransform optionsLayoutRoot;
    [SerializeField] private UIPanelNudge panelNudge;
    [SerializeField] private RouteOptionDatabase routeOptionDatabase;


    [SerializeField] private PagesController pagesController;
    [SerializeField] private ShopDrawPoolInventory drawPoolInventory;
    [SerializeField] private InventoryOpenSettings drawPoolOpenSettings;
    [SerializeField] private int targetExitPage = 0;

    [Header("UI")]
    //[SerializeField]
    //private TextMeshProUGUI[] optionTexts;
    [SerializeField] private TextTypewriter descriptionTypewriter;

    [Header("Route Icon Animation")]
    [SerializeField] private float iconOutDuration = 0.08f;
    [SerializeField] private float iconInDuration = 0.2f;
    [SerializeField] private float iconStartScale = 0.4f;


    [Header("Colors")]
    [SerializeField]
    private Color selectedColor = Color.yellow;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeInInterval = 0.5f;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float scaleSpeed = 0.5f;

    [SerializeField]
    private Color normalColor = Color.white;

    [Header("Audio")]
    [SerializeField] private SoundEffect popInSound;
    [SerializeField] private float pitchStep = 0.05f;


    private readonly List<WorldMapRouteOption> currentOptions = new();
    private int _selectedIndex;
    private bool isOpen;
    private Coroutine _buildCoroutine;
    private bool _textBuilt;
    private int _currentStep = 0;
    private DG.Tweening.Sequence _iconSequence;
    private bool _rerollLock;
    private int _currentOptionCount;
    private List<TextMeshProUGUI> optionTexts = new();


    

    private void Awake()
    {
        //routeIconRoot.gameObject.SetActive(false);
        CleanUpText();
        //RunStateManager.Instance.AddGuaranteedWorldMapOptions(baseOptionCount);

        foreach(var option in routeOptionDatabase.routeOptions)
        {
            RunStateManager.Instance.AddRouteWeightMultiplier(option.type, option.baseWeight);
        }
    }


    public void Open()
    {
        _currentOptionCount = RunStateManager.Instance.GetWorldMapOptionCount(baseOptionCount);

        _selectedIndex = 0;
        isOpen = true;
        _textBuilt = false;


        DrawOptions();
       
        InputFocusManager.Claim(this);
    }

    private void DestroyOptionUI()
    {
        foreach (TextMeshProUGUI optionText in optionTexts)
        {
            if (optionText != null)
                Destroy(optionText.gameObject);
        }

        optionTexts.Clear();
    }

    private void BuildOptionUI()
    {
        DestroyOptionUI();

        foreach (WorldMapRouteOption routeOption in currentOptions)
        {
            GameObject optionObject = Instantiate(worldMapOptionPrefab, optionsContainer);

            TextMeshProUGUI optionText = optionObject.GetComponent<TextMeshProUGUI>();

            optionText.text = routeOption.displayName;
            optionText.color = normalColor;
            optionText.alpha = 0f;
            optionText.transform.localScale = Vector3.one;

            optionTexts.Add(optionText);
        }
    }


    private void Update()
    {
    
        if (!isOpen || !InputFocusManager.HasFocus(this) || !_textBuilt)
            return;

        AnimateSelection();
        

        if(HandleRerollInput())
            return;

        if(HandleDrawPoolInput())
            return;

        if(HandleSelectInput())
            return;

        HandleNavigation();
    }

    private bool HandleDrawPoolInput()
    {
        if(!InputBindingManager.Instance.GetKeyDown(InputActionType.UseAbility))
            return false;

        WorldMapRouteOption hovered = currentOptions[_selectedIndex];

        UpgradeCategory category;

        bool isUpgradeCategory = hovered.TryGetUpgradeCategory(out category);

        if(!isUpgradeCategory)
            return false;
        
        drawPoolInventory.SetTargetCategory(category);
        drawPoolInventory.SetVisibilityData(pagesController, targetExitPage);

        if(drawPoolInventory.TryOpen(promptController,drawPoolOpenSettings))
            pagesController.ShowNextPage();

        return true;
    }

    private bool HandleRerollInput()
    {
        #if !UNITY_EDITOR
            if(!allowRerolling)
                return false;
        #endif
        

        if(!RunStateManager.Instance.CanRerollShop || _rerollLock)
            return false;

        if(!InputBindingManager.Instance.GetKeyDown(InputActionType.Reroll))
            return false;

        bool goThroughWithReroll = RunStateManager.Instance.ConsumeShopReroll();
        _rerollLock = true;
        if(goThroughWithReroll)
        {
            Reroll();
        } 

        return true;
    }

    private bool HandleSelectInput()
    {
        if (!InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            return false;

        InputBindingManager.Instance.BlockUntilRelease(InputActionType.Confirm);
        SelectCurrentOption();
        return true;
    }

    private void HandleNavigation()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            MoveSelection(1);
        }

        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            MoveSelection(-1);
        }
    }

    private void Reroll()
    {
        _selectedIndex = 0;
        _textBuilt = false;

        descriptionTypewriter.StopTyping(true);

        DestroyOptionUI();

        //CleanUpText();
        DrawOptions();
    }

    
    private WorldMapRouteOption DrawRouteOption(List<WorldMapRouteOption> available)
    {
        float totalWeight = 0f;
        foreach(var routeOption in available)
        {
            totalWeight += RunStateManager.Instance.GetRouteWeightMultiplier(routeOption.type);
        }

        float roll = Random.Range(0,totalWeight);

        foreach(var option in available)
        {
            roll -= RunStateManager.Instance.GetRouteWeightMultiplier(option.type);

            if(roll <= 0)
            {
                return option;
            }
        }

        return null;
    }


    private void DrawOptions()
    {
        currentOptions.Clear();

        List<WorldMapRouteOption> available = routeOptionDatabase.routeOptions.Where(CanRouteAppear).ToList();

        for (int i = 0; i < _currentOptionCount && available.Count > 0; i++)
        {
            WorldMapRouteOption option = DrawRouteOption(available);
            currentOptions.Add(option);
            available.Remove(option);
        }

        if (_buildCoroutine != null)
        {
            StopCoroutine(_buildCoroutine);
            _buildCoroutine = null;
        }

        _buildCoroutine =StartCoroutine(UpdateOptionText());
    }

    private bool CanRouteAppear(WorldMapRouteOption option)
    {
        switch (option.type)
        {
            case WorldMapRouteType.DuplicateCard:
                return ShopManager.Instance.HasAnyDuplicatableCard();

            default:
                return true;
        }
    }
  

    private IEnumerator UpdateOptionText()
    {
        BuildOptionUI();

        Canvas.ForceUpdateCanvases();

        if (optionsLayoutRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionsLayoutRoot);

        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < optionTexts.Count; i++)
        {
            Tween fadeTween = optionTexts[i].DOFade(1f, fadeInDuration);

            float pitchMult = 1.0f + pitchStep * i;

            AudioHelpers.PlaySoundEffect(
                popInSound,
                transform.position,
                pitchScalar: pitchMult
            );

            yield return fadeTween.WaitForCompletion();

            yield return new WaitForSecondsRealtime(fadeInInterval);
        }

        _buildCoroutine = null;
        _textBuilt = true;

        promptController.PushPrompts(keyPrompts);
        _rerollLock = false;

        mapViewer.CreateCurrentRoutePreview();

        UpdateVisuals();
    }




    private void MoveSelection(int direction)
    {
        _selectedIndex += direction;

        if(direction >= 1)
            panelNudge.NudgeDown();
        else
            panelNudge.NudgeUp();

        if (_selectedIndex < 0)
            _selectedIndex = currentOptions.Count - 1;

        if (_selectedIndex >= currentOptions.Count)
            _selectedIndex = 0;


        AudioHelpers.PlaySoundEffect(
            AudioLibrary.Instance.Database.navigateMajor,
            transform.position
        );

        UpdateVisuals();
    }


    private void UpdateVisuals()
    {
        for (int i = 0; i < optionTexts.Count; i++)
        {
            optionTexts[i].color =
                i == _selectedIndex
                    ? selectedColor
                    : normalColor;
        }

        WorldMapRouteOption selected = currentOptions[_selectedIndex];
        descriptionTypewriter.StartTyping(selected.description);
        mapViewer.SetCurrentRoutePreview(selected.icon);
    }

    void AnimateSelection()
    {
        for (int i = 0; i < optionTexts.Count; i++)
        {
            float targetScale = (i == _selectedIndex) ? selectedScale : 1f;
            
            optionTexts[i].transform.localScale = Vector3.Lerp(
                optionTexts[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * scaleSpeed
            );
        }
    }


    private void SelectCurrentOption()
    {
        if (currentOptions.Count == 0)
            return;

        WorldMapRouteOption selected = currentOptions[_selectedIndex];

        RunStateManager.Instance.SelectWorldMapRoute(selected.type);

        StageLevelIndex currentStageRoundIndex = new StageLevelIndex{ levelIndex = RoundManager.Instance.TotalCurrentLevelIndex,
                                                                      stageIndex = RoundManager.Instance.CurrentStageIndex};
                                                                    
        RunStateManager.Instance.SetRouteHistory(currentStageRoundIndex,selected.type);

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select,transform.position);

        mapViewer.RemoveCurrentRoutePreview();

        Close();

        BetweenRoundFlowManager.Instance.WorldMapTransitionToNextState();
    }


    private void Close()
    {
        isOpen = false;

        StopAllCoroutines();

        //CleanUpText();
        DestroyOptionUI();

        controller.WorldMapRootRect.gameObject.SetActive(false);

        descriptionTypewriter.StopTyping(true);

        promptController.PopPrompts();

        InputFocusManager.Release(this);
    }

    private void CleanUpText()
    {
        for (int i = 0; i < optionTexts.Count; i++)
        {
            optionTexts[i].color = normalColor;
            optionTexts[i].transform.localScale = Vector3.one;
        }

        for (int i = 0; i < optionTexts.Count; i++)
        {
            optionTexts[i].alpha = 0;
            //optionTexts[i].color = normalColor;
        }

        // Let TMP/layout components see the new preferred sizes.
        Canvas.ForceUpdateCanvases();

        if (optionsLayoutRoot != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(optionsLayoutRoot);
        }

        Canvas.ForceUpdateCanvases();
    }
}