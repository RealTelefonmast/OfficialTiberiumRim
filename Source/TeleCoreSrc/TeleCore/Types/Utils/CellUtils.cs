using Verse;

namespace TeleCore.Unsorted;

public static class CellUtils
{
    public static int Index(this IntVec3 vec, Map map)
    {
        return map.cellIndices.CellToIndex(vec);
    }

    public static Rot4 RelativeDir(CellRect rect, IntVec3 pos)
    {
        rect = rect.ContractedBy(1);

        if (pos.x < rect.minX)
            return Rot4.West;

        if (pos.x > rect.maxX)
            return Rot4.East;

        if (pos.z > rect.minZ)
            return Rot4.North;

        if (pos.z < rect.maxZ)
            return Rot4.South;

        return Rot4.Invalid;
    }


    /// <summary>
    ///     Compares two directly adjacent cells to determine the <see cref="Rot4" /> direction of <paramref name="other" />
    ///     relative of <paramref name="cell" />.
    /// </summary>
    /// <returns>The <see cref="Rot4" /> from <paramref name="cell" /> 'looking' towards <paramref name="other" />.</returns>
    public static Rot4 Rot4Relative(this IntVec3 cell, IntVec3 other)
    {
        if (!cell.AdjacentToCardinal(other)) return Rot4.Invalid;

        var diff = cell - other;
        if (diff.x > 0)
            return Rot4.East;
        if (diff.x < 0)
            return Rot4.West;
        if (diff.z > 0)
            return Rot4.North;
        if (diff.z < 0)
            return Rot4.South;

        return Rot4.Invalid;
    }
}