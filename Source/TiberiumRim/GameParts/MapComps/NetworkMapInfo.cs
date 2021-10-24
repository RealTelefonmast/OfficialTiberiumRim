using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class NetworkMapInfo : MapInformation
    {
        private Dictionary<NetworkDef, NetworkMaster> NetworksByType = new Dictionary<NetworkDef, NetworkMaster>();
        private List<NetworkMaster> NetworkSystems = new List<NetworkMaster>();

        public NetworkMapInfo(Map map) : base(map)
        {
        }

        public NetworkMaster this[NetworkDef type] => NetworksByType.TryGetValue(type);

        public NetworkMaster GetOrCreateNewNetworkSystemFor(NetworkDef networkDef)
        {
            if (NetworksByType.TryGetValue(networkDef, out var network))
            {
                return network;
            }
            var networkMaster = new NetworkMaster(Map, networkDef);
            NetworksByType.Add(networkDef, networkMaster);
            NetworkSystems.Add(networkMaster);
            return networkMaster;
        }

        public void Notify_NewNetworkStructureSpawned(INetworkStructure structure)
        {
            foreach (var networkComponent in structure.NetworkParts)
            {
                var master = GetOrCreateNewNetworkSystemFor(networkComponent.NetworkDef);
                master.RegisterComponent(networkComponent);
            }
        }

        public void Notify_NetworkStructureDespawned(INetworkStructure structure)
        {
            foreach (var networkComponent in structure.NetworkParts)
            {
                GetOrCreateNewNetworkSystemFor(networkComponent.NetworkDef).DeregisterComponent(networkComponent);
            }
        }

        //Data Getters
        public bool HasConnectionAtFor(Thing thing, IntVec3 c)
        {
            var networkStructure = thing.TryGetComp<Comp_NetworkStructure>();
            if (networkStructure == null) return false;
            foreach (var networkPart in networkStructure.NetworkParts)
            {
                if (this[networkPart.NetworkDef].HasNetworkConnectionAt(c))
                {
                    return true;
                }
            }
            return false;
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
            foreach (var networkSystem in NetworkSystems)
            {
                networkSystem.DrawNetwork();
            }
        }
    }
}
