using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumCrater : TiberiumProducer
    {
        private Building researchCrane;

        public override string LabelCap => IsGroundZero ? base.LabelCap + " (GZ)" : base.LabelCap;

        public override bool ShouldSpawnTiberium => InitialResearchDone && !ResearchBound && base.ShouldSpawnTiberium;

        public bool IsGroundZero { get; set; }
        public bool ShouldSpawnSpore => !ResearchBound && def.spore != null && IsMature;

        public virtual float GrowthRadius => def.spawner.growRadius * GroundZeroFactor * base.GrowthRadius;
        public float GroundZeroFactor => IsGroundZero ? 2f : 1f;

        protected override int MutationTicks => (int)(GenDate.TicksPerDay * (def.daysToMature * GroundZeroFactor / 2));

        //public bool ShouldEvolve => evolvesTo != null && ticksToEvolution <= 0;
        private bool ResearchBound => (researchCrane ??= (Building)Position.GetFirstThing(Map, TiberiumDefOf.TiberiumResearchCrane)) != null;

        //Since a placeworker is too much of a pain for some reason, the crater simply wont spawn tib until the main research is done
        private bool InitialResearchDone => TiberiumDefOf.MineralAnalysis.IsFinished;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            Log.Message("-Spawning Crater");
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) return;
            Log.Message("-Base SpawnSet done");

            //Try Set GroundZero
            WorldTiberiumComp.SetGroundZero(this);

            Log.Message("-Set GZ done");
            //TODO: Something fucks up here with cellpaths
            //TODO: Just fucking yeet this entire crap because its dumb anyway and wastes resources
            //Init CellPaths for Tiberium growth
            /*
            bool Validator(IntVec3 c) => c.InBounds(map) && c.Standable(map);
            bool EndCon(IntVec3 c) => Position.DistanceTo(c) > GrowthRadius;

            void Processor(IntVec3 c)
            {
                TiberiumComp.TiberiumInfo.SetForceGrowBool(c, true);
            }
            List<IntVec3> cells = new List<IntVec3>();
            var AdjacentCells = GenAdj.CellsAdjacent8Way(this).ToList();
            for (int i = 0; i < AdjacentCells.Count - 1; i++)
            {
                if (i % 3 == 0)
                {
                    new CellPath(map, AdjacentCells[i], IntVec3.Invalid, Position, GrowthRadius + 1.5f, Validator, EndCon, Processor).CreatePath();
                }
            }
            Log.Message("-CellPaths done");
            */
            if (def.scatterTiberium)
                DoMeteoriteImpact();

            Log.Message("-Meteorite Impact done");
        }

        private void GrowBlossomTree()
        {
            /*
            if (!ShouldSpawnBlossomTree) return;
            IntVec3 pos = GenRadial.RadialCellsAround(Position, areaMutator.MaxRadius, areaMutator.MaxRadius + 2).RandomElement();
            var list = new List<TiberiumProducerDef>() { TiberiumDefOf.BlossomTree, TiberiumDefOf.BlueBlossomTree };
            blossomTree = (TiberiumProducer)GenSpawn.Spawn(list.RandomElement(), pos, Map);
            */
        }

        private bool TrySpawnBlossomSpore()
        {
            if (!ShouldSpawnSpore) return false;
            if (!TiberiumComp.BlossomInfo.TryGetNewBlossom(out IntVec3 dest)) return false;
            //TODO: Blossom Tree Spore
            //var spore = GenTiberium.SpawnBlossomSpore(Position, dest, Map, def.spore.Blossom(), this);
            GenSpawn.Spawn(def.spore.Blossom(), dest, Map);
            Messages.Message("A blossom spore has appeared, and will fly to this position.", MessageTypeDefOf.NeutralEvent, false);
            //var let = LetterMaker.MakeLetter("Blossom Spore", "A blossom spore has appeared, and will fly to this position.", LetterDefOf.NeutralEvent, new LookTargets(dest, Map));
            //Find.LetterStack.ReceiveLetter(let);
            return true;
        }

        private void DoMeteoriteImpact()
        {
            bool Validator(IntVec3 c) => c.InBounds(Map) && c.Standable(Map) && GenTiberium.AllowsTiberiumAtFast(c, Map);
            void Action(IntVec3 c)
            {
                TiberiumCrystalDef crystalDef = TiberiumTypes.RandomElement();
                GenTiberium.SetTerrain(c, Map, crystalDef);
                if (!GenAdj.CellsAdjacent8Way(this).Contains(c) && Rand.Chance(0.45f))
                {
                    TiberiumCrystal crystal = GenTiberium.SpawnTiberium(c, Map, crystalDef, null);
                    if (crystal != null)
                        crystal.Growth = Rand.Range(0.35f, 0.75f);
                }
            }
            TerrainGenerator.RandomRootPatch(Position, Map, 3.9f, 8, Validator, Action).ToList();
        }


        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            if (IsGroundZero)
                sb.AppendLine("TR_GZProducer".Translate());
            if (ResearchBound)
                sb.AppendLine("TR_ResearchBound".Translate());
            if (DebugSettings.godMode)
            {
                sb.AppendLine("ShouldSpawnSpore: " + ShouldSpawnSpore);
            }
            sb.AppendLine(base.GetInspectString());
            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (!TiberiumTypes.EnumerableNullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Grow Blossom Tree",
                    action = delegate
                    {

                    }
                };
            }

            if (!TiberiumTypes.EnumerableNullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Send Spore",
                    action = delegate
                    {

                    }
                };
            }
        }
    }
}
