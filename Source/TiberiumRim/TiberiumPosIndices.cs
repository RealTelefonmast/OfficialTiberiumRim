using Verse;

namespace TR
{
    public static class TiberiumPosIndices
    {
        static TiberiumPosIndices()
        {
            for (int i = 0; i < 25; i++)
            {
                rootList[i] = new int[8][];
                for (int j = 0; j < 8; j++)
                {
                    int[] array = new int[i + 1];
                    for (int k = 0; k < i; k++)
                    {
                        array[k] = k;
                    }
                    array.Shuffle<int>();
                    rootList[i][j] = array;
                }
            }
        }

        public static int[] GetPositionIndices(TiberiumCrystal crystal)
        {
            int maxMeshCount = crystal.def.tiberium.MeshCount;
            int num = (crystal.thingIDNumber ^ 42348528) % 8;
            return rootList[maxMeshCount - 1][num];
        }

        private static int[][][] rootList = new int[25][][];

        private const int ListCount = 8;
    }
}
