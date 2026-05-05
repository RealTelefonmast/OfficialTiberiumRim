namespace TeleCore.Types.Utils;

[TeleCoreStartupClass]
public static class SharedStartup
{
    static SharedStartup()
    {
        TeleCoreStaticStartup.OnStartup += OnStartup;
    }

    private static void OnStartup()
    {
        TLog.Message("Starting TeleCore.Shared...");
    }
}