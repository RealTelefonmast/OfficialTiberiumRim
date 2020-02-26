using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Alert_TiberiumExposure : Alert_Critical
    {
        protected override Color BGColor => Color.green;

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
                    total.AddRange(map.GetComponent<MapComponent_Tiberium>().InfectionInfo.TotalSickColonists);
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
                            diff => diff is Hediff_Crystallizing || diff.def == TRHediffDefOf.TiberiumExposure));
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
