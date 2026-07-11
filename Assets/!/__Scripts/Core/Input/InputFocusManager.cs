public static class InputFocusManager
{
    public static object CurrentOwner { get; private set; }

    public static bool HasFocus(object owner)
    {
        return CurrentOwner == null || CurrentOwner == owner;
    }

    public static void Claim(object owner)
    {
        CurrentOwner = owner;
    }

    public static void Release(object owner)
    {
        if (CurrentOwner == owner)
            CurrentOwner = null;
    }

    public static void ClearOwner()
    {
        CurrentOwner = null;
    }
}