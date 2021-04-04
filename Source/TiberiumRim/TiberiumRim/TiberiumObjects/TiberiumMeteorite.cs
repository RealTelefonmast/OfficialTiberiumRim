using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumMeteorite : TRBuilding, IRadiationLeaker, IResearchCraneTarget
    {
        private TiberiumProducerDef craterDef;
        private Building researchCrane;
        private int ticksLeft;

        public Building ResearchCrane => researchCrane ??= (Building)Position.GetFirstThing(Map, TiberiumDefOf.TiberiumResearchCrane);
        public bool ResearchBound => !ResearchCrane.DestroyedOrNull();

        //Since a placeworker is too much of a pain for some reason, the crater simply wont spawn tib until the main research is done
        private bool InitialResearchDone => TiberiumDefOf.MineralAnalysis.IsFinished;

        public bool CauseLeak => InitialResearchDone;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref craterDef, "craterDef");
            Scribe_References.Look(ref researchCrane, "researchCrane");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                TRUtils.Tiberium().Notify_TiberiumArrival(map);

                //Initial Setup
                craterDef = (TiberiumProducerDef)new List<WeightedThing>()
                {
                    new WeightedThing(TiberiumDefOf.TiberiumCraterGreen,0.66f),
                    new WeightedThing(TiberiumDefOf.TiberiumCraterBlue,0.33f),
                    new WeightedThing(TiberiumDefOf.TiberiumCraterHybrid,0.22f),
                    //new WeightedThing(TiberiumDefOf.RedTiberiumShard,0.01f)
                }.RandomElementByWeight(s => s.weight).thing;
                ticksLeft = (int)(TRUtils.Range(1f, 2f) * GenDate.TicksPerDay);
                DoMeteoriteImpact();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //Crack Open
            CrackOpen();
        }

        private void CrackOpen()
        {
            if (ResearchBound)
            {
                ResearchCrane.Destroy(DestroyMode.KillFinalize);
            }

            Map map = Map;
            IntVec3 pos = Position;
            base.DeSpawn();
            GenSpawn.Spawn(craterDef, pos, map, WipeMode.VanishOrMoveAside);
        }

        public override void TickRare()
        {
            base.TickRare();
            //if (!ResearchBound) return;
            if (!InitialResearchDone) return;
            if (ticksLeft <= 0)
            {
                CrackOpen();
            }
            if (ticksLeft >= 0)
                ticksLeft -= 250;
        }

        private void DoMeteoriteImpact()
        {
            foreach (var cell in this.OccupiedRect())
            {
                Map.terrainGrid.SetTerrain(cell, craterDef.tiberiumFieldRules.TiberiumTypes.RandomElement().conversions.ConversionForStone().toTerrain);
            }
            bool Validator(IntVec3 c) => c.InBounds(Map) && c.Standable(Map) && !c.GetTerrain(Map).IsWater && GenTiberium.AllowsTiberiumAtFast(c, Map);
            void Action(IntVec3 c)
            {
                TiberiumCrystalDef crystalDef = (TiberiumCrystalDef)craterDef.tiberiumFieldRules.crystalOptions.RandomElement().thing;
                GenTiberium.SetTerrain(c, Map, crystalDef);

                if (!GenAdj.CellsAdjacent8Way(this).Contains(c) && Rand.Chance(0.20f))
                {
                    TiberiumCrystal crystal = GenTiberium.Spawn(c, Map, crystalDef); //SpawnTiberium(c, Map, crystalDef);
                    if (crystal == null) return;
                    crystal.PreSpawnSetup(null, 0, true);
                    crystal.Growth = Rand.Range(0.75f, 1f);
                }
            }
            TerrainGenerator.RandomRootPatch(Position, Map, 3.9f, 8, Validator, Action);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            if (ResearchBound)
                sb.AppendLine("TR_ResearchBound".Translate());
            if (InitialResearchDone)
            {
                sb.AppendLine("TR_Supercritical".Translate());
                sb.AppendLine("TR_TimeLeft".Translate(ticksLeft.ToStringTicksToPeriod(true, false, true, false)));
            }

            return sb.ToString().TrimStart().TrimEnd();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (!DebugSettings.godMode) yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Finish Research",
                action = delegate
                {
                    TiberiumDefOf.MineralAnalysis.tasks.ForEach(t => t.Debug_Finish());
                }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Crack Open",
                action = CrackOpen
            };
        }
    }
}
