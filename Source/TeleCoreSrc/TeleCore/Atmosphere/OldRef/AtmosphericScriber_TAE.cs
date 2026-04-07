// Preserved from TeleCore/Caching/AtmosphericScriber.cs (old version)

using Verse;

namespace TeleCore.Atmosphere.OldRef;

internal class AtmosphericScriber_TAE
{
    private readonly Map map;
    private DefValueStack<TAE.AtmosphericDef>[] atmosphericGrid;

    private DefValueStack<TAE.AtmosphericDef>[] temporaryGrid;

    internal AtmosphericScriber_TAE(Map map)
    {
        this.map = map;
    }

    private TAE.AtmosphericMapInfo AtmosphericMapInfo => map.GetMapInfo<TAE.AtmosphericMapInfo>();

    public void ApplyLoadedDataToRegions()
    {
        if (atmosphericGrid == null) return;

        var cellIndices = map.cellIndices;
        var values = atmosphericGrid[map.cellIndices.NumGridCells];
        if (values.IsValid) AtmosphericMapInfo.MapContainer.LoadFromStack(values);

        foreach (var comp in AtmosphericMapInfo.AllAtmosphericRooms)
        {
            var index = cellIndices.CellToIndex(comp.Parent.Room.Cells.First());
            var valueStack = atmosphericGrid[index];
            if (valueStack.IsValid) comp.Container.LoadFromStack(valueStack);
        }

        atmosphericGrid = null;
    }

    internal void ScribeData()
    {
        var arraySize = map.cellIndices.NumGridCells + 1;
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            temporaryGrid = new DefValueStack<TAE.AtmosphericDef>[arraySize];
            var outsideAtmosphere = AtmosphericMapInfo.MapContainer.ValueStack;
            temporaryGrid[arraySize - 1] = outsideAtmosphere;

            foreach (var roomComp in AtmosphericMapInfo.AllAtmosphericRooms)
            {
                if (roomComp.IsOutdoors) continue;
                var roomAtmosphereStack = roomComp.Container.ValueStack;
                foreach (IntVec3 c2 in roomComp.Room.Cells)
                    temporaryGrid[map.cellIndices.CellToIndex(c2)] = roomAtmosphereStack;
            }
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars) atmosphericGrid = new DefValueStack<TAE.AtmosphericDef>[arraySize];

        var savableTypes = DefDatabase<TAE.AtmosphericDef>.AllDefsListForReading;
        foreach (var type in savableTypes)
        {
            byte[] dataBytes = null;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                dataBytes = DataSerializeUtility.SerializeUshort(arraySize,
                    idx => (ushort)(temporaryGrid[idx].Values?.FirstOrFallback(f => f.Def == type).Value ?? 0));
                DataExposeUtility.ByteArray(ref dataBytes, $"{type.defName}.atmospheric");
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                DataExposeUtility.ByteArray(ref dataBytes, $"{type.defName}.atmospheric");
                DataSerializeUtility.LoadUshort(dataBytes, arraySize, delegate(int idx, ushort idxValue)
                {
                    var atmosStack = new DefFloat<TAE.AtmosphericDef>(type, idxValue);
                    if (atmosStack.Value > 0) atmosphericGrid[idx] += atmosStack;
                });
            }
        }
    }
}