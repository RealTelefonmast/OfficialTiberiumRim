using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class NetworkContainerSet
    {
        private Color networkColor = new Color(0, 0, 0, 0);

        private readonly Dictionary<NetworkRole, HashSet<NetworkContainer>> ContainersByRole;

        private readonly Dictionary<NetworkValueDef, float> TotalValueByType = new();
        private readonly Dictionary<NetworkRole, float> TotalValueByRole = new();
        private Dictionary<NetworkRole, Dictionary<NetworkValueDef, float>> ValueByTypeByRole = new();

        private readonly HashSet<NetworkValueDef> AllStoredTypes;

        public float TotalNetworkValue => GetTotalValueByRole(NetworkRole.All);
        public float TotalStorageValue => GetTotalValueByRole(NetworkRole.Storage);
        public IEnumerable<NetworkValueDef> AllTypes => AllStoredTypes;

        public HashSet<NetworkContainer> this[NetworkRole role]
        {
            get
            {
                if (role == NetworkRole.All)
                    return ContainersByRole[NetworkRole.All];

                var set = new HashSet<NetworkContainer>();
                foreach (var @enum in role.AllFlags())
                {
                    if (ContainersByRole.TryGetValue(@enum, out var valueSet))
                        set.AddRange(valueSet);
                }

                return set;
            }
        }

        public float GetValueByType(NetworkValueDef def)
        {
            return TotalValueByType.GetValueOrDefault(def, 0);
        }

        public float GetTotalValueByRole(NetworkRole role)
        {
            //If getting all, simply return the existing dict entry
            if (role == NetworkRole.All)
                return TotalValueByRole.GetValueOrDefault(role, 0);

            //Otherwise, check for all flags
            float totalVal = 0;
            foreach (var @enum in role.AllFlags())
            {
                totalVal += TotalValueByRole.GetValueOrDefault(@enum, 0);
            }
            return totalVal;
        }

        public float GetValueByTypeByRole(NetworkValueDef type, NetworkRole inRole)
        {
            float totalVal = 0;
            foreach (var role in inRole.AllFlags())
                totalVal += ValueByTypeByRole.GetValueOrDefault(role, null)?.GetValueOrDefault(type, 0) ?? 0;

            return totalVal;
        }

        public NetworkContainerSet()
        {
            //
            ContainersByRole = new();
            ContainersByRole.Add(NetworkRole.All, new HashSet<NetworkContainer>());
            ContainersByRole.Add(NetworkRole.Producer, new HashSet<NetworkContainer>());
            ContainersByRole.Add(NetworkRole.Consumer, new HashSet<NetworkContainer>());
            ContainersByRole.Add(NetworkRole.Storage, new HashSet<NetworkContainer>());
            //
            TotalValueByRole = new();
            TotalValueByRole.Add(NetworkRole.All, 0);

            //
            AllStoredTypes = new HashSet<NetworkValueDef>();
        }

        public void Notify_AddedValue(NetworkValueDef type, float value, INetworkComponent comp)
        {
            //Increment total value
            TotalValueByRole[NetworkRole.All] += value;

            //Increment by type
            if (!TotalValueByType.TryAdd(type, value))
                TotalValueByType[type] += value;

            foreach (var enums in comp.NetworkRole.AllFlags())
            {
                //Increment by role
                if (!TotalValueByRole.TryAdd(enums, value))
                    TotalValueByRole[enums] += value;
                //Increment by type by role
                if (!ValueByTypeByRole.TryAdd(enums, new Dictionary<NetworkValueDef, float>() { { type, value } }))
                    if (!ValueByTypeByRole[enums].TryAdd(type, value))
                        ValueByTypeByRole[enums][type] += value;
            }

            //Add type to known types
            AllStoredTypes.Add(type);
        }

        public void Notify_RemovedValue(NetworkValueDef type, float value, INetworkComponent comp)
        {
            TotalValueByRole[NetworkRole.All] -= value;

            //
            if (!TotalValueByType.ContainsKey(type))
            {
                TLog.Warning($"Tried to remove ({value}){type} in ContainerSet but {type} is not stored!");
            }
            else
            {
                TotalValueByType[type] -= value;
                //Remove type if empty
                if (TotalValueByType[type] <= 0)
                    AllStoredTypes.Remove(type);
            }

            //
            foreach (var enums in comp.NetworkRole.AllFlags())
            {
                if (!TotalValueByRole.ContainsKey(enums))
                {
                    TLog.Warning($"Tried to remove ({value}){type} for role {enums} in ContainerSet but {enums} is not stored!");
                }
                else
                {
                    TotalValueByRole[enums] -= value;
                    //if(TotalValueByRole[comp.NetworkRole] <= 0)
                }

                if (ValueByTypeByRole.ContainsKey(enums))
                {
                    if (ValueByTypeByRole[enums].ContainsKey(type))
                    {
                        ValueByTypeByRole[enums][type] -= value;
                    }
                }
            }
        }

        public bool AddNewContainerFrom(INetworkComponent component)
        {
            //TODO: Adjust value with existing values
            if (!component.HasContainer || this[NetworkRole.All].Contains(component.Container)) return false;
            AddContainerFrom(component, component.Container);
            return true;
            //structure.NeighbourStructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        public void RemoveContainerFrom(INetworkComponent component)
        {
            //TODO: Adjust value with existing values
            if (!this[NetworkRole.All].Contains(component.Container)) return;
            RemoveContainerFrom(component, component.Container);
            //structure.NeighbourStructureSet.AddStructure(parent, cell + parent?.Thing?.Position.PositionOffset(cell) ?? IntVec3.Invalid);
        }

        private void AddContainerFrom(INetworkComponent component, NetworkContainer container)
        {
            foreach (var @enum in component.NetworkRole.AllFlags())
            {
                if (!ContainersByRole.ContainsKey(@enum))
                    ContainersByRole.Add(@enum, new());
                ContainersByRole[@enum].Add(container);
            }

            //Adjust values
            foreach (var values in container.StoredValuesByType)
            {
                Notify_AddedValue(values.Key, values.Value, component);
            }
        }

        private void RemoveContainerFrom(INetworkComponent component, NetworkContainer container)
        {
            if (!this[NetworkRole.All].Contains(container)) return;
            foreach (var @enum in component.NetworkRole.AllFlags())
            {
                this[@enum].Remove(container);
            }

            //Adjust values
            foreach (var values in container.StoredValuesByType)
            {
                Notify_RemovedValue(values.Key, values.Value, component);
            }
        }

    }
}
