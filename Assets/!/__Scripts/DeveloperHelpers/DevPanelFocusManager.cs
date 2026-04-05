using UnityEngine;

public static class DevPanelFocusManager
{
    public static IDevPanel ActivePanel { get; private set; }

    public static void RequestFocus(IDevPanel panel)
    {
        if (ActivePanel == panel)
            return;

        ActivePanel?.OnFocusLost();
        ActivePanel = panel;
        ActivePanel?.OnFocusGained();
    }

    public static void ClearFocus(IDevPanel panel)
    {
        if (ActivePanel == panel)
        {
            ActivePanel.OnFocusLost();
            ActivePanel = null;
        }
    }

    public static bool HasFocus(IDevPanel panel)
    {
        return ActivePanel == panel;
    }
}
