using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class ASATNetwork : IExposable
    {
        public List<AttackSatellite> AttackSatellites = new List<AttackSatellite>();
        public List<AttackSatellite_Ion> ASatsIon = new List<AttackSatellite_Ion>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref AttackSatellites, "attackSATS", LookMode.Reference);
            Scribe_Collections.Look(ref ASatsIon, "ionSats", LookMode.Reference);
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
