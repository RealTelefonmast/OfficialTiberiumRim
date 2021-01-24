using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;

namespace TiberiumRim
{
    public class SatelliteInfo : WorldInfo
    {
        public ASATNetwork AttackSatelliteNetwork;

        public SatelliteInfo(World world) : base(world)
        {
            AttackSatelliteNetwork = new ASATNetwork();
        }
    }
}
