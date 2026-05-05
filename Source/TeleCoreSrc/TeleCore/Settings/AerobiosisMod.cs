using Verse;

namespace TeleCore.Settings;

public class AerobiosisMod : Mod
{
    private readonly HarmonyLib.Harmony _harmony;

    public AerobiosisMod(ModContentPack content) : base(content)
    {
        _harmony = new HarmonyLib.Harmony("telefonmast.aerobiosis");
        _harmony.PatchAll();
    }
}