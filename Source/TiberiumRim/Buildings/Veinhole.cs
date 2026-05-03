using System.Collections.Generic;
using Verse;

namespace TR;

public class Veinhole : TiberiumProducer
{
    private const int hubRadius = 70;

    //
    private Environment.Veinholes.VeinholeSystem _system;
    private readonly List<Thing> boundHubs = new();

    private int nutrients = 0;
    private int ticksToEgg;
    private int ticksToHub;
    
    //
    public Comp_AnimationRenderer AnimationComp { get; private set; }
    public Environment.Veinholes.VeinholeSystem System => _system;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        ResetEggTimer();
        ResetHubTimer();
        //
        _system = new Environment.Veinholes.VeinholeSystem(this);
        _system.Init();

        //Shake the camera!
        Find.CameraDriver.shaker.DoShake(0.2f);
        base.SpawnSetup(map, respawningAfterLoad);

        AnimationComp = GetComp<Comp_AnimationRenderer>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksToHub, "hubTicks");
        Scribe_Values.Look(ref ticksToEgg, "eggTicks");
        Scribe_Deep.Look(ref _system, "livingNetwork");
    }

    public override void Tick()
    {
        base.Tick();
        if (ticksToHub == 0) SpawnHub();
        if (ticksToEgg == 0) SpawnEgg();
        _system.Tick();
    }

    private void TryConsume(WrappedCorpse corpse)
    {
        _system.Notify_Consumed(corpse);
    }
    
    /*
     *     private void SpawnHub()
    {
        var action = delegate(IntVec3 c)
        {
            if (c.SupportsTiberiumTerrain(Map))
                Map.terrainGrid.SetTerrain(c, TiberiumCrystal.supportsTerrain.RandomElement().TerrainOutcome);
        };
        var flood = new TiberiumFloodInfo(Map, null, action);
        var end = GenRadial.RadialCellsAround(Position, 56, false).RandomElement();
        flood.TryMakeConnection(out var cells, Position, end);

        var hub = GenSpawn.Spawn(ThingDef.Named("VeinHub"), end, Map);
        boundHubs.Add(hub);
        ResetHubTimer();
    }

    public void RemoveHub(VeinHub hub)
    {
        if (boundHubs.Contains(hub))
            boundHubs.Remove(hub);
    }

    private void SpawnEgg()
    {
        var cell = FieldCells.RandomElement();

        GenSpawn.Spawn(ThingDef.Named("VeinEgg"), cell, Map);
        ResetEggTimer();
    }

    private void ResetHubTimer()
    {
        ticksToHub = (int)(GenDate.TicksPerDay * TRUtils.Range(3f, 7f));
    }

    private void ResetEggTimer()
    {
        ticksToEgg = (int)(GenDate.TicksPerDay * TRUtils.Range(1f, 3f));
    }

     */

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos()) yield return gizmo;

        yield return new Command_Action
        {
            defaultLabel = "Spawn Hub",
            action = () => _system.TrySpreadHub()
        };
    }
}