using System.Collections.Generic;
using RimWorld;
using TeleCore.Research.Events;
using Verse;

namespace TR.Incidents;

public class Event_TiberiumArrival : BaseEvent
{
    private Skyfaller skyfaller;
    private LocalTargetInfo targetInfo = LocalTargetInfo.Invalid;

    protected override float WeightForMap(Map map)
    {
        var baseVal = base.WeightForMap(map);
        if (map.TileInfo.Rivers != null)
            return baseVal * 10;
        return baseVal;
    }

    public override void EventSetup()
    {
        if (LandingSiteFor(TiberiumDefOf.TiberiumMeteorIncoming, MapForEvent, out var cell))
        {
            targetInfo = cell;
            EventTargets = new LookTargets(cell, MapForEvent);
            skyfaller = SkyfallerMaker.MakeSkyfaller(TiberiumDefOf.TiberiumMeteorIncoming,
                ThingMaker.MakeThing(TiberiumDefOf.TiberiumMeteoriteChunk));
        }
    }

    public override bool CanDoEventAction(int curTick)
    {
        return base.CanDoEventAction(curTick);
    }

    public override void EventAction()
    {
        var map = MapForEvent;
        if (targetInfo.IsValid) GenSpawn.Spawn(skyfaller, targetInfo.Cell, map);
    }

    private SkyfallerValue TiberiumCraterDef()
    {
        return new SkyfallerValue(TiberiumDefOf.TiberiumMeteorIncoming, TiberiumDefOf.TiberiumMeteoriteChunk);
        var skyFallers = new List<SkyfallerValue>
        {
            new(TiberiumDefOf.GreenTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterGreen, 1, 0.66f),
            new(TiberiumDefOf.BlueTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterBlue, 1, 0.33f),
            new(TiberiumDefOf.GreenTiberiumMeteorIncoming, TiberiumDefOf.TiberiumCraterHybrid, 1, 0.22f),
            new(TiberiumDefOf.RedTiberiumShardIncoming, TiberiumDefOf.RedTiberiumShard, 1, 0.01f)
        };
        return skyFallers.RandomElementByWeight(s => s.chance);
    }

    private bool LandingSiteFor(ThingDef skyfaller, Map map, out IntVec3 foundCell)
    {
        return CellFinderLoose.TryFindSkyfallerCell(skyfaller, map, out foundCell, 20, default(IntVec3), -1, true, true,
            false, false, false, false, x => CellUtils.AllowTiberiumMeteorite(x, map));
    }
}