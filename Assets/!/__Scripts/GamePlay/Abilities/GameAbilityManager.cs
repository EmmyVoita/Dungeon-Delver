using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameAbilityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerContainerTransform;

    [Header("Abilities")]
    [SerializeField] private List<AbilityData> abilities;
    [SerializeField] private AbilityData defaultAbility;

    public AbilityBase CurrentAbility { get; private set; }

    private void Start()
    {
        LoadAbility(AbilitySelection.SelectedAbility);
    }

    private void LoadAbility(AbilityType selectedType)
    {
        AbilityData data = GetAbilityData(selectedType);

        if (data == null || data.abilityPrefab == null)
        {
            Debug.LogError($"No valid ability configured for {selectedType}.");

            CurrentAbility = Instantiate(
                defaultAbility.abilityPrefab,
                playerContainerTransform
            );

            CurrentAbility.Initialize(defaultAbility);
            Player.Instance.CurrentAbility = CurrentAbility;
            
            return;
        }

        CurrentAbility = Instantiate(
            data.abilityPrefab,
            playerContainerTransform
        );

        CurrentAbility.Initialize(data);
        Player.Instance.CurrentAbility = CurrentAbility;

        Debug.Log($"Loaded ability: {data.abilityName}");
    }

    private AbilityData GetAbilityData(AbilityType targetType)
    {
        return abilities.FirstOrDefault(
                   ability => ability.abilityType == targetType
               )
               ?? defaultAbility;
    }
}