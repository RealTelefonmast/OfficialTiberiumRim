using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TR;

public class TurretProperties
{
    public float burstToRange = 10f;
    public bool continuous = false;
    public int damage;
    public Vector3 drawOffset;
    public float minRange = 10;
    public float range = 10;
    public float turretBurstCooldownTime = -1f;

    public float turretBurstWarmupTime;
    public Type turretGunClass = typeof(TurretGun);
    public ThingDef turretGunDef;
    public TurretTopProperties turretTop;

    //public TurretBurstMode burstMode = TurretBurstMode.Normal;
}

public class TurretHolderProperties
{
    public bool canForceTarget = false;
    public TurretHubProperties hub;
    public List<TurretProperties> turrets;
}

public class TurretTopProperties
{
    public float aimAngle = 1.5f;

    public Vector3 barrelMuzzleOffset = Vector3.zero;
    public List<TurretBarrelProperties> barrels;
    public IntRange idleDuration = new(50, 200);
    public IntRange idleInterval = new(150, 350);
    public float recoilSpeed = 150;

    public float resetSpeed = 5;

    public float speed = 20f;
    public GraphicData turret;
}

public class TurretBarrelProperties
{
    public float altitudeOffset = 0;
    public Vector3 barrelOffset = Vector3.zero;
    public GraphicData graphic;
    public Vector3 recoilOffset;
}

public class TurretHubProperties
{
    public GraphicData cableGraphic;
    public string cableTexturePath;
    public float connectRadius = 7.9f;
    public TRThingDef hubDef;
    public bool isHub = false;
    public int maxTurrets = 3;
    public TRThingDef turretDef;
}