using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TiberiumRim
{
    public class NetworkContainerSet
    {
        private Type networkValueType;
        private float totalValue, totalStorageValue;
        private Color networkColor = new Color(0,0,0,0);

        public HashSet<NetworkContainer> FullSet = new ();
        public HashSet<NetworkContainer> ProducerContainers = new ();
        public HashSet<NetworkContainer> ConsumerContainers = new ();
        public HashSet<NetworkContainer> StorageContainers = new ();

        public HashSet<Enum> AllStoredTypes;

        public Dictionary<Enum, float> TotalValueByType = new();
        public Dictionary<NetworkRole, float> TotalValueByRole = new();
        public Dictionary<NetworkRole, Dictionary<Enum, float>> ValueByTypeByRole = new();

        public float TotalNetworkValue => totalValue;
        public float TotalStorageValue => totalStorageValue;

        public NetworkContainerSet()
        {

        }

        public void AddNewContainerFrom(INetworkStructure structure)
        {
            //TODO: Adjust value with existing values
            if (FullSet.Contains(structure.Container)) return;
            AddContainerFrom(structure, structure.Container);
            //structure.StructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void RemoveContainerFrom(INetworkStructure structure)
        {
            //TODO: Adjust value with existing values
            if (!FullSet.Contains(structure.Container)) return;
            RemoveContainerFrom(structure, structure.Container);
            //structure.StructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void Notify_AddedValue(Enum type, float value)
        {

        }

        public void Notify_RemovedValue(Enum type, float value)
        {

        }

        private void AddContainerFrom(INetworkStructure structure, NetworkContainer container)
        {
            //if (!(structure.Container is NetworkContainer container)) return;
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

        public void RemoveContainerFrom(INetworkStructure structure, NetworkContainer container)
        {
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
