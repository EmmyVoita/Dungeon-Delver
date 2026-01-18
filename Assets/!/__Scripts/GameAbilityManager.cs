using UnityEngine;

public class GameAbilityManager : MonoBehaviour
{
    [Header("Ability Prefabs")]
    public Transform playerContainerTransform;
    public GameObject slowTimePrefab;
    public GameObject orbitingShieldPrefab;
    public GameObject placeShieldPrefab;
    public GameObject projectileBurstPrefab;
    public GameObject randomQuestionPrefab;
    public GameObject goldenHarvestPrefab;
    public AbilityBase currentAbility;

    private void Start()
    {
        // Read the selection from the static class
        AbilityType selected = AbilitySelection.SelectedAbility;

        Debug.Log("🎯 Loaded ability: " + selected);

        // Instantiate or activate the chosen ability
        switch (selected)
        {
            case AbilityType.SlowTime:
                currentAbility = Instantiate(slowTimePrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.OrbitingShield:
                currentAbility = Instantiate(orbitingShieldPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.PlaceShield:
                currentAbility = Instantiate(placeShieldPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.ProjectileBurst:
                currentAbility = Instantiate(projectileBurstPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.RandomQuestion:
                currentAbility = Instantiate(randomQuestionPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.GoldenHarvest:
                currentAbility = Instantiate(goldenHarvestPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.None:
            default:
                currentAbility = Instantiate(slowTimePrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                Debug.LogWarning("⚠️ No ability selected!");
                break;
        }

        currentAbility.transform.parent = playerContainerTransform;

        Player.Instance.CurrentAbility = currentAbility;
    }
}
