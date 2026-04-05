using UnityEngine;
using UnityEngine.SceneManagement;

public class AbilityMenu : MonoBehaviour
{
    public void SelectAbility(int abilityIndex)
    {
        // This could be called by a UI button
        AbilitySelection.SelectedAbility = (AbilityType)abilityIndex;
        SceneManager.LoadScene("GameScene");
    }
}
