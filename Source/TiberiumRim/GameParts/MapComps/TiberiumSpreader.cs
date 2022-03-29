﻿using System.Collections.Generic;
using System.Linq;
using Multiplayer.API;
using Verse;

namespace TiberiumRim
{
    public class TiberiumSpreader : MapInformation
    {
        public List<TiberiumProducer> producers = new List<TiberiumProducer>();

        public IEnumerator<TiberiumCrystal> crystalIterator;

        public IEnumerable<TiberiumField> TiberiumFields => producers.NullOrEmpty() ? default : producers.Select(t => t.TiberiumField);
        public IEnumerable<TiberiumField> MarkedFields => TiberiumFields.EnumerableNullOrEmpty() ? default : TiberiumFields.Where(t => t.MarkedForFastGrowth);
        public IEnumerable<TiberiumCrystal> TiberiumCrystals => MarkedFields.EnumerableNullOrEmpty() ? default : MarkedFields.SelectMany(t => t.GrowingCrystals);

        public bool ShouldSpread => !MarkedFields.EnumerableNullOrEmpty(); //TiberiumFields.Any(t => t?.MarkedForFastGrowth ?? false);
        private bool CanReset => !TiberiumCrystals.EnumerableNullOrEmpty();

        public TiberiumSpreader(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
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
                var current = crystalIterator.Current;
                if (current?.Spawned ?? false)
                {
                    while (current.Growth < 1)
                    {
                        current.TickLong();
                    }
                }

                if (!crystalIterator.MoveNext())
                {
                    crystalIterator = null;
                }
                /*
                do
                {
                    var current = crystalIterator.Current;
                    if (current?.Spawned ?? false)
                        current.TickLong();
                } while (crystalIterator.MoveNext());
                crystalIterator = null;
            */
            }
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
            //var crystals = TiberiumFields.Where(t => t?.MarkedForFastGrowth ?? false).SelectMany(t => t?.GrowingCrystals).ToList();
            var tiberiumCrystals = TiberiumCrystals.ToList();
            if (tiberiumCrystals.Any())
               crystalIterator = tiberiumCrystals.GetEnumerator();
        }
    }
}
