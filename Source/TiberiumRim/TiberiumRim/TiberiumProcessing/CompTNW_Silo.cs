using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class CompTNW_Silo : CompTNW
    {
        public override bool ShouldDoEffecters => Container.StoredPercent > 0.5f;

        public override IEnumerable<IntVec3> InnerConnectionCells
        {
            get
            {
                var rect = parent.OccupiedRect();
                var cells = rect.Cells.ToList();
                rect.Corners.ToList().ForEach(x => cells.Remove(x));
                return cells;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

        }

        public override void DistributeValues()
        {
            base.DistributeValues();
            if (!Container.ContainsForbiddenType) return;
            var forbiddenTypes = Container.AllStoredTypes.Where(t => !Container.AcceptsType(t));
            foreach (TiberiumValueType type in forbiddenTypes)
            {
                var siloOther = SiloForType(type);
                if (siloOther != null)
                {
                    Container.TryTransferTo(siloOther.Container, type, 5f);
                }
            }
        }

        public CompTNW_Silo SiloForType(TiberiumValueType valueType)
        {
            return Network.NetworkSet.Silos.Find(s => !s.Container.CapacityFull && s.Container.AcceptsType(valueType));
        }
    }
}
