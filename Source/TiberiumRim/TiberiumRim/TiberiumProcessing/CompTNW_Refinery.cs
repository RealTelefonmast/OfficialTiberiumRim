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
        public List<Harvester> harvesters = new List<Harvester>();

        public bool recallHarvesters = false;

        public new RefineryProperties Props => (RefineryProperties)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref recallHarvesters, "recallHarvesters");

        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                harvesters.Add(SpawnNewHarvester());
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            for(int i= 0; i < harvesters.Count; i++)
            {
                Harvester harvester = harvesters[i];
                if (!harvester.DestroyedOrNull())
                {
                    harvester.UpdateRefineries(null, this);
                    if (mode != DestroyMode.Deconstruct)
                    {
                        Messages.Message("TR_RefineryLost".Translate(), parent, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
            base.PostDestroy(mode, previousMap);
        }

        public override IEnumerable<IntVec3> InnerConnectionCells
        {
            get
            {
                CellRect rect = parent.OccupiedRect();
                Rot4 rot = parent.Rotation;
                if (rot == Rot4.North)
                {
                    rect.minZ += 1;
                }
                else if (rot == Rot4.East)
                {
                    rect.minX += 1;
                }
                else if (rot == Rot4.South)
                {
                    rect.maxZ -= 1;
                }
                else
                {
                    rect.maxX -= 1;
                }
                return rect.Cells;
            }
        }

        private Harvester SpawnNewHarvester()
        {
            Harvester harvester = (Harvester)PawnGenerator.GeneratePawn(Props.harvester, parent.Faction);
            harvester.ageTracker.AgeBiologicalTicks = 0;
            harvester.ageTracker.AgeChronologicalTicks = 0;
            harvester.Rotation = parent.Rotation;
            harvester.SetMainRefinery((Building)parent);
            IntVec3 spawnLoc = parent.InteractionCell;
            return (Harvester)GenSpawn.Spawn(harvester, spawnLoc, parent.Map, parent.Rotation.Opposite);
        }

        public bool CanBeRefinedAt
        {
            get
            {
                return CompPower.PowerOn && !parent.IsBrokenDown() && !Container.CapacityFull;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = recallHarvesters ? "TR_RefineryAllow".Translate() : "TR_RefineryReturn".Translate(),
                defaultDesc = "TR_RefineryReturnDesc".Translate(),
                icon = recallHarvesters ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
                action = delegate
                {
                    recallHarvesters = !recallHarvesters;
                },
            };
        }
    }

    public class RefineryProperties : CompProperties_TNW
    {
        public MechanicalPawnKindDef harvester;
        public float flowAmount = 0.5f;

        public RefineryProperties()
        {
            compClass = typeof(CompTNW_Refinery);
        }
    }
}
