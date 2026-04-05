using System;

public static class DevCheats
{
    public static bool Invincible { get; private set; }

    public static event Action<bool> OnInvincibilityChanged;

    public static void SetInvincible(bool value)
    {
        if (Invincible == value)
            return;

        Invincible = value;
        OnInvincibilityChanged?.Invoke(value);
    }

    public static void ToggleInvincible()
    {
        SetInvincible(!Invincible);
    }
}
