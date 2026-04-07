using TeleCore.Atmosphere.Defs;
using TeleCore.Utility;
using Verse;

namespace TeleCore.Atmosphere.Comps;

/// <summary>
/// Thing component that marks a building as a source of an atmospheric value.
/// </summary>
public class Comp_AtmosphericSource : ThingComp
{
    public CompProperties_AtmosphericSource Props => (CompProperties_AtmosphericSource) props;
    public Thing Thing => parent;
    public Room Room => parent.GetRoomIndirect();

    //
    public AtmosphericValueDef AtmosphericDef => Props.atmosphericDef;
    public int PushInterval => Props.pushInterval;
    public int PushAmount => Props.pushAmount;

    public virtual bool IsActive => true;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        // Thing.Map.GetMapInfo<AtmosphericMapInfo>().Notify_AddSource(this);
    }

    public override void PostDeSpawn(Verse.Map map)
    {
        // map.GetMapInfo<AtmosphericMapInfo>().Notify_RemoveSource(this);
        base.PostDeSpawn(map);
    }
}
