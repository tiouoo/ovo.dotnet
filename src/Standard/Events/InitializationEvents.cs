namespace Standard.Events;

public static class InitializationEvents
{
    public static event Action? BeforeSettingsLoad;

    public static void RaiseBeforeSettingsLoad()
    {
        BeforeSettingsLoad?.Invoke();
    }

    public static event Action? BeforeUiLoad;

    public static void RaiseBeforeUiLoad()
    {
        BeforeUiLoad?.Invoke();
    }

    public static event Action? AfterUiLoad;

    public static void RaiseAfterUiLoad()
    {
        AfterUiLoad?.Invoke();
    }
}