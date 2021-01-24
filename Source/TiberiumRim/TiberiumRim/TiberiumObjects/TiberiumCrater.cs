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
    public class TiberiumCrater : TiberiumProducer, IResearchCraneTarget, IGroundZero
    {
        public override string LabelCap => IsGroundZero ? base.LabelCap + " (GZ)" : base.LabelCap;

        public override bool ShouldSpawnTiberium => InitialResearchDone && base.ShouldSpawnTiberium;

        public bool IsGroundZero => TiberiumRimComp.GroundZeroInfo.IsGroundZero(this);
        public Thing GZThing => this;

        public bool ShouldSpawnSpore => def.spore != null && IsMature;
        public float GroundZeroFactor => IsGroundZero ? 2f : 1f;
        protected override float GrowthRadius => GroundZeroFactor * base.GrowthRadius;
        protected override int MutationTicks => (int)(GenDate.TicksPerDay * (def.daysToMature * GroundZeroFactor / 2));

        private Building researchCrane;
        public Building ResearchCrane => researchCrane ??= (Building)Position.GetFirstThing(Map, TiberiumDefOf.TiberiumResearchCrane);
        public bool ResearchBound => !ResearchCrane.DestroyedOrNull();

        //Since a placeworker is too much of a pain for some reason, the crater simply wont spawn tib until the main research is done
        private bool InitialResearchDone => TiberiumDefOf.MineralAnalysis.IsFinished;

        private CustomParticleSystem system;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) return;

            //Try Set GroundZero
            TiberiumRimComp.GroundZeroInfo.TryRegisterGroundZero(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public void PassOnGZTitle()
        {
            return;
        }

        private void GrowBlossomTree()
        {
            /*
            if (!ShouldSpawnBlossomTree) return;
            IntVec3 pos = GenRadial.RadialCellsAround(Position, areaMutator.MaxRadius, areaMutator.MaxRadius + 2).RandomElement();
            var list = new List<TiberiumProducerDef>() { TiberiumDefOf.Blossom, TiberiumDefOf.BlueBlossomTree };
            blossom = (TiberiumProducer)GenSpawn.Spawn(list.RandomElement(), pos, Map);
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


        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            if (IsGroundZero)
                sb.AppendLine("TR_GZProducer".Translate());

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

            if (!DebugSettings.godMode) yield break;

            if (!TiberiumTypes.EnumerableNullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Grow Blossom Tree",
                    action = delegate
                    {
                        areaMutator.CreateBlossom();
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
