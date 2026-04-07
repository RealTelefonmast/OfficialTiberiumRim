using System.Collections.Generic;
using TeleCore.Events;
using Verse;

namespace TeleCore.Types;

public class PawnInfoData
{
    private Dictionary<Pawn, HashSet<PawnInfo>> _pawninfoByPawn;

    public PawnInfoData()
    {
        GlobalEventHandler.Things.RegisterSpawned(Notify_RegisterPawn, x => x is Pawn);
        GlobalEventHandler.Things.RegisterDespawned(Notify_DeregisterPawn, x => x is Pawn);
    }

    private void Notify_RegisterPawn(ThingStateChangedEventArgs args)
    {
    }

    public void Notify_DeregisterPawn(ThingStateChangedEventArgs args)
    {

    }
}