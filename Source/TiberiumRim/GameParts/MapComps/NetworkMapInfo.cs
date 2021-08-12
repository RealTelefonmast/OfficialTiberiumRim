using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim.GameParts.MapComps
{
    public class NetworkMapInfo : MapInformation
    {
        private Dictionary<NetworkType, NetworkMaster> NetworksByType = new Dictionary<NetworkType, NetworkMaster>();
        private List<NetworkMaster> NetworkSystems = new List<NetworkMaster>();

        public NetworkMapInfo(Map map) : base(map)
        {
        }

        public NetworkMaster this[NetworkType type] => NetworksByType.TryGetValue(type);

        public NetworkMaster GetOrCreateNewNetworkSystemFor(NetworkType type)
        {
            if (NetworksByType.TryGetValue(type, out var network))
            {
                return network;
            }
            var networkMaster = new NetworkMaster(Map, type);
            NetworksByType.Add(type, networkMaster);
            NetworkSystems.Add(networkMaster);
            return networkMaster;
        }

        public void Notify_NewNetworkStructureSpawned(INetworkStructure structure)
        {
            var master = GetOrCreateNewNetworkSystemFor(structure.NetworkType);
            master.RegisterComponent(structure);
        }

        public void Notify_NetworkStructureDespawned(INetworkStructure structure)
        {
            GetOrCreateNewNetworkSystemFor(structure.NetworkType).DeregisterComponent(structure);
        }

        //Data Getters
        public bool HasConnectionAtFor(Thing thing, IntVec3 c)
        {
            var networkStructure = thing.TryGetComp<Comp_NetworkStructure>();
            if (networkStructure == null) return false;
            return this[networkStructure.NetworkType].HasNetworkStrucureAt(c);
        }

        public override void Tick()
        {
            base.Tick();
            foreach (var networkSystem in NetworkSystems)
            {
                networkSystem.TickNetwork();
            }
        }

        public override void Draw()
        {
            base.Draw();
            foreach (var networkSystem in NetworkSystems)
            {
                networkSystem.DrawNetwork();
            }
        }
    }
}
