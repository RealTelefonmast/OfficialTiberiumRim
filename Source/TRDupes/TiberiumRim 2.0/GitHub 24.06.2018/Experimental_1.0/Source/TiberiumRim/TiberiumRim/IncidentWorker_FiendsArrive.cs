using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class IncidentWorker_FiendsArrive : IncidentWorker
    {
        IntVec3 cell = IntVec3.Invalid;

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            Thing Tree = map.listerThings.AllThings.Find((Thing x) => x.def == TiberiumDefOf.BlossomTree_TBNS);
            if (Tree != null)
            {
                return true;
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 intVec, map, CellFinder.EdgeRoadChance_Animal, null))
            {
                return false;
            }

            Rot4 rot = Rot4.FromAngleFlat((map.Center - intVec).AngleFlat);
            float points = parms.points;

            List<Pawn> list = GenerateFiends(parms.points, parms.target.Tile);
            foreach (Pawn p in list)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, map, 10, null);
                GenSpawn.Spawn(p, loc, map, rot, false);

                IntVec3 pos = map.listerThings.AllThings.Find((Thing x) => x.def == TiberiumDefOf.BlossomTree_TBNS).Position.RandomAdjacentCell8Way();

                if (Rand.Chance(0.15f))
                {
                    p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, true, false, FindPawnTarget(p));
                }
                else
                {
                    p.jobs.TryTakeOrderedJob(new Job(JobDefOf.GotoWander, pos));
                }
            }
            Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(cell, map, false), null);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }

        private Pawn FindPawnTarget(Pawn pawn)
        {
            return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), 3.40282347E+38f, true);
        }

        public List<Pawn> GenerateFiends(float points, int tile)
        {
            List<Pawn> pawnList = new List<Pawn>();
            float pointsLeft = points;
            while(pointsLeft > 0)
            {
                if (SelectCreature(pointsLeft, tile, out TiberiumKindDef def))
                {
                    pointsLeft -= def.combatPower;
                    Pawn pawn = null;
                    PawnGenerationRequest request = new PawnGenerationRequest(def, null, PawnGenerationContext.NonPlayer, tile, false, false, false, false, true, false, 1f, false, true, true);
                    pawn = PawnGenerator.GeneratePawn(request);
                    pawnList.Add(pawn);
                }
            }
            return pawnList;
        }

        public bool SelectCreature(float points, int tile, out TiberiumKindDef result)
        {
            return (from k in DefDatabase<TiberiumKindDef>.AllDefs
                    where k.RaceProps.Animal && !k.notFiend
                    select k).TryRandomElementByWeight((TiberiumKindDef k) => ManhunterPackIncidentUtility.ManhunterAnimalWeight(k, points), out result);
        }
    }      
}