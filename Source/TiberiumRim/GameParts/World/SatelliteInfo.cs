using TR.SuperWeapon;
using TR.WorldInfos;

namespace TR;

public class SatelliteInfo : WorldInformation
{
    public ASATNetwork AttackSatelliteNetwork;

    public SatelliteInfo(World world) : base(world)
    {
        AttackSatelliteNetwork = new ASATNetwork();
    }
}