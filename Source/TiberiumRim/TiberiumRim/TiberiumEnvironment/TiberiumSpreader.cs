using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TiberiumSpreader : MapInformation
    {
        public List<TiberiumProducer> producers = new List<TiberiumProducer>();

        public IEnumerator<TiberiumCrystal> crystalIterator;

        public IEnumerable<TiberiumField> TiberiumFields => producers.Select(t => t.TiberiumField);
        public bool ShouldSpread => TiberiumFields.Any(t => t?.MarkedForGrowth ?? false);

        public TiberiumSpreader(Map map) : base(map)
        {
        }

        public void Tick()
        {
            if (!ShouldSpread) return;

            if (crystalIterator == null)
            {
                ResetIterator();
            }

            do
            {
                var current = crystalIterator.Current;
                if(current?.Spawned ?? false)
                    current.TickLong();
            } while (crystalIterator.MoveNext());

            ResetIterator();
        }

        public void RegisterField(TiberiumProducer producer)
        {
            producers.Add(producer);
        }

        public void DeregisterField(TiberiumProducer producer)
        {
            producers.Remove(producer);
        }

        private void ResetIterator()
        {
            var crystals = TiberiumFields.Where(t => t?.MarkedForGrowth ?? false).SelectMany(t => t?.GrowingCrystals).ToList();
            var tiberiumCrystals = crystals.ToList();
            if (tiberiumCrystals.Any())
               crystalIterator = tiberiumCrystals.GetEnumerator();
        }
    }
}
