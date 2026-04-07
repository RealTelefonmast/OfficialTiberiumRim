using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Rooms;
using TeleCore.FlowCore;
using Verse;

namespace TeleCore.Atmosphere.Oxygen;

public class Comp_OxygenProvider : ThingComp
{
    public AtmosphericVolume Volume { get; set; }

    public CompEquippable Equippable { get; private set; }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        Equippable = parent.TryGetComp<CompEquippable>();
        Volume = new AtmosphericVolume(new FlowVolumeConfig<AtmosphericValueDef>
        {
            capacity = 0,
            area = 0,
            elevation = 0,
            height = 0
        });
    }
}