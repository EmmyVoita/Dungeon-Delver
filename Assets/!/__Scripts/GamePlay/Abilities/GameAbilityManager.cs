using System.Collections.Generic;
using System.Linq;
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
    public GameObject ignoreMissPrefab;
    public GameObject bigGoalPrefab;
    public GameObject defaultPrefab;
    public AbilityBase currentAbility;

    [SerializeField] private List<AbilityData> abilities;

    [SerializeField] private AbilityData defaultAbility;


    private AbilityData GetAbilityData(AbilityType targetType)
    {
        AbilityData data = abilities.FirstOrDefault(a => a.abilityType == targetType);
        return data == null ? defaultAbility : data;
    }

    private void Start()
    {
        // Read the selection from the static class
        AbilityType selected = AbilitySelection.SelectedAbility;

        AbilityData data = GetAbilityData(selected);

        currentAbility = Instantiate(data.abilityPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();

        Debug.Log("🎯 Loaded ability: " + selected);

            /*
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

            case AbilityType.IgnoreMiss:
                currentAbility = Instantiate(ignoreMissPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.BigGoal:
                currentAbility = Instantiate(bigGoalPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                break;

            case AbilityType.None:
            default:
                currentAbility = Instantiate(defaultPrefab, transform.position, Quaternion.identity).GetComponent<AbilityBase>();
                Debug.LogWarning("⚠️ No ability selected!");
                break;
        }
        */

        currentAbility.transform.parent = playerContainerTransform;

        Player.Instance.CurrentAbility = currentAbility;
    }
}
