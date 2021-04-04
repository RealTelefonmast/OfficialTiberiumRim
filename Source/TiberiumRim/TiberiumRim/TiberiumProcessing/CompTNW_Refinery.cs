using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class CompTNW_Refinery : CompTNW
    {
        //TODO: Parking Zones should support multiple refineries
        //TODO: Parking Zones cells should always be Min(total harvesters)
        //private readonly Zone_MechParking parkingZone;

        //Settings
        private bool recallHarvesters = false;


        //Refinery works with Comp_MechStation to handle harvesters
        public Comp_MechStation MechComp => parent.GetComp<Comp_MechStation>();
        public new RefineryProperties Props => (RefineryProperties)props;

        public bool CanBeRefinedAt => CompPower.PowerOn && !parent.IsBrokenDown() && !Container.CapacityFull;

        public bool RecallHarvesters
        {
            get => recallHarvesters;
            private set => recallHarvesters = value;
        }

        public int HarvesterCount => MechComp.ConnectedMechs.Count;

        public override void Notify_ContainerFull()
        {
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SilosNeeded);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref recallHarvesters, "recallHarvesters");
            //Scribe_Collections.Look(ref Harvesters, "Harvesters", LookMode.Reference);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                //parkingZone = new Zone_MechParking(parent.Map.zoneManager, Props.harvester);
                //parkingZone.AddCell(parent.InteractionCell);
                AddHarvester(SpawnNewHarvester());
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            for (var i = MechComp.ConnectedMechs.Count - 1; i >= 0; i--)
            {
                var mech = MechComp.ConnectedMechs[i];
                var harvester = mech as Harvester;
                harvester.Notify_RefineryDestroyed(this);
                if (mode != DestroyMode.Deconstruct)
                    Messages.Message("TR_RefineryLost".Translate(), parent, MessageTypeDefOf.NegativeEvent);
            }

            base.PostDestroy(mode, previousMap);
        }

        public IntVec3 PositionFor(Harvester harvester)
        {
            return parent.InteractionCell;
            //TODO: Re-Add Parking Zone
            /*
                if (parkingZone == null)
                    return parent.InteractionCell;

                IntVec3 slot = parkingZone.SlotFor(harvester);
                if (!slot.IsValid || parkingZone == null)
                    return parent.InteractionCell;
                return slot;
            */
        }

        private Harvester SpawnNewHarvester()
        {
            Harvester harvester = (Harvester)MechComp.MakeMech(Props.harvester);
            //Harvester harvester = (Harvester)PawnGenerator.GeneratePawn(Props.harvester, parent.Faction);
            //harvester.ageTracker.AgeBiologicalTicks = 0;
            //harvester.ageTracker.AgeChronologicalTicks = 0;
            harvester.Rotation = parent.Rotation;
            harvester.SetMainRefinery((Building)parent, this, null);
            IntVec3 spawnLoc = parent.InteractionCell;
            return (Harvester)GenSpawn.Spawn(harvester, spawnLoc, parent.Map, parent.Rotation.Opposite);
        }

        //Used to share harvesters between refineries
        public void AddHarvester(Harvester harvester)
        {
            if (MechComp.MainMechLink.Contains(harvester)) return;
            MechComp.TryAddMech(harvester);
            //Harvesters.Add(harvester);
            //parkingZone?.AssignNextSlot(harvester);
        }

        public void RemoveHarvester(Harvester harvester)
        {
            MechComp.RemoveMech(harvester);
            //Harvesters.Remove(harvester);
            //parkingZone?.DismissSlot(harvester);
        }

        public override IEnumerable<IntVec3> InnerConnectionCells
        {
            get
            {
                CellRect rect = parent.OccupiedRect();
                Rot4 rot = parent.Rotation;

                if (rot == Rot4.North)
                    rect.minZ += 1;
                else if (rot == Rot4.East)
                    rect.minX += 1;
                else if (rot == Rot4.South)
                    rect.maxZ -= 1;
                else
                    rect.maxX -= 1;
                return rect.Cells;
            }
        }

        public override string CompInspectStringExtra()
        {
            string str = base.CompInspectStringExtra();
            return str; //base.CompInspectStringExtra();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            /*
            if (parkingZone != null)
            {
                foreach (var gizmo in parkingZone.GetZoneAddGizmos())
                {
                    yield return gizmo;
                }
            }
            */

            yield return new Command_Action
            {
                defaultLabel = RecallHarvesters ? "TR_RefineryAllow".Translate() : "TR_RefineryReturn".Translate(),
                defaultDesc = "TR_RefineryReturnDesc".Translate(),
                icon = RecallHarvesters ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
                action = delegate
                {
                    RecallHarvesters = !RecallHarvesters;
                },
            };
        }
    }

    public class RefineryProperties : CompProperties_TNW
    {
        public MechanicalPawnKindDef harvester;

        public RefineryProperties()
        {
            compClass = typeof(CompTNW_Refinery);
        }
    }
}
