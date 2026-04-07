using Verse;

namespace TeleCore
{
    public class TrueOxygenMod : Verse.Mod
    {
        private static HarmonyLib.Harmony _oxygen;
        
        public TrueOxygenMod(ModContentPack content) : base(content)
        {
            _oxygen = new HarmonyLib.Harmony("telefonmast.trueoxygen");
            _oxygen.PatchAll();
        }
    }
}
