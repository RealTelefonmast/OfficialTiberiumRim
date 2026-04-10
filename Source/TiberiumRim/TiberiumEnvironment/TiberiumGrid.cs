using Verse;

namespace TR;

public class MapGrid<T>
{
    private readonly int _x;
    private readonly int _y;
    private readonly int _cellCount;

    private T[] _gridArray;

    public MapGrid(Map map)
    {
        (_x, _y) = (map.Size.x, map.Size.y);
        _cellCount = map.Size.x * map.Size.y;
        _gridArray = new T[_cellCount];
    }
}

// DOD Stub
// public class TiberiumGrid(Map map) : MapGrid<TiberiumCell>(map)
// {
//     
// }