using Verse;

namespace TeleCore.Types.Exposables;

public class FixesConfig : IExposable
{
    public bool enableProjectileGraphicRandomFix;

    public void ExposeData()
    {
        Scribe_Values.Look(ref enableProjectileGraphicRandomFix, "enableProjectileGraphicRandomFix");
    }
}