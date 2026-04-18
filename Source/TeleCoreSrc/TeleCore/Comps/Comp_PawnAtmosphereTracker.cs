using System.Collections.Generic;
using TeleCore.Defs;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Comps;

public class Comp_PawnAtmosphereTracker : ThingComp
{
    private static readonly Dictionary<Pawn, Comp_PawnAtmosphereTracker> OneOffs = new();
    private RoomComponent_Atmosphere? _curAtmosphere;

    public Pawn Pawn => parent as Pawn;
    public RoomComponent_Atmosphere RoomComp => _curAtmosphere;

    public bool IsOutside
    {
        get
        {
            if (_curAtmosphere == null)
            {
                TLog.Warning($"{Pawn.NameFullColored} had no atmosphere tracker. Re-attaching from current position.");
                if (!Pawn.Spawned)
                {
                    TLog.Error($"{Pawn.NameFullColored} is not spawned!");
                    if (Pawn.Map == null)
                    {
                        TLog.Error($"{Pawn.NameFullColored} has no map at position {Pawn.Position}!");
                        return false;
                    }
                }
                var room = Pawn.Position.GetRoom(Pawn.Map);
                _curAtmosphere = room.GetRoomComp<RoomComponent_Atmosphere>();
                if (_curAtmosphere == null)
                {
                    TLog.Error($"Pawn is in invalid room! {Pawn.Position} -> Room[{room.ID}][{room.Dereferenced}]");
                    return false;
                }
            }
            return _curAtmosphere.IsOutdoors;
        }
    }

    public static Comp_PawnAtmosphereTracker CompFor(Pawn pawn)
    {
        if (OneOffs.TryGetValue(pawn, out var value))
            return value;
        return null;
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        if (OneOffs.TryAdd(Pawn, this))
        {
            //...
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
    }

    public bool IsInAtmosphere(AtmosphericValueDef valueDef)
    {
        return _curAtmosphere.Volume.StoredValueOf(valueDef) > 0;
    }

    //
    public void Notify_EnteredAtmosphere(RoomComponent_Atmosphere atmosphere)
    {
        _curAtmosphere = atmosphere;
    }

    //Implies leaving outside
    public void Notify_Clear()
    {
        _curAtmosphere = null;
    }
}
