using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

public class TurretTopProperties
{
    public float aimSpeed = 20f;
    public List<TurretBarrelProperties>? barrels;

    public IntRange idleDuration = new(50, 200);
    public IntRange idleInterval = new(150, 350);
    public float recoilSpeed = 150;

    //
    public float resetSpeed = 5;
    public GraphicData topGraphic;
}