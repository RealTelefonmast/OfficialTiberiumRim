using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TR.Factions.GDI;

public class IonCannonCore : ThingWithComps
{
    private List<PowerBeam> currentBeams = new();

    private bool Finalized = false;
    private int maxBeams = 8;
    private float radius = 25f;

    // Stage One


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void Tick()
    {
        base.Tick();
    }
}