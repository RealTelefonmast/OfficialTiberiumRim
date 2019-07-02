using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Alert_Debug : Alert_Critical
    {
        public Alert_Debug()
        {
            defaultLabel = "Debug: Mapcomp Info";
        }

        public override string GetExplanation()
        {
            MapComponent_Particles particles = Find.CurrentMap.GetComponent<MapComponent_Particles>();
            MapComponent_Tiberium tiberium = Find.CurrentMap.GetComponent<MapComponent_Tiberium>();
            TiberiumMapInfo mapinfo = tiberium.TiberiumInfo;
            MapComponent_TNWManager tnwManager = Find.CurrentMap.GetComponent<MapComponent_TNWManager>();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Current Particles: " + particles.SavedParticles.Count);
            sb.AppendLine("Total Producers: " + tiberium.TiberiumProducers.Count);
            int TibCount = tiberium.TiberiumInfo.TotalCount;
            sb.AppendLine("Total Tiberium: " + TibCount);
            int AffectedCount = 0;
            int TotalCount = tiberium.AffectedCells.Count;
            sb.AppendLine("Total Cells: " + TotalCount);
            sb.AppendLine("Active percent: " + ((float)AffectedCount / (float)TotalCount).ToStringPercent());
            sb.AppendLine("Networks: " + tnwManager.Networks.Count);
            sb.AppendLine("MapInfo:\n Valuables: " + mapinfo.TiberiumCrystals[HarvestType.Valuable].Count + " - " + mapinfo.TiberiumCrystalTypes[HarvestType.Valuable].Count + " types"+ "\n Unvaluables: " + mapinfo.TiberiumCrystals[HarvestType.Unvaluable].Count + " - " + mapinfo.TiberiumCrystalTypes[HarvestType.Unvaluable].Count + " types");
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
