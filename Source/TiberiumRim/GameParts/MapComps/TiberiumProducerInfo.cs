using System.Collections.Generic;
using System.Linq;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class TiberiumProducerInfo : MapInformation
    {
        private List<TiberiumProducer> producers = new List<TiberiumProducer>();

        private BoolGrid producerBoolGrid;
        private TiberiumProducer[] producerFieldGrid;

        public IEnumerator<TiberiumCrystal> crystalIterator;

        public IEnumerable<TiberiumField> TiberiumFields => producers.NullOrEmpty() ? default : producers.Select(t => t.TiberiumField);
        public IEnumerable<TiberiumField> MarkedFields => TiberiumFields.EnumerableNullOrEmpty() ? default : TiberiumFields.Where(t => t.MarkedForFastGrowth);
        public IEnumerable<TiberiumCrystal> TiberiumCrystals => MarkedFields.EnumerableNullOrEmpty() ? default : MarkedFields.SelectMany(t => t.GrowingCrystals);

        public bool ShouldSpread => !MarkedFields.EnumerableNullOrEmpty(); //TiberiumFields.Any(t => t?.MarkedForFastGrowth ?? false);
        private bool CanReset => !TiberiumCrystals.EnumerableNullOrEmpty();

        public TiberiumProducerInfo(Map map) : base(map)
        {
            producerBoolGrid = new BoolGrid(map);
            producerFieldGrid = new TiberiumProducer[map.cellIndices.NumGridCells];
        }

        public override void Tick()
        {
            if (!ShouldSpread) return;

            if (crystalIterator == null && CanReset)
            {
                ResetIterator();
            }
            else if(crystalIterator != null)
            {
                for(int i = 0; i < 20; i++)
                {
                    var current = crystalIterator.Current;
                    if (current?.Spawned ?? false)
                    {
                        current.TickLong();
                    }

                    if (!crystalIterator.MoveNext())
                    {
                        crystalIterator = null;
                        break;
                    }
                }
            }
        }

        public bool HasProducerAt(IntVec3 cell, out TiberiumProducer producer)
        {
            producer = producerFieldGrid[cell.Index(map)];
            return producerBoolGrid[cell];
        }

        public void Notify_FieldCellAdded(TiberiumProducer producer, IntVec3 cell)
        {
            producerBoolGrid[cell] = true;
            producerFieldGrid[cell.Index(map)] = producer;
        }

        public void Notify_FieldCellRemoved(IntVec3 cell)
        {
            producerBoolGrid[cell] = false;
            producerFieldGrid[cell.Index(map)] = null;
        }

        public void RegisterProducer(TiberiumProducer producer)
        {
            producers.Add(producer);
        }

        public void DeregisterProducer(TiberiumProducer producer)
        {
            producers.Remove(producer);
        }

        private void ResetIterator()
        {
            //var crystals = TiberiumFields.Where(t => t?.MarkedForFastGrowth ?? false).SelectMany(t => t?.GrowingCrystals).ToList();
            var tiberiumCrystals = TiberiumCrystals.ToList();
            if (tiberiumCrystals.Any())
               crystalIterator = tiberiumCrystals.GetEnumerator();
        }
    }
}
