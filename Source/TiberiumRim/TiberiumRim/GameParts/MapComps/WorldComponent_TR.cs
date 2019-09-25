using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class WorldComponent_TR : WorldComponent
    {
        public ASATNetwork AttackSatelliteNetwork;
        public List<SuperWeaponInfo> SuperWeapons = new List<SuperWeaponInfo>();

        public WorldComponent_TR(World world) : base(world)
        {
            AttackSatelliteNetwork = new ASATNetwork();
        }

        public void TryRegisterSuperweapon(TRBuilding building)
        {
            var superWep = building.def.superWeapon;
            if (superWep == null) return;
            SuperWeaponInfo info = (SuperWeaponInfo) Activator.CreateInstance(superWep.worker);
            info.building = building;
            info.ticksUntilReady = superWep.chargeTime.SecondsToTicks();

            SuperWeapons.Add(info);
        }

        public void Notify_SuperWeaponFired(TRThingDef def)
        {

        }
    }
}
