using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class AtmosphericSaveLoad
    {
        private Map map;

        private NetworkValueStack[] temporaryGrid;
        private NetworkValueStack[] atmosphericGrid;

        private AtmosphericMapInfo AtmosphericMapInfo => map.Tiberium().AtmosphericInfo;

        public AtmosphericSaveLoad(Map map)
        {
            this.map = map;
        }

        public void ApplyLoadedDataToRegions()
        {
            if (atmosphericGrid == null) return;

            CellIndices cellIndices = map.cellIndices;
            AtmosphericMapInfo.OutsideContainer.Container.LoadFromStack(atmosphericGrid[map.cellIndices.NumGridCells]); //ShortToInt(pollGrid[map.cellIndices.NumGridCells]);
            TRLog.Debug($"Applying Outside Atmospheric: {atmosphericGrid[map.cellIndices.NumGridCells]}");

            foreach (var comp in AtmosphericMapInfo.PollutionComps)
            {
                var valueStack = atmosphericGrid[cellIndices.CellToIndex(comp.Key.Cells.First())];
                comp.Value.ActualContainer.Container.LoadFromStack(valueStack);
                //Log.Message($"Applying on Tracker {comp.Key.ID}: {valueStack}");
            }
            //
            /*
            foreach (Region region in map.regionGrid.AllRegions_NoRebuild_InvalidAllowed)
            {
                if (region.Room != null)
                {
                    var valueStack = atmosphericGrid[cellIndices.CellToIndex(region.Cells.First())];
                    Log.Message($"Applying on Region  {region.Room.ID}: {valueStack}");
                    AtmosphericMapInfo.PollutionFor(region.Room).ActualContainer.Container.LoadFromStack(valueStack);
                }
            }
            */
            //

            atmosphericGrid = null;
        }

        public void DoExposing()
        {
            TRLog.Debug("Exposing Atmospheric");
            int arraySize = map.cellIndices.NumGridCells + 1;
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                temporaryGrid = new NetworkValueStack[arraySize];
                var outsideAtmosphere = AtmosphericMapInfo.OutsideContainer.Container.ValueStack;
                temporaryGrid[arraySize - 1] = outsideAtmosphere;

                foreach (var roomComp in AtmosphericMapInfo.AllComps)
                {
                    if (roomComp.IsOutdoors) continue;
                    var roomAtmosphereStack = roomComp.ActualContainer.Container.ValueStack;
                    foreach (IntVec3 c2 in roomComp.Room.Cells)
                    {
                        temporaryGrid[map.cellIndices.CellToIndex(c2)] = roomAtmosphereStack;
                    }
                }
            }

            //Turn temp grid into byte arrays
            var savableTypes = AtmosphericMapInfo.OutsideContainer.Container.AcceptedTypes;
            foreach (var type in savableTypes)
            {
                byte[] dataBytes = null;
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    dataBytes = DataSerializeUtility.SerializeInt(arraySize, (int idx) => temporaryGrid[idx].networkValues?.FirstOrFallback(f => f.valueDef == type).value ?? 0);
                    DataExposeUtility.ByteArray(ref dataBytes, $"{type.defName}.atmospheric");
                }

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    DataExposeUtility.ByteArray(ref dataBytes, $"{type.defName}.atmospheric");
                    atmosphericGrid = new NetworkValueStack[arraySize];
                    DataSerializeUtility.LoadInt(dataBytes, arraySize, delegate (int idx, int idxValue)
                    {
                        /*
                        if (idxValue > 0)
                        {
                            Log.Message($"Loading {idx}[{map.cellIndices.IndexToCell(idx)}]: {idxValue} ({type})");
                        }
                        */
                        atmosphericGrid[idx] += new NetworkValueStack(type, idxValue);
                    });
                }
            }
        }
    }
}
