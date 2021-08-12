using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumField : IExposable
    {
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

        public bool MarkedForFastGrowth => fastFastGrowth;

        public int TotalWorth => FieldCrystals.Sum(c => (int)c.HarvestValue);

        public TiberiumField()
        {
        }

        public TiberiumField(TiberiumProducer mainProducer)
        {
            this.mainProducer = mainProducer;
            fieldCellArea = new CellArea(mainProducer.Map);
        }

        public TiberiumField(TiberiumProducer mainProducer, List<TiberiumCrystal> crystals)
        {
            this.mainProducer = mainProducer;
            this.tiberium = crystals;
            fieldCellArea = new CellArea(mainProducer.Map);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref mainProducer, "mainProducer");
            Scribe_Collections.Look(ref tiberium, "tiberiumList", LookMode.Reference);
            Scribe_Deep.Look(ref fieldCellArea, "fieldCells");
        }

        public List<IntVec3> FieldCells => fieldCellArea.Cells;

        private int iterationTicks = 0;

        public void Tick()
        {
        }

        public void AddFieldCell(IntVec3 cell, Map map)
        {
            if(!fieldCellArea.Contains(cell))
                fieldCellArea.Add(cell);
            if (mainProducer.TiberiumTypes.EnumerableNullOrEmpty()) return;
            foreach (var type in mainProducer.TiberiumTypes)
            {
                map.Tiberium().TiberiumInfo.SetFieldColor(cell, true, type.TiberiumValueType);
            }
        }

        public void RemoveFieldCell(IntVec3 cell, Map map)
        {
            fieldCellArea.Remove(cell);
            foreach (var type in mainProducer.TiberiumTypes)
            {
                map.Tiberium().TiberiumInfo.SetFieldColor(cell, false, type.TiberiumValueType);
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
            fastFastGrowth = !fastFastGrowth;
        }

        public string InspectString()
        {
            string fieldString = "Tiberium Field:";
            fieldString += "\nField Size: " + fieldCellArea.Count;
            fieldString += "\nTiberium Crystals: " + tiberium.Count;
            fieldString += "\nGrowing Crystals: " + GrowingCrystals.Count();//crystalsToGrow.Count;
            fieldString += "\nTotal Field Value: " + TotalWorth;
            fieldString += "\nFast Growth Enabled: " + fastFastGrowth;
            if (fastFastGrowth)
                fieldString += "\nIteration Tick: " + iterationTicks;
            //fieldString += "\n" + (mutationTicks - (GenTicks.TicksGame - startTick)) + " Ticks Remaining";
            return fieldString;
        }

        public void DrawField()
        {
            if(drawField)
                GenDraw.DrawFieldEdges(FieldCells, Color.green);
        }

        public IEnumerable<Gizmo> Gizmos()
        {
            if (!DebugSettings.godMode) yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Show Field",
                action = delegate { drawField = !drawField; }
            };
        }
    }
}
