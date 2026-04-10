using System;
using Verse;

namespace TeleCore.Utils.Serialization;

public static class TeleExposeUtility
{
    public static byte[] SerializeUInt(Verse.Map map, Func<IntVec3, uint> intReader)
    {
        return TeleSerializeUtility.SerializeUInt(map.info.NumCells,
            idx => intReader(map.cellIndices.IndexToCell(idx)));
    }

    public static void LoadUInt(byte[] arr, Verse.Map map, Action<IntVec3, uint> intWriter)
    {
        TeleSerializeUtility.LoadUInt(arr, map.info.NumCells,
            delegate (int idx, uint data) { intWriter(map.cellIndices.IndexToCell(idx), data); });
    }

    public static void ExposeUInt(Verse.Map map, Func<IntVec3, uint> uintReader, Action<IntVec3, uint> uintWriter,
        string label)
    {
        byte[] arr = null;
        if (Verse.Scribe.mode == LoadSaveMode.Saving) arr = SerializeUInt(map, uintReader);
        DataExposeUtility.LookByteArray(ref arr, label);
        if (Verse.Scribe.mode == LoadSaveMode.LoadingVars) LoadUInt(arr, map, uintWriter);
    }
}