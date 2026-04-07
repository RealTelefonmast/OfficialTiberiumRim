using UnityEngine;
using Verse;

namespace TR.TiberiumEnvironment;

public class Zone_Tiberium : Zone
{
    public HarvestType ZoneType = HarvestType.Unharvestable;

    public override Color NextZoneColor => Color.green;

    public override void PostRegister()
    {
        base.PostRegister();
    }
}