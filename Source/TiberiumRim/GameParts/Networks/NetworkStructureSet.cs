using System.Collections.Generic;

namespace TR.Networks
{
    public class NetworkStructureSet
    {
        private NetworkType networkType;
        private INetworkStructure parent;

        public HashSet<INetworkStructure> FullSet;
        public HashSet<INetworkStructure> Transmitters;
        public HashSet<INetworkStructure> Producers;
        public HashSet<INetworkStructure> Consumers;
        public HashSet<INetworkStructure> Storages;

        public NetworkStructureSet(){}

        public NetworkStructureSet(INetworkStructure parent, NetworkType type)
        {
            networkType = type;
            this.parent = parent;
        }

        public void AddNewStructure(INetworkStructure structure)
        {
            if (FullSet.Contains(structure) || structure == null) return;
            parent?.Notify_StructureAdded(structure);
            AddStructure(structure);
            //structure.StructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void AddStructure(INetworkStructure structure)
        {
            if (structure.NetworkType != networkType) return;
            if (FullSet.Contains(structure)) return;
            FullSet.Add(structure);
            switch (structure.NetworkRole)
            {
                case NetworkRole.Transmitter:
                    Transmitters.Add(structure);
                    break;
                case NetworkRole.Producer:
                    Producers.Add(structure);
                    break;
                case NetworkRole.Consumer:
                    Consumers.Add(structure);
                    break;
                case NetworkRole.Storage:
                    Storages.Add(structure);
                    break;
            }
        }

        public void RemoveStructure(INetworkStructure structure)
        {
            if (!FullSet.Contains(structure)) return;
            switch (structure.NetworkRole)
            {
                case NetworkRole.Transmitter:
                    Transmitters.Remove(structure);
                    break;
                case NetworkRole.Producer:
                    Producers.Remove(structure);
                    break;
                case NetworkRole.Consumer:
                    Consumers.Remove(structure);
                    break;
                case NetworkRole.Storage:
                    Storages.Remove(structure);
                    break;
            }
            FullSet.Remove(structure);
        }

    }
}
