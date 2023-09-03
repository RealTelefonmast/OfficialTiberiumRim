using System.Text;
using RimWorld;
using Verse;

namespace TR
{
    public class Alert_Debug : Alert_Critical
    {
        public Alert_Debug()
        {
            defaultLabel = "[DEBUG INFO]";
        }

        public override TaggedString GetExplanation()
        {
            MapComponent_Tiberium tiberium = Find.CurrentMap.GetComponent<MapComponent_Tiberium>();
            TiberiumMapInfo mapinfo = tiberium.TiberiumInfo;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Total AllProducers: " + tiberium.NaturalTiberiumStructureInfo.AllProducers.Count);
            int TibCount = tiberium.TiberiumInfo.TotalCount;
            sb.AppendLine("Total Tiberium: " + TibCount);
            sb.AppendLine("Total Cells: " + tiberium.TiberiumInfo.TotalCount);
            sb.AppendLine("Active percent: " + tiberium.TiberiumInfo.InfestationPercent.ToStringPercent());
            //sb.AppendLine($"Networks: {tiberiumNetworkMaster?.?.Count}");
            sb.AppendLine("MapInfo:\n Valuables: " + mapinfo.TiberiumCrystals[HarvestType.Valuable].Count + " - " +
                          mapinfo.TiberiumCrystalTypes[HarvestType.Valuable].Count + " types" + "\n Unvaluables: " +
                          mapinfo.TiberiumCrystals[HarvestType.Unvaluable].Count + " - " +
                          mapinfo.TiberiumCrystalTypes[HarvestType.Unvaluable].Count + " types");
            return sb.ToString();
        }

        public override AlertReport GetReport()
        {
            if (DebugSettings.godMode)
            {
                return AlertReport.Active;
            }
            return false;
        }
    }
}
