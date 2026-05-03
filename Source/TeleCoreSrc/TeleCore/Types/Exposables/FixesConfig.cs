using Verse;

namespace TeleCore.Unsorted;

public class FixesConfig : IExposable
{
    public bool enableProjectileGraphicRandomFix;

    public void ExposeData()
    {
        Scribe_Values.Look(ref enableProjectileGraphicRandomFix, "enableProjectileGraphicRandomFix");
    }
}