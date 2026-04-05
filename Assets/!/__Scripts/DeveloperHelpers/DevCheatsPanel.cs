using UnityEngine;
using TMPro;

public class DevCheatsPanel : MonoBehaviour, IDevPanel
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI invincibleText;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;

    private bool isVisible;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            TogglePanel();

        if (DevPanelFocusManager.HasFocus(this))
        {
            if (Input.GetKeyDown(KeyCode.I))
                DevCheats.ToggleInvincible();
        }
    }

    private void TogglePanel()
    {
        isVisible = !isVisible;
        panel.SetActive(isVisible);

        if (isVisible)
            DevPanelFocusManager.RequestFocus(this);
        else
            DevPanelFocusManager.ClearFocus(this);

        UpdateUI();
    }

    void OnEnable()
    {
        DevCheats.OnInvincibilityChanged += _ => UpdateUI();
    }

    void OnDisable()
    {
        DevCheats.OnInvincibilityChanged -= _ => UpdateUI();
    }

    private void UpdateUI()
    {
        invincibleText.text =
            $"Invincible: {(DevCheats.Invincible ? "ON" : "OFF")}";
    }

    public void OnFocusGained() { }
    public void OnFocusLost()
    {
        isVisible = false;
        panel.SetActive(false);
    }
}
