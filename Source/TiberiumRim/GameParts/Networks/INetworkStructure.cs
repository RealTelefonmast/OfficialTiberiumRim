using System;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public interface INetworkComponent
    {
        public INetworkStructure Parent { get; }

        public NetworkDef NetworkDef { get; }
        public Network Network { get; set; }
        public NetworkContainer Container { get; }
        public NetworkComponentSet ConnectedComponentSet { get; }

        public bool IsMainController { get; }
        public bool HasLeak { get; }
        public bool HasConnection { get; }
        public bool HasContainer { get; }
        public bool IsReceiving { get; set; }

        public NetworkRole NetworkRole { get; }

        bool ConnectsTo(INetworkComponent other);
        bool NeedsValue(NetworkValueDef value);

        void Notify_NewComponentAdded(INetworkComponent component);
        void Notify_NewComponentRemoved(INetworkComponent component);
    }

    public interface INetworkStructure
    {
        //Data References
        public Thing Thing { get; }
        public List<NetworkComponent> NetworkParts { get; }

        //Internal Data
        public bool IsPowered { get; }

        public IntVec3[] InnerConnectionCells { get; }
        public IntVec3[] ConnectionCells { get; }

        //Methods
        void Notify_StructureAdded(INetworkStructure other);
        void Notify_StructureRemoved(INetworkStructure other);
    }
}
