using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_Refinery : Comp_TiberiumNetworkStructure
    {
        //Control Data
        private bool recallHarvesters = false;

        //Refinery works with Comp_MechStation to handle harvesters
        public Comp_MechStation MechComp => parent.GetComp<Comp_MechStation>();
        public new CompProperties_TNSRefinery Props => (CompProperties_TNSRefinery)props;

        public bool CanBeRefinedAt => CompPower.PowerOn && !parent.IsBrokenDown() && !NetworkParts[0].Container.Full;

        public bool RecallHarvesters
        {
            get => recallHarvesters;
            private set => recallHarvesters = value;
        }

        public int HarvesterCount => MechComp.ConnectedMechs.Count;

        //Preferences
        private Zone_HarvestTiberium zoneHarvest;

        public Zone_HarvestTiberium HarvestTiberiumZone
        {
            get => zoneHarvest;
            set
            {
                zoneHarvest?.Delete();
                zoneHarvest = value;
            }
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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref recallHarvesters, "recallHarvesters");
            Scribe_References.Look(ref zoneHarvest, "zoneHarvest");
        }

        //Constructors
        private Harvester SpawnNewHarvester()
        {
            Harvester harvester = (Harvester)MechComp.MakeMech(Props.harvester);
            harvester.Rotation = parent.Rotation;
            harvester.SetMainRefinery((Building)parent, this, null);
            IntVec3 spawnLoc = parent.InteractionCell;
            return (Harvester)GenSpawn.Spawn(harvester, spawnLoc, parent.Map, parent.Rotation.Opposite);
        }

        //Data Handlers
        //Used to share harvesters between refineries
        public void AddHarvester(Harvester harvester)
        {
            if (MechComp.MainMechLink.Contains(harvester)) return;
            MechComp.TryAddMech(harvester);
            //parkingZone?.AssignNextSlot(harvester);
        }

        public void RemoveHarvester(Harvester harvester)
        {
            MechComp.RemoveMech(harvester);
            //parkingZone?.DismissSlot(harvester);
        }

        //Data Getters
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

        //
        public override string CompInspectStringExtra()
        {
            string str = base.CompInspectStringExtra();
            return str; //base.CompInspectStringExtra();
        }

        private Designator_ZoneAdd_HarvestTiberium zoneDesignator;
        public Designator_ZoneAdd_HarvestTiberium ZoneDesignator => zoneDesignator ??= new Designator_ZoneAdd_HarvestTiberium(this);

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

            //
            yield return ZoneDesignator;

            yield return new Command_Target
            {
                defaultLabel = "TR_CreateHarvestZoneProducer".Translate(),
                defaultDesc = "TR_CreateHarvestZoneProducerDesc".Translate(),
                icon = TiberiumContent.ZoneCreate_HarvestTiberium_Producer,
                targetingParams = RefineryTargetInfo.ForTiberiumProducers(),
                action = delegate(LocalTargetInfo target)
                {
                    if (target.HasThing && target.Thing is TiberiumProducer producer)
                    {
                        Zone_HarvestTiberium newZone = new Zone_HarvestTiberium(parent.Map.zoneManager);
                        newZone.ParentRefinery = this;
                        producer.FieldCells.ForEach(c => newZone.AddCell(c));
                        this.HarvestTiberiumZone = newZone;
                    }
                }
            };

            yield return new Command_Action
            {
                icon = TexButton.DeleteX,
                defaultLabel = "TR_DeleteRefineryZone".Translate(),
                defaultDesc = "TR_DeleteRefineryZoneDesc".Translate(),
                action = delegate
                {
                    HarvestTiberiumZone.Delete();
                    HarvestTiberiumZone = null;
                },
            };
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);
        }
    }

    public class CompProperties_TNSRefinery : CompProperties_TNS
    {
        public MechanicalPawnKindDef harvester;

        public CompProperties_TNSRefinery()
        {
            compClass = typeof(CompTNS_Refinery);
        }
    }
}
