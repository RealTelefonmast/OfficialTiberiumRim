using RimWorld;
using TeleCore.Logging;
using TeleCore.ThingComps.Props;
using Verse;

namespace TeleCore.ThingComps;

public class CompGlowerOffset : ThingComp
{
    public ThingWithComps glower;

    private CompFlickable Flickable;

    public CompProperties_GlowerOffset Props => (CompProperties_GlowerOffset)base.props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref glower, "glowerThing");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (glower == null || !glower.Spawned)
            {
                TLog.Warning("Glower was not spawned after respawn!");
                var existing = parent.Position.GetFirstThing(parent.Map, Props.glowerDef);
                existing?.DeSpawn();
                glower = (ThingWithComps)GenSpawn.Spawn(Props.glowerDef, parent.Position + parent.Rotation.FacingCell, parent.Map);
            }
            Flickable ??= glower.GetComp<CompFlickable>();
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            glower = (ThingWithComps)GenSpawn.Spawn(Props.glowerDef, parent.Position + parent.Rotation.FacingCell, parent.Map);
            Flickable = glower.GetComp<CompFlickable>();
            ToggleLight(false, true);
        }
    }

    public override void PostDeSpawn(Verse.Map map)
    {
        base.PostDeSpawn(map);
        glower.DeSpawn();
    }

    private void ToggleLight(bool turnOn, bool turnOff)
    {
        if (Flickable == null) return;
        if (Flickable.SwitchIsOn && turnOff)
            Flickable.DoFlick();
        if (!Flickable.SwitchIsOn && turnOn)
            Flickable.DoFlick();
    }

    public override void ReceiveCompSignal(string signal)
    {
        bool turnOn = signal == "PowerTurnedOn" || signal == "FlickedOn" || signal == "Refueled" || signal == "ScheduledOn";
        bool turnOff = signal == "PowerTurnedOff" || signal == "FlickedOff" || signal == "RanOutOfFuel" || signal == "ScheduledOff";
        ToggleLight(turnOn, turnOff);
    }
}