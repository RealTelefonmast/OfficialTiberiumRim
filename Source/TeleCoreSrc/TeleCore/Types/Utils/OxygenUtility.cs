using RimWorld;
using Verse;

namespace TeleCore.Types.Utils;

public static class OxygenUtility
{
    private static bool BurnsMaterial(ThingFilter filter)
    {
        foreach (var def in filter.thingDefs)
            if (def == ThingDefOf.WoodLog || def == ThingDefOf.Chemfuel)
                return true;

        return false;
    }

    //Example: Hives etc
    public static bool IsLivingBreathing(ThingWithComps thing)
    {
        var invalid = true;
        if (thing.comps.NullOrEmpty()) return false;
        var hiveSpawner = thing.GetComp<CompSpawnerHives>();
        return hiveSpawner != null;
        //TODO: Adjust for dynamic cases (modded and so on)
    }

    //Example: Campfire, torches, wood fired generators etc
    public static bool IsBurner(ThingWithComps thing)
    {
        var invalid = true;
        if (thing.comps.NullOrEmpty()) return false;
        var refuelable = thing.GetComp<CompRefuelable>();
        var heatPusher = thing.GetComp<CompHeatPusher>();
        if (refuelable == null || heatPusher == null) return false;
        invalid &= !BurnsMaterial(refuelable.Props.fuelFilter);
        invalid &= !(heatPusher.Props.heatPerSecond > 0);
        return !invalid;
    }
}