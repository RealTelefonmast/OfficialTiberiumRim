using System.Collections.Generic;
using TeleCore.CompProperties;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.PlaceWorkers;

public class PlaceWorker_AtmosphericVent : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 pos, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        base.DrawGhost(def, pos, rot, ghostCol, thing);
        var intakeCell = VentCell(def, pos, rot);
        GenDraw.DrawFieldEdges(new List<IntVec3>
        {
            intakeCell
        }, TColor.BlueHighlight);
    }

    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 pos, Rot4 rot, Verse.Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var intakeCell = pos;
        if (checkingDef is ThingDef tDef)
            intakeCell = VentCell(tDef, pos, rot);

        if (intakeCell.GetEdifice(map) != null)
            return "TELE.PassiveVent.PlacingBlocked".Translate();
        return base.AllowsPlacing(checkingDef, pos, rot, map, thingToIgnore, thing);
    }

    private static IntVec3 VentCell(ThingDef def, IntVec3 pos, Rot4 rot)
    {
        var ventComp = def.GetCompProperties<CompProperties_ANS_Vent>();
        if (ventComp != null)
        {
            return ventComp.GetIntakePos(pos, rot);
        }
        return pos;
    }
}