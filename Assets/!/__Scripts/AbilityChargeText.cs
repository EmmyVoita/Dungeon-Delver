using UnityEngine;

public class AbilityChargeText : MonoBehaviour
{
    private TMPro.TextMeshProUGUI abilityChargeTMP;

    private void Awake()
    {
        abilityChargeTMP = GetComponent<TMPro.TextMeshProUGUI>();
    }

    void Update()
    {
        abilityChargeTMP.text = $"{Player.Instance.MaxAbilityCharge}";
    }
}