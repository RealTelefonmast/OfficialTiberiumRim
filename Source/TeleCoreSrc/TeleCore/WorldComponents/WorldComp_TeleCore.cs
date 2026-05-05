using RimWorld.Planet;
using TeleCore.Types.Exposables;
using TeleCore.Types.Utils;
using Verse;
using World = RimWorld.Planet.World;

namespace TeleCore.WorldComponents;

public class WorldComp_TeleCore : WorldComponent
{
    //Discovery
    internal DiscoveryTable discoveries;

    public WorldComp_TeleCore(World world) : base(world)
    {
        GenerateInfos();
        StaticData.Notify_NewTeleWorldComp(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        StaticData.ExposeStaticData();
        Scribe_Deep.Look(ref discoveries, "DiscoveryTable");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            GenerateInfos();
    }


    private void GenerateInfos()
    {
        discoveries ??= new DiscoveryTable();
    }
}