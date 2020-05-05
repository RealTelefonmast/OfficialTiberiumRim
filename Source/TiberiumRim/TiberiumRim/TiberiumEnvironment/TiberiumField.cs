using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumField : IExposable
    {
        private readonly TiberiumProducer producer;
        private List<TiberiumCrystal> tiberium = new List<TiberiumCrystal>();
        private readonly List<IntVec3> fieldCells       = new List<IntVec3>();

        //FastGrowth
        private readonly List<TiberiumCrystal> crystalsToGrow = new List<TiberiumCrystal>();

        private bool fastGrowth;

        private IEnumerator<TiberiumCrystal> tiberiumEnumerator;

        public TiberiumField(TiberiumProducer producer)
        {
            this.producer = producer;
        }

        public TiberiumField(TiberiumProducer producer, List<TiberiumCrystal> crystals)
        {
            this.producer = producer;
            this.tiberium = crystals;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref tiberium, "tiberiumList", LookMode.Reference);
        }

        public List<IntVec3> FieldCells => fieldCells;

        private int iterationTicks = 0;

        public void Tick()
        {
            if (fastGrowth)
            {
                for (int i = crystalsToGrow.Count - 1; i >= 0; i--)
                {
                    crystalsToGrow[i].TickLong();//TiberiumTick(2000);
                    if (crystalsToGrow[i].HasSpread)
                        crystalsToGrow.RemoveAt(i);
                }
                //EnumerateCrystals();
            }
        }

        public void AddFieldCell(IntVec3 cell)
        {
            fieldCells.Add(cell);
            foreach (var type in producer.TiberiumTypes)
            {
                producer.Map.Tiberium().TiberiumInfo.SetFieldColor(cell, true, type.TiberiumValueType);
            }
        }

        public void RemoveFieldCell(IntVec3 cell)
        {
            fieldCells.Remove(cell);
            foreach (var type in producer.TiberiumTypes)
            {
                producer.Map.Tiberium().TiberiumInfo.SetFieldColor(cell, false, type.TiberiumValueType);
            }
        }

        public void AddTiberium(TiberiumCrystal crystal)
        {
            tiberium.Add(crystal);

            crystalsToGrow.Add(crystal);
        }

        public void RemoveTiberium(TiberiumCrystal crystal)
        {
            tiberium.Remove(crystal);

            crystalsToGrow.Remove(crystal);
        }

        public void DEBUGFastGrowth()
        {
            fastGrowth = !fastGrowth;
        }

        private void EnumerateCrystals()
        {
            var curCrystal = tiberiumEnumerator?.Current;
            for (int i = 0; i < 250; i++)
            {
                curCrystal?.TiberiumTick(1);
            }
            iterationTicks++;
            if (!tiberiumEnumerator?.MoveNext() ?? true)
            {
                var tempList = new List<TiberiumCrystal>(tiberium);
                tiberiumEnumerator = tempList.GetEnumerator();
                Log.Message("Resetting tiberium enumerator");
            }
        }

        public string InspectString()
        {
            string fieldString = "Tiberium Field:";
            fieldString += "\nField Size: " + fieldCells.Count;
            fieldString += "\nTiberium Crystals: " + tiberium.Count;
            fieldString += "\nGrowing Crystals: " + crystalsToGrow.Count;
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
                GenDraw.DrawFieldEdges(fieldCells, Color.green);
        }

        public IEnumerable<Gizmo> Gizmos()
        {
            yield return new Command_Action()
            {
                defaultLabel = "Show Field",
                action = delegate { drawField = !drawField; }
            };
        }
    }
}
