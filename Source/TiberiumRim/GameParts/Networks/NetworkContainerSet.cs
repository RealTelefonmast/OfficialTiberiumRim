using System;
using System.Collections.Generic;
using System.Linq;

namespace TR.GameParts.Networks
{
    public class NetworkContainerSet<T> where T : Enum
    {
        private T containerType;

        public HashSet<NetworkContainer<T>> FullSet = new HashSet<NetworkContainer<T>>();
        public HashSet<NetworkContainer<T>> ProducerContainers = new HashSet<NetworkContainer<T>>();
        public HashSet<NetworkContainer<T>> ConsumerContainers = new HashSet<NetworkContainer<T>>();
        public HashSet<NetworkContainer<T>> StorageContainers = new HashSet<NetworkContainer<T>>();

        public HashSet<T> AllStoredTypes;

        public NetworkContainerSet() { }

        public void AddNewContainerFrom(INetworkStructure structure)
        {
            if (FullSet.Contains(structure.ContainerObject)) return;
            AddContainerFrom(structure);
            //structure.StructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void Notify_ChangedValue(T type, float finalValue)
        {

        }

        public void AddContainerFrom(INetworkStructure structure)
        {
            if (!(structure.ContainerObject is NetworkContainer<T> container)) return;
            FullSet.Add(container);
            switch (structure.NetworkRole)
            {
                case NetworkRole.Producer:
                    ProducerContainers.Add(container);
                    break;
                case NetworkRole.Consumer:
                    ConsumerContainers.Add(container);
                    break;
                case NetworkRole.Storage:
                    StorageContainers.Add(container);
                    break;
            }
        }

        public void RemoveContainerFrom(INetworkStructure structure)
        {
            if (!(structure.ContainerObject is NetworkContainer<T> container)) return;
            if (!FullSet.Contains(container)) return;
            switch (structure.NetworkRole)
            {
                case NetworkRole.Producer:
                    ProducerContainers.Remove(container);
                    break;
                case NetworkRole.Consumer:
                    ConsumerContainers.Remove(container);
                    break;
                case NetworkRole.Storage:
                    StorageContainers.Remove(container);
                    break;
            }
            FullSet.Remove(container);
        }

    }
}
