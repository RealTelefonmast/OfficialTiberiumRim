using Verse;

namespace TeleCore.TeleTurrets.Hubs;

public class TurretHubProperties
{
    public GraphicData cableGraphic;
    public string cableTexturePath;
    public float connectRadius = 7.9f;
    public ThingDef hubDef;
    public bool isHub = false;
    public int maxTurrets = 3;
    public ThingDef turretDef;
}