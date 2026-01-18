using UnityEngine;

public class HealPopup : MonoBehaviour
{
    public HealPopupObject healPopupPrefab;
    public Transform popupTargetSpawnPos;

    private void OnEnable() => Player.OnHeal += OnHeal;
    private void OnDisable() => Player.OnHeal -= OnHeal;
    
    private void OnHeal(int amount)
    {
        if (healPopupPrefab != null)
        {
            HealPopupObject popup = Instantiate(healPopupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(amount);
        }
    }
}
