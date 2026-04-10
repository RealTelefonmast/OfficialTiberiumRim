using System.Collections.Generic;
using Verse;

namespace TR.Networks
{
    public interface INetworkStructure
    {
        public Thing Thing { get; }
        public NetworkType NetworkType { get; }
        public TiberiumProcessing.NetworkMode NetworkMode { get; }
        public NetworkStructureSet StructureSet { get; }
        public NetworkRole NetworkRole { get; }
        public Network Network { get; set; }
        public NetworkStructureSet StructureSet { get; }
        public object ContainerObject { get; }

        IEnumerable<IntVec3> ConnectionCells { get; }
        void Notify_StructureAdded(INetworkStructure other);
        void Notify_StructureRemoved(INetworkStructure other);
        bool ConnectsTo(INetworkStructure other);
    }
}
