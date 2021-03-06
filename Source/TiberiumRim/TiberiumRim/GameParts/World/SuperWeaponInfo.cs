﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class SuperWeaponInfo : WorldInfo
    {
        public List<SuperWeapon> SuperWeapons = new List<SuperWeapon>();

        public SuperWeaponInfo(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref SuperWeapons,  "SuperWeapons", LookMode.Deep);
        }

        public void TryRegisterSuperweapon(TRBuilding building)
        {
            var superWep = building.def.superWeapon;
            if (superWep == null) return;
            SuperWeapon wepWorker = (SuperWeapon)Activator.CreateInstance(superWep.worker);
            wepWorker.building = building;
            wepWorker.ticksUntilReady = superWep.chargeTime.SecondsToTicks();
            SuperWeapons.Add(wepWorker);
        }

        public void Notify_SuperWeaponFired(TRThingDef def)
        {

        }

    }
}
