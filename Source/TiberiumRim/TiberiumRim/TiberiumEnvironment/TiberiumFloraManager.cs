using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumFloraManager
    {
        public Map map;
        public List<TiberiumGarden> Gardens;

        public TiberiumFloraManager(Map map)
        {
            this.map = map;
        }

        public void ManagerTick()
        {

        }

    }

    public class TiberiumGarden : Area
    {
        private IntVec3 center;

        public override string Label => throw new NotImplementedException();

        public override Color Color => throw new NotImplementedException();

        public override int ListPriority => throw new NotImplementedException();

        public TiberiumGarden(AreaManager manager) : base(manager)
        {
        }

        public void GardenTick()
        {

        }

        private void CalculateCenter()
        {

        }

        public override string GetUniqueLoadID()
        {
            throw new NotImplementedException();
        }
    }
}
