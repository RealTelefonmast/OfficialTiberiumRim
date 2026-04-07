using Verse;

namespace TR.Data.Scrin;

public enum PortalType
{
    Drones,
    Reinforcement
}

public static class GenPortal
{
    public static ScrinPortal MakeScrinPortal()
    {
        return (ScrinPortal) ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ScrinPortal"));
    }

    public static ScrinPortal SpawnPortal(IntVec3 cell, Map map)
    {
        return (ScrinPortal)GenSpawn.Spawn(MakeScrinPortal(), cell, map);
    }

    public static ScrinPortal SpawnDronePortal(IntVec3 cell, Map map)
    {
        //TODO: make portal with stuff
        ScrinPortal portal = MakeScrinPortal();
        portal.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("ScrinDrone"), Faction.OfPlayer));
        portal.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("ScrinDrone"), Faction.OfPlayer));
        portal.Add(PawnGenerator.GeneratePawn(PawnKindDef.Named("ScrinDrone"), Faction.OfPlayer));
        portal.PortalSetup(1000, 400);
        return (ScrinPortal) GenSpawn.Spawn(portal, cell, map);
    }
}