using RimWorld;
using TiberiumRim;
using TR.Designators;
using Verse;
using EVASignal = TR.EVA.EVASignal;
using GameComponent_EVA = TR.EVA.GameComponent_EVA;

namespace TR.Scrin;

public class Designator_ScrinLanding : Designator_Target
{
    public bool activated;

    public Designator_ScrinLanding()
    {
        defaultLabel = "DEBUG: Scrin Landing";
        defaultDesc = "Scrin lands here now";
        icon = TiberiumContent.ScrinIcon;
        useMouseIcon = false;
        soundSucceeded = SoundDefOf.Click;
        mustBeUsed = true;

        targeterMat = TiberiumContent.NodNukeTargeter;
        size = 6;
    }

    public override bool MustStaySelected => base.MustStaySelected && !activated;

    public override void Selected()
    {
        base.Selected();
        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.SelectDestination, null);
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        var skyfaller = SkyfallerMaker.MakeSkyfaller(TRFDefOf.ScrinDronePlatformIncoming, TRFDefOf.ScrinDronePlatform);
        var platform = (DronePlatform)ThingMaker.MakeThing(TRFDefOf.ScrinDronePlatform);
        platform.SetFactionDirect(Faction.OfPlayer);
        SkyfallerMaker.SpawnSkyfaller(TRFDefOf.ScrinDronePlatformIncoming, platform, c, Map);
        activated = true;
    }

    public override bool CanRemainSelected()
    {
        return !activated;
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return base.CanDesignateCell(loc).Accepted && loc.Standable(Map);
    }
}