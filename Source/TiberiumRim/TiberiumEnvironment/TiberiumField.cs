using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumField : IExposable
    {
        private Map map;

        private TiberiumProducer mainProducer;
        private TiberiumProducer blossomTree;
        private List<TiberiumCrystal> tiberium = new List<TiberiumCrystal>();

        private CellArea fieldCellArea;
        private TiberiumGarden fieldGarden;

        //Debug
        private bool fastFastGrowth;
        private bool drawField = false;

        public TiberiumProducer MainProducer => mainProducer;

        public TiberiumProducer BlossomTree
        {
            get => blossomTree;
            set => blossomTree = value;
        }
        public IEnumerable<TiberiumCrystal> FieldCrystals => tiberium;
        public IEnumerable<TiberiumCrystal> GrowingCrystals => FieldCrystals.Where(t => t.Spawned && t.ShouldSpread);

        public bool MarkedForFastGrowth
        {
            get => fastFastGrowth;
            //TODO:[SyncMethod]
            private set => fastFastGrowth = value;
        }

        public int TotalWorth => FieldCrystals.Sum(c => (int)c.HarvestValue);

        /*TODO:[SyncWorker]
        static void SyncWorkerTibField(SyncWorker sync, ref TiberiumField type)
        {
            if (sync.isWriting)
            {
                sync.Write(type.mainProducer);
            }
            else
            {
                var thing = sync.Read<TiberiumProducer>();
                type = thing.TiberiumField;
            }
        }
        */

        public TiberiumField()
        {
        }

        public TiberiumField(TiberiumProducer mainProducer)
        {
            this.mainProducer = mainProducer;
            fieldCellArea = new CellArea(mainProducer.Map);
            map = mainProducer.Map;
        }

        public TiberiumField(TiberiumProducer mainProducer, List<TiberiumCrystal> crystals)
        {
            this.mainProducer = mainProducer;
            this.tiberium = crystals;
            fieldCellArea = new CellArea(mainProducer.Map);
            map = mainProducer.Map;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref mainProducer, "mainProducer");
            Scribe_Deep.Look(ref fieldCellArea, "fieldCells");
        }

        public List<IntVec3> FieldCells => fieldCellArea.Cells;

        private int iterationTicks = 0;

        public void Tick()
        {
        }

        public void AddFieldCell(IntVec3 cell, Map map)
        {
            var Tiberium = map.Tiberium();
            if (!fieldCellArea.Contains(cell, map))
            {
                fieldCellArea.Add(cell);
                Tiberium.TiberiumProducerInfo.Notify_FieldCellAdded(MainProducer, cell);
            }

            if (mainProducer.TiberiumTypes.EnumerableNullOrEmpty()) return;
            foreach (var type in mainProducer.TiberiumTypes)
            {
                Tiberium.TiberiumInfo.SetFieldColor(cell, true, type.TiberiumValueType);
            }
        }

        public void RemoveFieldCell(IntVec3 cell, Map map)
        {
            var Tiberium = map.Tiberium();
            if (fieldCellArea.Remove(cell))
            {
                Tiberium.TiberiumProducerInfo.Notify_FieldCellRemoved(cell);
            }
            foreach (var type in mainProducer.TiberiumTypes)
            {
                Tiberium.TiberiumInfo.SetFieldColor(cell, false, type.TiberiumValueType);
            }
        }

        public void AddTiberium(TiberiumCrystal crystal)
        {
            tiberium.Add(crystal);
        }

        public void RemoveTiberium(TiberiumCrystal crystal)
        {
            tiberium.Remove(crystal);
        }

        public void DEBUGFastGrowth()
        {
            MarkedForFastGrowth = !MarkedForFastGrowth;
        }

        public string InspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==Tiberium Field==");
            sb.AppendLine($"[FastGrowth]: {MarkedForFastGrowth}");
            sb.AppendLine($"Field Size: {fieldCellArea?.Count}");
            sb.AppendLine($"Tiberium Crystals: {tiberium?.Count}");
            sb.AppendLine($"Growing Crystals:  {GrowingCrystals?.Count()}"); 
            sb.AppendLine($"Total Field Value: {TotalWorth}");

            if (MarkedForFastGrowth)
                sb.AppendLine($"Iteration Tick: {iterationTicks}");
            //fieldString += "\n" + (mutationTicks - (GenTicks.TicksGame - startTick)) + " Ticks Remaining";
            return sb.ToString().TrimStart().TrimEnd();
        }

        internal void DrawField()
        {
            if(drawField)
                GenDraw.DrawFieldEdges(FieldCells, Color.green);
        }

        internal IEnumerable<Gizmo> Gizmos()
        {
            if (!DebugSettings.godMode) yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Show Field",
                action = delegate { drawField = !drawField; }
            };
        }

        internal bool Contains(IntVec3 cell)
        {
            return fieldCellArea.Contains(cell, map);
        }
    }
}
