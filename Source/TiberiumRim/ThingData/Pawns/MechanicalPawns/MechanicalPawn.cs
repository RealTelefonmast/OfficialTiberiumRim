using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.VFX.FX.Implementations;
using TR.Components;
using Verse;

namespace TR.ThingData.Pawns.MechanicalPawns;

public class MechanicalPawn : FXPawn
{
    protected Building parent;
    protected MechLink parentLink;

    public MechLink ParentMechLink
    {
        get => parentLink;
        set => parentLink = value;
    }

    public virtual Building ParentBuilding
    {
        get => parent;
        set => parent = value;
    }

    public MapComponent_Tiberium TiberiumManager => Map.GetComponent<MapComponent_Tiberium>();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (story == null) story = new Pawn_StoryTracker(this);
        if (Faction == Faction.OfPlayer)
        {
            if (playerSettings == null) playerSettings = new Pawn_PlayerSettings(this);
            if (drafter == null) drafter = new Pawn_DraftController(this);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref parent, "parent");
    }

    public override void Tick()
    {
        base.Tick();
    }

    public bool IsDamaged()
    {
        return Damage().Any();
    }

    public IEnumerable<Hediff> Damage()
    {
        return from x in health?.hediffSet?.GetHediffs<Hediff>()
            where x is Hediff_Injury || x is Hediff_MissingPart
            select x;
    }
}