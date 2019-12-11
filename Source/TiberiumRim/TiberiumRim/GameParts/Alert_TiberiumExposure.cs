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

        private IEnumerable<Pawn> SickPawns
        {
            get
            {
                foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep)
                {
                    for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
                    {
                        Hediff diff = p.health.hediffSet.hediffs[i];
                        if (diff is Hediff_Crystallizing || diff.def == TRHediffDefOf.TiberiumExposure)
                        {
                            yield return p;
                            break;
                        }
                    }
                }
                yield break;
            }
        }

        //TODO: Add alert for player
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(SickPawns);
        }
    }
}
