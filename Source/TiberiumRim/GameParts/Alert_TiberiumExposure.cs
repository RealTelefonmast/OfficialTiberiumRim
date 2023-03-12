using System.Collections.Generic;
using RimWorld;
using TeleCore.Static;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Alert_TiberiumExposure : Alert_Critical
    {
        //public override
        public override Color BGColor => Color.green;

        public override string GetLabel() => "TR.Alert.Exposure".Translate();

        public override void AlertActiveUpdate()
        {
            base.AlertActiveUpdate();
        }

        //
        private List<Pawn> SickPawns
        {
            get
            {
                List<Pawn> total = StaticListHolder<Pawn>.RequestList("TR_SickPawnsTempList");
                total.Clear();
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
        
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(SickPawns);
        }
        
        public override TaggedString GetExplanation()
        {
            return base.GetExplanation();
        }
    }
}
