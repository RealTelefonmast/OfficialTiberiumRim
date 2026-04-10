using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ASATNetwork : IExposable
    {
        public List<AttackSatellite> AttackSatellites = new List<AttackSatellite>();
        public List<AttackSatellite_Ion> ASatsIon = new List<AttackSatellite_Ion>();

        public void ExposeData()
        {
        }

        public void RegisterNew(AttackSatellite sat)
        {
            AttackSatellites.Add(sat);
            if (sat is AttackSatellite_Ion ion)
            {
                ASatsIon.Add(ion);
            }
        }
    }
}
