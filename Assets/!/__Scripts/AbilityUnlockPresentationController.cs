using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityUnlockPresentationController : MonoBehaviour
{
   [SerializeField] private AbilitySelectManager selectManager;
   [SerializeField] private AbilityUnlockManager unlockManager;

   public IEnumerator PlayUnlockSequence(
        List<AbilityType> unlocks)
    {
        if(unlocks.Count <= 0) yield break;

        // Lock player input
        //MenuManager.Instance.LockActiveMenuInput(true);

        yield return null;

        

        // Navigate to the ability select page;
        MenuManager.Instance.RequestMenuTransition(MenuState.Play);

        selectManager.SetInputLocked(true);


        yield return new WaitForSeconds(2.0f);

        foreach (var ability in unlocks)
        {
            yield return PlaySingleUnlock(ability);

            yield return new WaitForSeconds(2.0f);
        }

        yield return new WaitForSeconds(2.0f);

        // Navigate back to the main page;
        MenuManager.Instance.RequestMenuTransition(MenuState.Main);

        yield return new WaitForSeconds(2.0f);

        // UnLock player input
        selectManager.SetInputLocked(false);
    }

    private IEnumerator PlaySingleUnlock(AbilityType unlock)
    {
        yield return StartCoroutine(selectManager.ScrollToCard(unlock));

        yield return new WaitForSeconds(0.5f);

        AbilityCardUI card = selectManager.GetCard(unlock);

        if(card) yield return card.PlayUnlockAnimation();

        ConfettiEffect.TriggerConfetti();

        yield return new WaitForSeconds(0.5f);

        ConfettiEffect.TriggerConfetti();

        unlockManager.MarkPresented(unlock);
    }
   
}