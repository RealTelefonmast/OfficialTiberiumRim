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
        private float totalValue, totalStorageValue;
        private Color networkColor = new Color(0,0,0,0);

        public HashSet<NetworkContainer> FullSet = new ();
        public HashSet<NetworkContainer> ProducerContainers = new ();
        public HashSet<NetworkContainer> ConsumerContainers = new ();
        public HashSet<NetworkContainer> StorageContainers = new ();

        public HashSet<NetworkValueDef> AllStoredTypes;

        public Dictionary<NetworkValueDef, float> TotalValueByType = new();
        public Dictionary<NetworkRole, float> TotalValueByRole = new();
        public Dictionary<NetworkRole, Dictionary<NetworkValueDef, float>> ValueByTypeByRole = new();

        public float TotalNetworkValue => totalValue;
        public float TotalStorageValue => totalStorageValue;

        public NetworkContainerSet()
        {

        }

        public bool AddNewContainerFrom(INetworkComponent component)
        {
            //TODO: Adjust value with existing values
            if (!component.HasContainer || FullSet.Contains(component.Container)) return false;
            AddContainerFrom(component, component.Container);
            return true;
            //structure.NeighbourStructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void RemoveContainerFrom(INetworkComponent component)
        {
            //TODO: Adjust value with existing values
            if (!FullSet.Contains(component.Container)) return;
            RemoveContainerFrom(component, component.Container);
            //structure.NeighbourStructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void Notify_AddedValue(NetworkValueDef type, float value)
        {
            totalValue += value;
            if(!TotalValueByType.TryAdd(type, value))
                TotalValueByType[type] += value;
        }

        public void Notify_RemovedValue(NetworkValueDef type, float value)
        {
            totalValue -= value;
            TotalValueByType[type] -= value;
        }

        private void AddContainerFrom(INetworkComponent component, NetworkContainer container)
        {
            //if (!(structure.Container is NetworkContainer container)) return;
            container.Notify_SetParentSet(this);
            FullSet.Add(container);
            switch (component.NetworkRole)
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

        private void RemoveContainerFrom(INetworkComponent component, NetworkContainer container)
        {
            if (!FullSet.Contains(container)) return;
            switch (component.NetworkRole)
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
