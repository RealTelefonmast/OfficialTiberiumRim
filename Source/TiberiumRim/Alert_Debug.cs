using System.Text;
using RimWorld;
using TeleCore.Rendering.Particles;
using TR.Components;
using TR.TiberiumEnvironment;
using Verse;

namespace TR;

public class Alert_Debug : Alert_Critical
{
    public Alert_Debug()
    {
        defaultLabel = "[DEBUG INFO]";
    }

    public override TaggedString GetExplanation()
    {
        var particles = Find.CurrentMap.GetComponent<MapComponent_Particles>();
        var tiberium = Find.CurrentMap.GetComponent<MapComponent_Tiberium>();
        var mapinfo = tiberium.TiberiumInfo;
        var sb = new StringBuilder();
        sb.AppendLine("Current Particles: " + particles.SavedParticles.Count);
        sb.AppendLine("Total AllProducers: " + tiberium.StructureInfo.AllProducers.Count);
        var TibCount = tiberium.TiberiumInfo.TotalCount;
        sb.AppendLine("Total Tiberium: " + TibCount);
        sb.AppendLine("Total Cells: " + tiberium.TiberiumInfo.TotalCount);
        sb.AppendLine("Active percent: " + tiberium.TiberiumInfo.Coverage.ToStringPercent());
        sb.AppendLine("MapInfo:\n Valuables: " + mapinfo.TiberiumCrystals[HarvestType.Valuable].Count + " - " +
                      mapinfo.TiberiumCrystalTypes[HarvestType.Valuable].Count + " types" + "\n Unvaluables: " +
                      mapinfo.TiberiumCrystals[HarvestType.Unvaluable].Count + " - " +
                      mapinfo.TiberiumCrystalTypes[HarvestType.Unvaluable].Count + " types");
        sb.AppendLine("Trackers: " + tiberium.PollutionInfo.AllComps.Count);
        //sb.AppendLine("All Trackers: " + tiberium.PollutionInfo.PollutionTrackers.Count + "Tr/" +
        //Find.CurrentMap.regionGrid.allRooms.Count + "Rooms\n[" +
        //(tiberium.PollutionInfo.TotalPollution) + "][" + tiberium.PollutionInfo.OutsideCells + "][" + tiberium.PollutionInfo.OutsideSaturation +
        //"]");
        //sb.AppendLine("All RoomGroups: " + tiberium.PollutionInfo.RoomGroups.Count + "/" + Find.CurrentMap.regionGrid.allRooms.Select(r => r.Group).Distinct().Count());
        return sb.ToString();
    }

    public override AlertReport GetReport()
    {
        if (DebugSettings.godMode)
            return AlertReport.Active;
        return false;
    }
}