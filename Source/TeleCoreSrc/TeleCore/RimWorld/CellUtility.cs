using Verse;

namespace TeleCore;

public static class CellUtility
{
    public static int Index(this IntVec3 pos, Map map)
    {
        return map.cellIndices.CellToIndex(pos);
    }
}