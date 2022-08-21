using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAE;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class DynamicDataCacheInfo : MapInformation
    {
        private ComputeGrid<float> atmosphericPassGrid;
        private ComputeGrid<float> lightPassGrid;
        private ComputeGrid<uint> edificeGrid;

        public ComputeGrid<float> AtmosphericPassGrid => atmosphericPassGrid;
        public ComputeGrid<float> LightPassGrid => lightPassGrid;
        public ComputeGrid<uint> EdificeGrid => edificeGrid;

        public ComputeBuffer AtmosphericBuffer => atmosphericPassGrid.DataBuffer;
        public ComputeBuffer LightPassBuffer => lightPassGrid.DataBuffer;
        public ComputeBuffer EdificeBuffer => edificeGrid.DataBuffer;

        public DynamicDataCacheInfo(Map map) : base(map)
        {
            atmosphericPassGrid = new ComputeGrid<float>(map, (_) => 1f);
            lightPassGrid = new ComputeGrid<float>(map, (_) => 1f);
            edificeGrid = new ComputeGrid<uint>(map);
        }

        public override void ThreadSafeInit()
        {
            atmosphericPassGrid.ThreadSafeInit();
            lightPassGrid.ThreadSafeInit();
            edificeGrid.ThreadSafeInit();

            atmosphericPassGrid.UpdateCPUData();
            lightPassGrid.UpdateCPUData();
            edificeGrid.UpdateCPUData();

            //UpdateGraphics();
        }

        public void Notify_ThingSpawned(Thing thing)
        {
            Notify_UpdateThingState(thing);
            //UpdateGraphics();
        }

        public void UpdateGraphics()
        {
            if (!atmosphericPassGrid.IsReady) return;
            /*
            Color[] colors = new Color[map.cellIndices.NumGridCells];
            Color[] colors2 = new Color[map.cellIndices.NumGridCells];
            Color[] colors3 = new Color[map.cellIndices.NumGridCells];
            IntVec2 size = new IntVec2(map.Size.x, map.Size.z);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(0, atmosphericPassGrid[i], 0);
                colors2[i] = new Color(lightPassGrid[i], 0, 0);
                colors3[i] = new Color(0, 0, edificeGrid[i]);
            }

            TiberiumContent.GenerateTextureFrom(colors, size, "AtmosphericGrid");
            TiberiumContent.GenerateTextureFrom(colors2, size, "LightPassGrid");
            TiberiumContent.GenerateTextureFrom(colors3, size, "EdificeGrid");
            */
        }

        public void Notify_ThingDespawned(Thing thing)
        {
            foreach (var pos in thing.OccupiedRect())
            {
                if (thing is Building b)
                {
                    atmosphericPassGrid.ResetValue(pos, 1f);
                    if (b.def.IsEdifice())
                        edificeGrid.ResetValue(pos, 0);
                    if (b.def.blockLight)
                        lightPassGrid.ResetValue(pos, 1f);
                }
            }
        }

        public void Notify_UpdateThingState(Thing thing)
        {
            foreach (var pos in thing.OccupiedRect())
            {
                if (thing is Building b)
                {
                    //TODO: Map AtmosDef to int ID for compute shader def-based pass percent
                    atmosphericPassGrid.SetValue(pos, AtmosphericTransferWorker.AtmosphericPassPercent(b));
                    if (b.def.IsEdifice())
                        edificeGrid.SetValue(pos, 1);
                    if (b.def.blockLight)
                        lightPassGrid.SetValue(pos, 0);
                }
            }
        }
    }
}
