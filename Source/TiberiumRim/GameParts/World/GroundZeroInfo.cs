using TR.Interfaces;
using TR.WorldInfos;
using Verse;

namespace TR;

public class GroundZeroInfo : WorldInformation
{
    private Thing groundZeroThing;

    private IGroundZero MainGroundZero;

    public GroundZeroInfo(RimWorld.Planet.World world) : base(world)
    {
    }

    public bool HasGroundZero => MainGroundZero != null;

    public override void ExposeData()
    {
        Scribe_References.Look(ref groundZeroThing, "gzThing");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) MainGroundZero = GetGroundZeroAfterLoad();
    }

    private IGroundZero GetGroundZeroAfterLoad()
    {
        return (IGroundZero)groundZeroThing;
    }

    public bool IsGroundZero(IGroundZero groundZero)
    {
        return MainGroundZero == groundZero;
    }

    public void TryRegisterGroundZero(IGroundZero groundZero)
    {
        if (MainGroundZero != null) return;
        MainGroundZero = groundZero;
        groundZeroThing = MainGroundZero.GZThing;
    }
}