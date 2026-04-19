using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class TNW_Refinery : TiberiumNetworkBuilding
{
    public List<TR.Harvester> boundHarvesters = new();
    public new RefineryDef def;

    private bool recallHarvesters;

    public TiberiumContainer Container => CompTNW.Container;

    public override IEnumerable<IntVec3> ConnectableCells
    {
        get
        {
            var rect = this.OccupiedRect();
            rect.minZ += 1;
            return rect.Cells;
        }
    }

    public float FlowAmount
    {
        get
        {
            var val = def.flowAmount;
            return val;
        }
    }

    public bool CanBeRefinedAt => GetComp<CompPowerTrader>().PowerOn && !this.IsBrokenDown() && !Container.CapacityFull;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref recallHarvesters, "recallHarvesters");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        def = base.def as RefineryDef;
        if (!respawningAfterLoad) boundHarvesters.Add(SpawnHarvester());
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        for (var i = 0; i < boundHarvesters.Count; i++)
        {
            TR.Harvester harvester = boundHarvesters[i];
            if (!harvester.DestroyedOrNull())
            {
                if (harvester.MainRefinery == this)
                {
                    harvester.MainRefinery = null;
                    harvester.UpdateRefineries();
                }

                if (mode != DestroyMode.Deconstruct)
                    Messages.Message("RefineryLost".Translate(), this, MessageTypeDefOf.NegativeEvent);
            }
        }

        base.Destroy(mode);
    }

    private TR.Harvester SpawnHarvester()
    {
        TR.Harvester harvester = (TR.Harvester)PawnGenerator.GeneratePawn(def.harvester, Faction);
        harvester.ageTracker.AgeBiologicalTicks = 0;
        harvester.ageTracker.AgeChronologicalTicks = 0;
        harvester.Rotation = Rotation;
        harvester.MainRefinery = this;
        var spawnLoc = InteractionCell;
        return (TR.Harvester)GenSpawn.Spawn(harvester, spawnLoc, Map, Rotation.Opposite);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos()) yield return gizmo;

        yield return new Command_Action
        {
            defaultLabel = recallHarvesters ? "TR_Harvester_Return".Translate() : "TR_Harvester_Harvest".Translate(),
            defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
            icon = recallHarvesters
                ? ContentFinder<Texture2D>.Get("UI/Icons/Network/Return")
                : ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvest"),
            action = delegate { recallHarvesters = !recallHarvesters; }
        };
    }
}