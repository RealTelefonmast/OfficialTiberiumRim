using System.Collections.Generic;
using TR.Weaponry.SuperWeapon;
using Verse;

namespace TR.GameParts;

public class ASATNetwork : IExposable
{
    public List<AttackSatellite_Ion> ASatsIon = new();
    public List<AttackSatellite> AttackSatellites = new();

    public void ExposeData()
    {
        Scribe_Collections.Look(ref AttackSatellites, "attackSATS", LookMode.Reference);
        Scribe_Collections.Look(ref ASatsIon, "ionSats", LookMode.Reference);
    }

    public void RegisterNew(AttackSatellite sat)
    {
        AttackSatellites.Add(sat);
        if (sat is AttackSatellite_Ion ion) ASatsIon.Add(ion);
    }
}