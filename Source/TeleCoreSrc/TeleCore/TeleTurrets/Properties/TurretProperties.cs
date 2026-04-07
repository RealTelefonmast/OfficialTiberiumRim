using System;
using UnityEngine;
using Verse;

namespace TeleCore.TeleTurrets.Properties;

public class TurretProperties
{
    public bool canForceTarget = false;

    public bool continuous = false;
    public string label = "Turret Gun";
    public float turretBurstCooldownTime = -1f;

    public float turretBurstWarmupTime;
    public Type turretGunClass = typeof(TurretGun);
    public ThingDef turretGunDef;
    public float turretInitialCooldownTime;
    public Vector3 turretOffset;
    public TurretTopProperties turretTop;

    //public TurretBurstMode burstMode = TurretBurstMode.Normal;
}