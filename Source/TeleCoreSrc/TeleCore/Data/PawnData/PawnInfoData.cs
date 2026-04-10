using System.Collections.Generic;
using TeleCore.Events;
using TeleCore.Events.Args;
using Verse;

namespace TeleCore.PawnData;

public class PawnInfoData
{
    private Dictionary<Pawn, HashSet<PawnInfo>> _pawninfoByPawn;

    public PawnInfoData()
    {
        GlobalEventHandler.ThingSpawned += Notify_RegisterPawn;
        GlobalEventHandler.ThingDespawned += Notify_DeregisterPawn;
    }

    private void Notify_RegisterPawn(ThingStateChangedEventArgs args)
    {
    }

    public void Notify_DeregisterPawn(ThingStateChangedEventArgs args)
    {
        
    }
}