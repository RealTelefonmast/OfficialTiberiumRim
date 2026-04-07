using TeleCore.MapInfo;

namespace TeleCore.MapComps.MapInfos;

public class RadiationMapInfo : MapInformation
{
    public RadiationMapInfo(Verse.Map map) : base(map)
    {
    }

    public override void InfoInit(bool initAfterReload = false)
    {
        base.InfoInit(initAfterReload);
    }
}