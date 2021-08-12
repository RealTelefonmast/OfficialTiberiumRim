using System;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public interface INetworkStructure
    {
        //Data References
        public Thing Thing { get; }
        public Network Network { get; set; }
        public NetworkStructureSet StructureSet { get; }
        public NetworkContainer Container { get; }

        //Internal Data
        public bool IsPowered { get; }
        public bool HasLeak { get; }
        public bool HasConnection { get; }

        public NetworkType NetworkType { get; }
        public NetworkRole NetworkRole { get; }

        IEnumerable<IntVec3> ConnectionCells { get; }

        //Methods
        void Notify_StructureAdded(INetworkStructure other);
        void Notify_StructureRemoved(INetworkStructure other);
        bool ConnectsTo(INetworkStructure other);
    }
}
