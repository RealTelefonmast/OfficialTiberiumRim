using System.Collections.Generic;
using TeleCore.Defs;
using Unity.Collections;
using Verse;

namespace TeleCore.Unsorted;

public class RadiationLayer
{
    private bool[] _affectedCells;
    private RadiationTypeDef _def;
    private byte[] _radiation;
    private List<IRadiationSource> _sources = new();
}

public interface IRadiationSource
{
}

public class RadiationGrid
{
    internal static RadiationTypeDef[]? _radDefs;
    internal static int _radDefCount;
    private readonly int _gridSize;
    private readonly Map _map;

    private NativeArray<DefValueStack<RadiationTypeDef, byte>> _grid;

    public RadiationGrid(Map map)
    {
        _map = map;
        _gridSize = map.cellIndices.NumGridCells;

        //
        if (_radDefs == null)
        {
            _radDefs = DefDatabase<RadiationTypeDef>.AllDefsListForReading.ToArray();
            _radDefCount = _radDefs.Length;
        }

        _grid = new NativeArray<DefValueStack<RadiationTypeDef, byte>>(_radDefCount, Allocator.Persistent);

        for (var c = 0; c < _gridSize; c++) _grid[c] = new DefValueStack<RadiationTypeDef, byte>();
    }

    public void AddRadiation(IntVec3 pos, RadiationTypeDef def, byte value)
    {
        var idx = _map.cellIndices.CellToIndex(pos);
        _grid[idx] += new DefValue<RadiationTypeDef, byte>(def, value);
    }

    public void RemoveRadiation(IntVec3 pos, RadiationTypeDef def, byte value)
    {
        var idx = _map.cellIndices.CellToIndex(pos);
        _grid[idx] -= new DefValue<RadiationTypeDef, byte>(def, value);
    }
}