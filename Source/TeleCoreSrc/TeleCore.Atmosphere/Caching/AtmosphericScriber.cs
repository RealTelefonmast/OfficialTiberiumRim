using System.Linq;
using TeleCore.Atmosphere.Defs;
using TeleCore.Logging;
using TeleCore.Primitive;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TeleCore.Atmosphere.Caching;

internal class AtmosphericScriber : IExposable
{
    private AtmosphericMapInfo _mapInfo;
    private Map _map;

    private DefValueStack<AtmosphericValueDef, double>[] temporaryGrid;
    private DefValueStack<AtmosphericValueDef, double>[] atmosphericGrid;

    public AtmosphericScriber(AtmosphericMapInfo mapInfo)
    {
        _mapInfo = mapInfo;
        _map = mapInfo.Map;
    }

    internal void ApplyLoadedDataToRegions()
    {
        if (atmosphericGrid == null) return;

        var cellIndices = _map.cellIndices;
        var outsideStack = atmosphericGrid[_map.cellIndices.NumGridCells];
        if (outsideStack.IsValid)
        {
            _mapInfo.Notify_LoadedOutsideAtmosphere(outsideStack);
        }

        foreach (var comp in _mapInfo.AllAtmosphericRooms)
        {
            var index = cellIndices.CellToIndex(comp.Parent.Room.Cells.First());
            var valueStack = atmosphericGrid[index];
            if (valueStack.IsValid)
            {
                comp.Volume.LoadFromStack(valueStack);
            }
        }

        atmosphericGrid = null;
    }

    public void ExposeData()
    {
        ScribeData();
    }

    internal void ScribeData()
    {
        TLog.Debug($"Exposing Atmospheric | {Scribe.mode}".Colorize(Color.cyan));
        int arraySize = _map.cellIndices.NumGridCells + 1;
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            temporaryGrid = new DefValueStack<AtmosphericValueDef,double>[arraySize];
            var outsideAtmosphere = _mapInfo.MapVolume.Stack;
            temporaryGrid[arraySize - 1] = outsideAtmosphere;

            foreach (var roomComp in _mapInfo.AllAtmosphericRooms)
            {
                if (roomComp.IsOutdoors) continue;
                var roomAtmosphereStack = roomComp.Volume.Stack;
                foreach (IntVec3 c2 in roomComp.Room.Cells)
                {
                    temporaryGrid[_map.cellIndices.CellToIndex(c2)] = roomAtmosphereStack;
                }
            }
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            atmosphericGrid = new DefValueStack<AtmosphericValueDef,double>[arraySize];
        }

        //Turn temp grid into byte arrays
        var savableTypes = DefDatabase<AtmosphericValueDef>.AllDefsListForReading;
        foreach (var type in savableTypes)
        {
            byte[] dataBytes = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                dataBytes = DataSerializeUtility.SerializeUshort(arraySize, (int idx) => (ushort)(temporaryGrid[idx].Values?.FirstOrFallback(f => f.Def == type).Value ?? 0));
                DataExposeUtility.LookByteArray(ref dataBytes, $"{type.defName}.atmospheric");
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                DataExposeUtility.LookByteArray(ref dataBytes, $"{type.defName}.atmospheric");
                DataSerializeUtility.LoadUshort(dataBytes, arraySize, delegate(int idx, ushort idxValue)
                {
                    var atmosStack =  new DefValue<AtmosphericValueDef,double>(type, idxValue);
                    if (atmosStack.Value > 0d)
                    {
                        atmosphericGrid[idx] += atmosStack;
                    }
                });
            }
        }
    }
}
