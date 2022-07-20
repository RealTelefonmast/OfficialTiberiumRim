using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AtmosphericCache : IExposable
    {
        private Map map;
        private HashSet<int> processedRoomIDs = new HashSet<int>();
        public CachedAtmosphereInfo[] pollutionCache;
        internal AtmosphericSaveLoad atmosphericSaveLoad;

        public AtmosphericMapInfo AtmosphericInfo => map.Tiberium().AtmosphericInfo;

        public AtmosphericCache(Map map)
        {
            this.map = map;
            this.pollutionCache = new CachedAtmosphereInfo[map.cellIndices.NumGridCells];
            this.atmosphericSaveLoad = new AtmosphericSaveLoad(map);
        }

        public void ExposeData()
        {
            this.atmosphericSaveLoad.DoExposing();
        }

        private void SetCachedInfo(IntVec3 c, CachedAtmosphereInfo atmosphere)
        {
            pollutionCache[map.cellIndices.CellToIndex(c)] = atmosphere;
        }

        public void ResetInfo(IntVec3 c)
        {
            pollutionCache[map.cellIndices.CellToIndex(c)].Reset();
        }

        public void TryCacheRegionAtmosphericInfo(IntVec3 c, Region reg)
        {
            Room room = reg.Room;
            if (room == null) return;
            SetCachedInfo(c, new CachedAtmosphereInfo(room.ID, room.CellCount, AtmosphericInfo.ComponentAt(room).ActualContainer.ValueStack));
        }

        public bool TryGetAtmosphericValuesForRoom(Room r, out Dictionary<NetworkValueDef, int> result)
        {
            CellIndices cellIndices = this.map.cellIndices;
            result = new();
            foreach (var c in r.Cells)
            {
                CachedAtmosphereInfo cachedInfo = this.pollutionCache[cellIndices.CellToIndex(c)];
                //If already processed or not a room, ignore
                if (cachedInfo.numCells <= 0 || processedRoomIDs.Contains(cachedInfo.roomID) || cachedInfo.stack.Empty) continue;
                processedRoomIDs.Add(cachedInfo.roomID);
                foreach (var value in cachedInfo.stack.networkValues)
                {
                    if (!result.ContainsKey(value.valueDef))
                    {
                        result.Add(value.valueDef, 0);
                    }

                    float addedValue = value.value;
                    if (cachedInfo.numCells > r.CellCount)
                    {
                        addedValue = value.value * (r.CellCount / (float)cachedInfo.numCells);
                    }
                    //var adder = value.value * (Mathf.Min(cachedInfo.numCells, r.CellCount)/(float)Mathf.Max(cachedInfo.numCells , r.CellCount));
                    result[value.valueDef] += Mathf.CeilToInt(addedValue);
                }
            }

            processedRoomIDs.Clear();
            return !result.NullOrEmpty();
        }
    }
}
