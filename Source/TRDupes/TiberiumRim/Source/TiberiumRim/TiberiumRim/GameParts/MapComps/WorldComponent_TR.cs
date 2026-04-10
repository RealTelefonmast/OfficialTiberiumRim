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

        //Discovery
        public DiscoveryTable DiscoveryTable;

        public WorldComponent_TR(World world) : base(world)
        {
            AttackSatelliteNetwork = new ASATNetwork();
            DiscoveryTable = new DiscoveryTable();
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
