using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public enum TurretBurstMode
    {
        Normal,
        ToTarget
    }

    public class TurretProperties
    {
        public GraphicData graphic;
        public ThingDef turretGunDef;
        public float turretBurstWarmupTime;
        public float turretBurstCooldownTime = -1f;
        public float burstToRange = 10f;
        public TurretBurstMode burstMode = TurretBurstMode.Normal;
        public bool hasTurret = true;

        public float range = 10;
        public float minRange = 10;
        public int damage;

        public float speed = 20f;
        public float aimAngle = 1.5f;

        public Vector3 drawOffset;
        public Vector3 barrelOffset;
        public IntRange idleDuration = new IntRange(50, 200);
        public IntRange idleInterval = new IntRange(150, 350);
    }

    public class TurretHolderProps
    {
        public List<TurretProperties> turrets;
        public bool canForceTarget = false;
        
    }
}
