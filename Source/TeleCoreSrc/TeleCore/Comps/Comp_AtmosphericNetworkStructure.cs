using TeleCore.CompProperties;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Comps;

public class Comp_AtmosphericNetworkStructure : Comp_Network
{
    private RoomComponent_Atmosphere atmosphericInt;

    //
    public INetworkPart OwnedAtmosPart { get; private set; }
    public PipeNetwork AtmosNetwork => OwnedAtmosPart.Network;

    public RoomComponent_Atmosphere AtmosRoom
    {
        get
        {
            if (atmosphericInt == null || atmosphericInt.Parent.IsDisbanded)
                atmosphericInt = AtmosphericSource.GetRoomComp<RoomComponent_Atmosphere>();
            return atmosphericInt;
        }
    }

    protected virtual Room AtmosphericSource => parent.GetRoom();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        OwnedAtmosPart = this[AtmosDefOf.AtmosphericNetwork];
    }

    public override void CompTick()
    {
        base.CompTick();
    }
}

public class CompProperties_ANS : CompProperties_Network
{
    public CompProperties_ANS()
    {
        compClass = typeof(Comp_AtmosphericNetworkStructure);
    }
}
