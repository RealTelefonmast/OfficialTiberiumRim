using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TNW_Refinery : TiberiumNetworkBuilding
    {
        public new RefineryDef def;
        public List<Harvester> boundHarvesters = new List<Harvester>();

        public TiberiumContainer Container => CompTNW.Container;

        private bool recallHarvesters = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref recallHarvesters, "recallHarvesters");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = base.def as RefineryDef;
            if (!respawningAfterLoad)
            {
                boundHarvesters.Add(SpawnHarvester());
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < boundHarvesters.Count; i++)
            {
                Harvester harvester = boundHarvesters[i];
                if (!harvester.DestroyedOrNull())
                {
                    if (harvester.MainRefinery == this)
                    {
                        harvester.MainRefinery = null;
                        harvester.UpdateRefineries();
                    }                   
                    if (mode != DestroyMode.Deconstruct)
                    {
                        Messages.Message("RefineryLost".Translate(), this, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
            base.Destroy(mode);
        }

        public override IEnumerable<IntVec3> ConnectableCells
        {
            get
            {
                CellRect rect = this.OccupiedRect();
                rect.minZ += 1;
                return rect.Cells;
            }
        }

        public float FlowAmount
        {
            get
            {
                float val = def.flowAmount;
                return val;
            }
        }

        private Harvester SpawnHarvester()
        {
            Harvester harvester = (Harvester)PawnGenerator.GeneratePawn(this.def.harvester, this.Faction);
            harvester.ageTracker.AgeBiologicalTicks = 0;
            harvester.ageTracker.AgeChronologicalTicks = 0;
            harvester.Rotation = this.Rotation;
            harvester.MainRefinery = this;
            IntVec3 spawnLoc = this.InteractionCell;
            return (Harvester)GenSpawn.Spawn(harvester, spawnLoc, this.Map, Rotation.Opposite);
        }

        public bool CanBeRefinedAt
        {
            get
            {
                return this.GetComp<CompPowerTrader>().PowerOn && !this.IsBrokenDown() && !Container.CapacityFull;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = recallHarvesters ? "TR_Harvester_Return".Translate() : "TR_Harvester_Harvest".Translate(),
                defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
                icon = recallHarvesters ? ContentFinder<Texture2D>.Get("UI/Icons/Network/Return") : ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvest"),
                action = delegate
                {
                    recallHarvesters = !recallHarvesters;
                },
            };
        }
    }
}
