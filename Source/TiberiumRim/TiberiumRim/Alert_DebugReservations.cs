using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Alert_DebugReservations : Alert_Critical
    {
        public Alert_DebugReservations()
        {
            defaultLabel = "Debug: Reservations";
        }

        public override string GetExplanation()
        {
            MapComponent_Tiberium tiberium = Find.CurrentMap.GetComponent<MapComponent_Tiberium>();
            MapComponent_TNWManager tnwManager = Find.CurrentMap.GetComponent<MapComponent_TNWManager>();
            HarvesterReservationManager reservations = tnwManager.ReservationManager;
            TiberiumMapInfo mapinfo = tiberium.TiberiumInfo;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Harvesters: " + reservations.Reservations.Count);
            sb.AppendLine("Total Reserves " + reservations.ReservedTotal + "\n Valuable: " + reservations.ReservedTypes[HarvestType.Valuable] + "\n Unvaluable: " + reservations.ReservedTypes[HarvestType.Unvaluable]);
            sb.AppendLine("Current Pair: " + reservations.CurrentPair.ToString());
            return sb.ToString();
        }

        public override AlertReport GetReport()
        {
            if (DebugSettings.godMode || TiberiumRimSettings.settings.ShowMapCompAlert)
            {
                return AlertReport.Active;
            }
            return false;
        }
    }
}
