using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumField : IExposable
    {
        private TiberiumProducer producer;
        private List<TiberiumCrystal> tiberium = new List<TiberiumCrystal>();

        private CellArea fieldCellArea;
        private TiberiumGarden fieldGarden;

        //FastGrowth
        private bool fastGrowth;

        public TiberiumProducer Producer => producer;

        public IEnumerable<TiberiumCrystal> FieldCrystals => tiberium;
        public IEnumerable<TiberiumCrystal> GrowingCrystals => FieldCrystals.Where(t => t.ShouldSpread);

        public bool MarkedForGrowth => fastGrowth;

        public TiberiumField()
        {
        }

        public TiberiumField(TiberiumProducer producer)
        {
            this.producer = producer;
            fieldCellArea = new CellArea(producer.Map);
        }

        public TiberiumField(TiberiumProducer producer, List<TiberiumCrystal> crystals)
        {
            this.producer = producer;
            this.tiberium = crystals;
            fieldCellArea = new CellArea(producer.Map);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref producer, "producer");
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
            fieldCellArea.Add(cell);
            if (producer.TiberiumTypes.EnumerableNullOrEmpty()) return;
            foreach (var type in producer.TiberiumTypes)
            {
                map.Tiberium().TiberiumInfo.SetFieldColor(cell, true, type.TiberiumValueType);
            }
        }

        public void RemoveFieldCell(IntVec3 cell, Map map)
        {
            fieldCellArea.Remove(cell);
            foreach (var type in producer.TiberiumTypes)
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
            fastGrowth = !fastGrowth;
        }

        public string InspectString()
        {
            string fieldString = "Tiberium Field:";
            fieldString += "\nField Size: " + fieldCellArea.Count;
            fieldString += "\nTiberium Crystals: " + tiberium.Count;
            fieldString += "\nGrowing Crystals: " + GrowingCrystals.Count();//crystalsToGrow.Count;
            fieldString += "\nFast Growth Enabled: " + fastGrowth;
            if (fastGrowth)
                fieldString += "\nIteration Tick: " + iterationTicks;
            //fieldString += "\n" + (mutationTicks - (GenTicks.TicksGame - startTick)) + " Ticks Remaining";
            return fieldString;
        }

        private bool drawField = false;
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
