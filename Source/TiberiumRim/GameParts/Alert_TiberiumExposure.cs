using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Alert_TiberiumExposure : Alert_Critical
    {
        //public override
        public override Color BGColor => Color.green;

        public override void AlertActiveUpdate()
        {
            base.AlertActiveUpdate();
        }

        //OutSource into 

        private List<Pawn> SickPawns
        {
            get
            {
                List<Pawn> total = new List<Pawn>();
                foreach (var map in Find.Maps)
                {
                    total.AddRange(map.Tiberium().MapPawnInfo.TotalSickColonists);
                }
                return total;
            }
        }

        /*
        private IEnumerable<Pawn> SickPawns
        {
            get
            {
                return PawnsFinder
                    .AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep.Where(p =>
                        Enumerable.Any(p.health.hediffSet.hediffs,
                            diff => diff is Hediff_Crystallizing || diff.props == TRHediffDefOf.TiberiumExposure));
            }
        }
        */

        //TODO: Add alert for player
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(SickPawns);
        }
    }
}
