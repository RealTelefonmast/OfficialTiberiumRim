using TR.TiberiumObjects;
using Verse;

namespace TR;

public static class TiberiumPosIndices
{
    private const int ListCount = 8;

    private static readonly int[][][] rootList = new int[25][][];

    static TiberiumPosIndices()
    {
        for (var i = 0; i < 25; i++)
        {
            rootList[i] = new int[8][];
            for (var j = 0; j < 8; j++)
            {
                var array = new int[i + 1];
                for (var k = 0; k < i; k++) array[k] = k;
                array.Shuffle();
                rootList[i][j] = array;
            }
        }
    }

    public static int[] GetPositionIndices(TiberiumCrystal crystal)
    {
        var maxMeshCount = crystal.def.tiberium.MeshCount;
        var num = (crystal.thingIDNumber ^ 42348528) % 8;
        return rootList[maxMeshCount - 1][num];
    }
}