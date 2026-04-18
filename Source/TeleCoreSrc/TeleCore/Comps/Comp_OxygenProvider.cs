using TeleCore.Defs;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Comps;

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
