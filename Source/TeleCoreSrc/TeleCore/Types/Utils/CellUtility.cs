using Verse;

namespace TeleCore.Types.Utils;

public static class CellUtility
{
    public static int Index(this IntVec3 pos, Map map)
    {
        return map.cellIndices.CellToIndex(pos);
    }
}