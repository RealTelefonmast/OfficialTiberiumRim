using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class MapPawnInfo : MapInformation
    {
        public List<Pawn> TotalSickPawns = new List<Pawn>();
        public List<Pawn> TotalSickColonists = new List<Pawn>();
        public Dictionary<HediffDef, List<Pawn>> InfectedPawns = new Dictionary<HediffDef, List<Pawn>>();

        public MapPawnInfo(Map map) : base(map) { }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        //TODO: Pawn Registering For Alerts
        public void RegisterPawn(Pawn pawn, HediffDef def)
        {
            TotalSickPawns.Add(pawn);
            if(pawn.IsColonist)
                TotalSickColonists.Add(pawn);

            if (InfectedPawns.ContainsKey(def))
            {
                InfectedPawns[def].Add(pawn);
            }
            else
            {
                InfectedPawns.Add(def, new List<Pawn>() {pawn});
            }
        }

        public void DeregisterPawns(Pawn pawn)
        {
            TotalSickPawns.Remove(pawn);
            TotalSickColonists.Remove(pawn);
            foreach (var def in InfectedPawns.Keys)
            {
                InfectedPawns[def].Remove(pawn);
            }
        }
    }
}