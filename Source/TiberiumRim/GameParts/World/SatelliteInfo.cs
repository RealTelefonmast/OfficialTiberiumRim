using TR.GameParts.WorldInfos;

namespace TR.GameParts;

public class SatelliteInfo : WorldInformation
{
    public ASATNetwork AttackSatelliteNetwork;

    public SatelliteInfo(World world) : base(world)
    {
        AttackSatelliteNetwork = new ASATNetwork();
    }
}