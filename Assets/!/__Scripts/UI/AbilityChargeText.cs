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
        string amount = Player.Instance == null ? "" : Player.Instance.MaxAbilityCharge.ToString();
        abilityChargeTMP.text = $"{amount}";
    }
}