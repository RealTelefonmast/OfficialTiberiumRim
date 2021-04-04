using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public abstract class WorldInfo : IExposable
    {
        private World world;

        public WorldInfo(World world)
        {
            this.world = world;
        }

        public virtual void ExposeData()
        {
        }

        public virtual void Setup()
        {
        }

        public virtual void InfoTick()
        {
        }
    }
}
