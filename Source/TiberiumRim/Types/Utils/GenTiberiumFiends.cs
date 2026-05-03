using System.Collections.Generic;

namespace TiberiumRim;

public static class GenTiberiumFiends
{
    public static List<TiberiumPawn> TrySpawnFiendsNear(TiberiumBlossom blossom)
    {
        var pawns = new List<TiberiumPawn>();
        var count = blossom.FieldCells.Count / 5;
        for (var i = 0; i < count; i++)
        {
        }

        return pawns;
    }
}