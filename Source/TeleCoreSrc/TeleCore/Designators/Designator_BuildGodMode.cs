//using Multiplayer.API;

using RimWorld;
using Verse;

namespace TeleCore.Designators;

public class Designator_BuildGodMode : Designator_Build
{
    //public SubMenuExtension DefExtension => entDef.SubMenuExtension();

    public Designator_BuildGodMode(BuildableDef entDef) : base(entDef)
    {
    }

    /*[SyncWorker]
    private static void SyncBuildGodMode(SyncWorker sync, ref Designator_BuildGodMode type)
    {
        if (sync.isWriting)
        {
            sync.Write(type.entDef);
        }
        else
        {
            var entDef = sync.Read<BuildableDef>();
            type = new Designator_BuildGodMode(entDef);
        }
    }*/

    //[SyncMethod]
    public override void DesignateSingleCell(IntVec3 c)
    {
        var thing = ThingMaker.MakeThing((ThingDef)entDef, stuffDef);
        GenSpawn.Spawn(thing, c, Map, placingRot);
        FleckMaker.ThrowMetaPuffs(GenAdj.OccupiedRect(c, placingRot, entDef.Size), Map);

        if (entDef.PlaceWorkers == null) return;
        foreach (var placeWorker in entDef.PlaceWorkers) placeWorker.PostPlace(Map, entDef, c, placingRot);
    }
}