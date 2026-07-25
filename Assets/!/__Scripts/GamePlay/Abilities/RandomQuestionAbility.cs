using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class RandomQuestionAbility : AbilityBase
{
    [Header("Question Settings")]
    public List<QuestionData> questions = new List<QuestionData>();
    public GameObject questionUIPrefab;
    public GameObject dicePrefab;
    public TextTypewriter diceTypewriterPrefab;
    public GameObject diceParticleEffect;
    public AudioClip questionAppearSound;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public float waitTimeAfterRoll = 1.0f;

    [Header("Buff & Penalty Settings")]
    public List<UpgradeEffectBase> possibleBuffs; // random buff from this list
    public List<string> buffDescriptions; // descriptions for each buff
    public int healthPenalty = 1;
    public float pauseFadeDuration = 0.3f;

    private bool questionActive = false;
    private RandomQuestionUI activeUI;
    private TextTypewriter activeTypewriter;

    public override void Activate(Quaternion rotation)
    {
        
        if (questionActive || questions.Count == 0)
            return;

        StartCoroutine(WaitAndActivate());
    }
    
    private IEnumerator WaitAndActivate()
    {
        //TimeManager.Instance.SetBaseScale(0f, 0.1f); // pause
        questionActive = true;

        AudioHelpers.PlayMyClipAtPoint(questionAppearSound, AudioChannel.SFX, Camera.main.transform.position);

        yield return new WaitForSecondsRealtime(0.5f);

        GameObject uiObj = Instantiate(questionUIPrefab);
        activeUI = uiObj.GetComponent<RandomQuestionUI>();
        if (activeUI == null)
        {
            Debug.LogError("❌ Missing RandomQuestionUI component on prefab!");
            //TimeManager.Instance.ResetAll(0.5f);
        }
        else
        {
            QuestionData question = questions[Random.Range(0, questions.Count)];
            activeUI.ShowQuestion(question, OnAnswerSelected);
        }
    }


    private void OnAnswerSelected(bool wasCorrect)
    {
        if (wasCorrect)
        {
            AudioHelpers.PlayMyClipAtPoint(correctSound, AudioChannel.SFX, Camera.main.transform.position);
            ShowDiceRoll();
        }
        else
        {
            //ScreenDimmerManager.Instance.UndimWholeGameScreen();
            StartCoroutine(WaitAndResume(1.0f, () =>
            {
                AudioHelpers.PlayMyClipAtPoint(wrongSound, AudioChannel.SFX, Camera.main.transform.position);
                //Player.Instance.DamageSelf(healthPenalty);
            }));
        }
    }

    private void ShowDiceRoll()
    {
        GameObject diceObj = Instantiate(dicePrefab, Vector3.zero, Quaternion.identity);
        DiceRoll dice = diceObj.GetComponent<DiceRoll>();
        //Instantiate(diceParticleEffect, dice.transform.position, Quaternion.identity);
        dice.Roll((result) =>
        {
            int safeResult = GetSafeResult(result, possibleBuffs.Count);
            Destroy(activeTypewriter?.gameObject);

            Debug.Log($"🎲 Dice rolled: {safeResult} start typing buff description");
            StartCoroutine(WaitAndResume(waitTimeAfterRoll, () =>
            {
                GrantBuff(safeResult);
                
            }));
        }, (animationResult) =>
        {
            int safeResult = GetSafeResult(animationResult, buffDescriptions.Count);
            activeTypewriter = Instantiate(diceTypewriterPrefab, Vector3.zero, Quaternion.identity);
            activeTypewriter.StartTyping(buffDescriptions[safeResult]);
        });
    }

    private IEnumerator WaitAndResume(float waitTime, System.Action onComplete = null)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        
        //ScreenDimmerManager.Instance.UndimWholeGameScreen();
        //TimeManager.Instance.SetBaseScale(1f, 1f); // smoothly resume normal time
        questionActive = false;
        onComplete?.Invoke();
    }

    private void GrantBuff(int diceResult)
    {
        if (possibleBuffs == null || possibleBuffs.Count == 0)
        {
            Debug.Log("No buffs assigned.");
            return;
        }

        UpgradeEffectBase buffPrefab = possibleBuffs[diceResult];
        UpgradeEffectBase buff = Instantiate(buffPrefab, Player.Instance.transform);
        Player.Instance.AddUpgrade(buff);
        Debug.Log($"✨ Granted random buff: {buffPrefab.name} (from dice roll safe restult {diceResult})");
    }
    
    private static int GetSafeResult(int diceResult, int listCount)
    {
        return Mathf.Clamp(diceResult, 0, listCount - 1);
    }


}
