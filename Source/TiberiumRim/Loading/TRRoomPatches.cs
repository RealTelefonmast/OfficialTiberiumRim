using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace TiberiumRim
{
    internal static class TRRoomPatches
    {
        [HarmonyPatch(typeof(RegionAndRoomUpdater))]
        [HarmonyPatch("CreateOrUpdateRooms")]
        public static class CreateOrUpdateRoomsPatch
        {
            public static bool Prefix(Map ___map)
            {
                ___map.Tiberium().RoomInfo.updater.Notify_RoomUpdatePrefix();
                return true;
            }


            public static void Postfix(Map ___map)
            {
                ___map.Tiberium().RoomInfo.updater.Notify_RoomUpdatePostfix();
            }
        }

        [HarmonyPatch(typeof(RegionAndRoomUpdater))]
        [HarmonyPatch("NotifyAffectedDistrictsAndRoomsAndUpdateTemperature")]
        public static class NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperaturePatch
        {
            public static bool Prefix(Map ___map, List<Room> ___newRooms, HashSet<Room> ___reusedOldRooms)
            {
                ___map.Tiberium().RoomInfo.updater.Notify_SetNewRoomData(___newRooms, ___reusedOldRooms);
                return true;
            }
        }


        [HarmonyPatch(typeof(Room))]
        [HarmonyPatch("Notify_RoofChanged")]
        public static class Notify_RoofChangedPatch
        {
            public static void Postfix(Room __instance)
            {
                __instance.Map.Tiberium().RoomInfo.updater.Notify_RoofChanged(__instance);
            }
        }

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
}
