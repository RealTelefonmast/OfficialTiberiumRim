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
        public List<TiberiumPond> Ponds;

        public TiberiumFloraManager(Map map)
        {
            this.map = map;
        }

        public void ManagerTick()
        {

        }

        public void Notify_PlantSpawnedFromOutside(TiberiumPlant plant)
        {

        }

    }
}
