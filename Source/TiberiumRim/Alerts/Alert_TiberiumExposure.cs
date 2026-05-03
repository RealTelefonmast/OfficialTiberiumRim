using System.Collections.Generic;
using RimWorld;
using TR.Components;
using UnityEngine;
using Verse;

namespace TR;

public class Alert_TiberiumExposure : Alert_Critical
{
    public override Color BGColor => Color.green;

    //OutSource into 

    private List<Pawn> SickPawns
    {
        get
        {
            var total = new List<Pawn>();
            foreach (var map in Find.Maps)
                total.AddRange(map.GetComponent<MapComponent_Tiberium>().MapPawnInfo.TotalSickColonists);
            return total;
        }
    }

    public override void AlertActiveUpdate()
    {
        base.AlertActiveUpdate();
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