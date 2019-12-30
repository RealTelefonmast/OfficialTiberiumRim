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
            sb.AppendLine("Total Producers: " + tiberium.StructureInfo.Producers.Count);
            int TibCount = tiberium.TiberiumInfo.TotalCount;
            sb.AppendLine("Total Tiberium: " + TibCount);
            sb.AppendLine("Total Cells: " + tiberium.TiberiumInfo.TotalCount);
            sb.AppendLine("Active percent: " + tiberium.TiberiumInfo.Coverage.ToStringPercent());
            sb.AppendLine("Networks: " + tnwManager.Networks.Count);
            sb.AppendLine("MapInfo:\n Valuables: " + mapinfo.TiberiumCrystals[HarvestType.Valuable].Count + " - " + mapinfo.TiberiumCrystalTypes[HarvestType.Valuable].Count + " types"+ "\n Unvaluables: " + mapinfo.TiberiumCrystals[HarvestType.Unvaluable].Count + " - " + mapinfo.TiberiumCrystalTypes[HarvestType.Unvaluable].Count + " types");
            sb.AppendLine("Cur Time:" + MapComponent_Tiberium.CURBIGGESTTIME);
            sb.AppendLine("Max Producer Time: " + TiberiumProducer.BIGGESTTIME);
            sb.AppendLine("Max MapComp Time: " + MapComponent_Tiberium.BIGGESTTIME);
            sb.AppendLine("Max Total Time: " + MapComponent_Tiberium.LASTBIGGESTTIME + " after " + MapComponent_Tiberium.TICKSSINCELASTTIME);
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
