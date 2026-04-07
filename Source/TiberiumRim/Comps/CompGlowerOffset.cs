using RimWorld;
using Verse;

namespace TR.Comps;

public class CompGlowerOffset : ThingComp
{
    private CompFlickable Flickable;
    public ThingWithComps glower;

    public CompProperties_GlowerOffset Props => (CompProperties_GlowerOffset)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_References.Look(ref glower, "glowerThing");
        if (Scribe.mode == LoadSaveMode.PostLoadInit) Flickable ??= glower.GetComp<CompFlickable>();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!respawningAfterLoad)
        {
            glower = (ThingWithComps)GenSpawn.Spawn(Props.glowerDef, parent.Position + parent.Rotation.FacingCell,
                parent.Map);
            Flickable = glower.GetComp<CompFlickable>();
            ToggleLight(false, true);
        }
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
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
        var turnOn = signal == "PowerTurnedOn" || signal == "FlickedOn" || signal == "Refueled" ||
                     signal == "ScheduledOn";
        var turnOff = signal == "PowerTurnedOff" || signal == "FlickedOff" || signal == "RanOutOfFuel" ||
                      signal == "ScheduledOff";
        ToggleLight(turnOn, turnOff);
    }
}

public class CompProperties_GlowerOffset : CompProperties
{
    public ThingDef glowerDef;

    public CompProperties_GlowerOffset()
    {
        compClass = typeof(CompGlowerOffset);
    }
}