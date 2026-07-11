using UnityEngine;

public class HealPopup : MonoBehaviour
{
    public TextPopupObject healPopupPrefab;
    public Transform popupTargetSpawnPos;

    private void OnEnable() => Player.OnHeal += OnHeal;
    private void OnDisable() => Player.OnHeal -= OnHeal;
    
    private void OnHeal(int amount, bool wasFullHealth)
    {
        if (healPopupPrefab != null)
        {
            TextPopupObject popup = Instantiate(healPopupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(amount);
        }
    }
}
