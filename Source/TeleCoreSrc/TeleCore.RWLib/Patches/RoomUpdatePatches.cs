using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace TeleCore;

/// <summary>
///     Main handling of changes in regions and rooms of RimWorld
/// </summary>
internal static class RoomUpdatePatches
{

    // Rebuilding Dirty Regions And Rooms Begins
    // > RegenerateNewRegionsFromDirtyCells
    //   Crates new Regions based on dirty cells
    // > CreateOrUpdateRooms
    //   Actaully creates rooms from new regions

    private static Room GetParentRoom(Thing thing, Map map)
    {
        var position = thing.Position;
        if (!position.InBounds(map)) return null;

        var validRegion = map.regionGrid.GetValidRegionAt_NoRebuild(position);
        if (validRegion != null && validRegion.type.Passable()) return validRegion.Room;

        return null;
    }

    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("CreateOrUpdateRooms")]
    public static class CreateOrUpdateRoomsPatch
    {
        public static bool Prefix(Map ___map)
        {
            RoomUpdateNotifiers.Notify_RoomUpdatePrefix(___map);
            return true;
        }

        public static void Postfix(Map ___map)
        {
            //Clear null and void rooms - RW doesnt do this
            for (var i = ___map.regionGrid.allRooms.Count - 1; i >= 0; i--)
            {
                var room = ___map.regionGrid.allRooms[i];
                if (room.Dereferenced) ___map.regionGrid.allRooms.RemoveAt(i);
            }

            //
            RoomUpdateNotifiers.Notify_RoomUpdatePostfix(___map);
        }
    }

    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("NotifyAffectedDistrictsAndRoomsAndUpdateTemperature")]
    public static class NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperaturePatch
    {
        public static bool Prefix(Map ___map, List<Room> ___newRooms, HashSet<Room> ___reusedOldRooms)
        {
            if (___map is null) return true;

            RoomUpdateNotifiers.Notify_SetNewRoomData(___newRooms, ___reusedOldRooms);
            return true;
        }
    }

    [HarmonyPatch(typeof(Room))]
    [HarmonyPatch("Notify_RoofChanged")]
    public static class Notify_RoofChangedPatch
    {
        public static void Postfix(Room __instance)
        {
            RoomUpdateNotifiers.Notify_RoofChanged(__instance);
        }
    }

    //
    [HarmonyPatch(typeof(RegionListersUpdater))]
    [HarmonyPatch(nameof(RegionListersUpdater.RegisterInRegions))]
    public static class RegionListersUpdater_RegisterInRegionsPatch
    {
        public static void Postfix(Thing thing, Map map)
        {
            var room = GetParentRoom(thing, map);
            if (room is null) return;
            map.GetMapInfo<MapInformation_Rooms>().Notify_RegisterThing(thing, room);
        }
    }

    [HarmonyPatch(typeof(RegionListersUpdater))]
    [HarmonyPatch(nameof(RegionListersUpdater.DeregisterInRegions))]
    public static class RegionListersUpdater_DeregisterInRegions
    {
        public static void Postfix(Thing thing, Map map)
        {
            map.GetMapInfo<MapInformation_Rooms>().Notify_DeregisterThing(thing, GetParentRoom(thing, map));
        }
    }

    //Hijack Roomtemperature update for cell-based caching

    #region MyRegion

    [HarmonyPatch(typeof(TemperatureCache), nameof(TemperatureCache.TryCacheRegionTempInfo))]
    public static class TryCacheRegionTempInfoPatch
    {
        public static void Postfix(IntVec3 c, Region reg, Map ___map)
        {
            RoomUpdateNotifiers.Notify_RoomUpdateSetDirtyCell(c, reg, ___map);
            // GlobalEventHandler.OnRegionStateCached(new RegionStateChangedArgs
            // {
            //     Map = ___map,
            //     Cell = c,
            //     Region = reg,
            // });
        }
    }


    [HarmonyPatch(typeof(TemperatureCache), nameof(TemperatureCache.ResetCachedCellInfo))]
    public static class ResetCachedCellInfoPatch
    {
        public static void Postfix(IntVec3 c, Map ___map)
        {
            RoomUpdateNotifiers.Notify_RoomUpdateResetDirtyCell(c, ___map);
            // GlobalEventHandler.OnRegionStateReset(new RegionStateChangedArgs
            // {
            //     Map = ___map,
            //     Cell = c,
            // });
        }
    }

    [HarmonyPatch(typeof(TemperatureCache), nameof(TemperatureCache.TryGetAverageCachedRoomTemp))]
    public static class TryGetAverageCachedRoomTempPatch
    {
        public static void Postfix(Room r, Map ___map)
        {
            RoomUpdateNotifiers.Notify_RoomUpdateGetCache(r, ___map);
            // GlobalEventHandler.OnRegionStateGet(new RegionStateChangedArgs
            // {
            //     Map = ___map,
            //     Room = r,
            // });
        }
    }

    #endregion


    #region OldStuff

    /*
    [HarmonyPatch(typeof(RegionAndRoomUpdater))]
    [HarmonyPatch("RegenerateNewRegionsFromDirtyCells")]
    public static class RegionPatch
    {
        private static readonly List<Region> oldRegions = new List<Region>();
        private static readonly List<Region> newRegions = new List<Region>();

        public static bool Prefix(RegionAndRoomUpdater __instance)
        {
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            RegionGrid grid = Traverse.Create(map).Field("regionGrid").GetValue<RegionGrid>();

            oldRegions.Clear();
            map.Tiberium().TiberiumInfo.regionGrid.RemoveDirtyGrids(map.regionDirtyer.DirtyCells);
            foreach (IntVec3 dirty in map.regionDirtyer.DirtyCells)
            {
                oldRegions.AddDistinct(grid.GetValidRegionAt(dirty));
            }
            return true;
        }

        public static void Postfix(RegionAndRoomUpdater __instance)
        {
            var instance = Traverse.Create(__instance);
            Map map = instance.Field("map").GetValue<Map>();
            var tiberium = map.Tiberium();
            var info = tiberium.TiberiumInfo;
            List<Region> regions = instance.Field("newRegions").GetValue<List<Region>>();

            newRegions.Clear();
            newRegions.AddRange(regions);

            info.regionGrid.Notify_RegionSplit(oldRegions,newRegions);
        }
    }
    */

    #endregion
}