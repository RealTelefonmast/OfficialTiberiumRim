using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TurretProperties
    {
        public TurretTopProperties turretTop;
        public ThingDef turretGunDef;
        public Vector3 drawOffset;

        public float turretBurstWarmupTime;
        public float turretBurstCooldownTime = -1f;
        public float burstToRange = 10f;
        public float range = 10;
        public float minRange = 10;
        public int damage;

        //public TurretBurstMode burstMode = TurretBurstMode.Normal;
    }

    public class TurretHolderProps
    {
        public TurretHubProperties hub;
        public List<TurretProperties> turrets;
        public bool canForceTarget = false;
    }

    public class TurretTopProperties
    {
        public GraphicData turret;
        public List<TurretBarrelProperties> barrels;

        public float speed = 20f;
        public float aimAngle = 1.5f;

        public float resetSpeed = 5;
        public float recoilSpeed = 150;

        public Vector3 barrelMuzzleOffset = Vector3.zero;
        public IntRange idleDuration = new IntRange(50, 200);
        public IntRange idleInterval = new IntRange(150, 350);
    }

    public class TurretBarrelProperties
    {
        public GraphicData graphic;
        public float altitudeOffset = 0;
        public Vector3 barrelOffset = Vector3.zero;
        public Vector3 recoilOffset;
    }

    public class TurretHubProperties
    {
        public bool isHub = false;
        public TRThingDef hubDef;
        public TRThingDef turretDef;
        public int maxTurrets = 3;

    }
}
