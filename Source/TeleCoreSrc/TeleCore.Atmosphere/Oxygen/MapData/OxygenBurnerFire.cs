using System.Collections.Generic;
using RimWorld;
using TeleCore.Atmosphere.Rooms;
using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.Oxygen.MapData;

public class OxygenBurnerFire : AtmosphereConverter
{
    private Fire _sourceFire;

    public override float BurningRate => 0.25f;

    protected IEnumerable<RoomComponent_Atmosphere> Atmospheres
    {
        get
        {
            foreach (var room in _sourceThing.GetRoomsAround())
            {
                yield return room.GetRoomComp<RoomComponent_Atmosphere>();
            }
        }
    }

    public OxygenBurnerFire(Fire fire) : base(fire)
    {
        _sourceFire = fire;
    }

    public override void Tick()
    {
        if (GenTicks.TicksAbs % 2 != 0) return;

        //If a wall, use atmospheres surrounding it
        if (_sourceFire.parent != null && _sourceFire.parent.def.IsRoomDivider())
        {
            foreach (var atmos in Atmospheres)
            {
                BurnOxygen(atmos);
            }
            return;
        }

        if (Atmosphere == null)
        {
            Log.WarningOnce($"Tried to tick oxygen burner with thing without a room: {_sourceThing}", _sourceFire.GetHashCode());
            return;
        }

        //Otherwise burn oxygen in the direct room
        BurnOxygen(Atmosphere);
    }

    private void BurnOxygen(RoomComponent_Atmosphere atmos)
    {
        var saturation = atmos.Volume.StoredPercentOf(NMODefOf.Atmosphere_Oxygen);
        var result = atmos.Volume.TryRemove(NMODefOf.Atmosphere_Oxygen, BurningRate);
        if(result.Actual.Value > 0) //[NMODefOf.Atmosphere_Oxygen]
        {
            atmos.Volume.TryAdd(NMODefOf.Atmosphere_CarbonMonoxide, 0.125f);
        }

        var exp = Fire.MaxFireSize * ((saturation * saturation) * 10);
        _sourceFire.fireSize = Mathf.Clamp((float)System.Math.Round(_sourceFire.fireSize, 2), 0, exp);
    }
}
