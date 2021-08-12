using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiberiumRim.GameParts.Interfaces;

namespace TiberiumRim
{
    public class NetworkStructureSet : IStringCache
    {
        private NetworkType networkType;
        private INetworkStructure parent;

        public HashSet<INetworkStructure> FullSet;
        public HashSet<INetworkStructure> Transmitters;
        public HashSet<INetworkStructure> Producers;
        public HashSet<INetworkStructure> Consumers;
        public HashSet<INetworkStructure> Storages;

        public HashSet<INetworkStructure> this[NetworkRole role]
        {
            get
            {
                return role switch
                {
                    NetworkRole.Transmitter => Transmitters,
                    NetworkRole.Producer => Producers,
                    NetworkRole.Consumer => Consumers,
                    NetworkRole.Storage => Storages,
                    NetworkRole.All => FullSet,
                    _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
                };
            }
        }

        public string[] CachedStrings { get; set; }
        public bool Empty => !FullSet.Any();

        public NetworkStructureSet(){}

        public NetworkStructureSet(INetworkStructure parent, NetworkType type)
        {
            networkType = type;
            this.parent = parent;
            CachedStrings = new string[1];
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

            UpdateString(0);
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
            UpdateString(0);
        }

        //
        public void ParentDestroyed()
        {
            foreach (var ns in FullSet)
            {
                ns.StructureSet.RemoveStructure(parent);
            }
        }


        //
        public void UpdateString(int index)
        {
            if (index == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Transmitters: ");
                foreach (var ns in Transmitters)
                {
                    sb.AppendLine($"    - {ns.Thing}");
                }
                sb.AppendLine("Producers: ");
                foreach (var ns in Producers)
                {
                    sb.AppendLine($"    - {ns.Thing}");
                }
                sb.AppendLine("Consumers: ");
                foreach (var ns in Consumers)
                {
                    sb.AppendLine($"    - {ns.Thing}");
                }
                sb.AppendLine("Storages: ");
                foreach (var ns in Storages)
                {
                    sb.AppendLine($"    - {ns.Thing}");
                }
                sb.AppendLine($"Total Count: {FullSet.Count}");
                CachedStrings[0] = $"";
            }
        }

        public string CachedString(int index)
        {
            if(CachedStrings[index] == null)
                UpdateString(index);
            return CachedStrings[index];
        }

        public override string ToString()
        {
            return CachedString(0);
        }
    }
}
