using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TeleCore
{
    public class NetworkComponentSet : IStringCache
    {
        private NetworkDef def;
        private INetworkComponent parent;

        public INetworkComponent Controller;
        public Dictionary<NetworkRole, HashSet<INetworkComponent>> StructuresByRole;

        public HashSet<INetworkComponent> FullSet;
        public HashSet<INetworkComponent> Transmitters;
        public HashSet<INetworkComponent> Producers;
        public HashSet<INetworkComponent> Consumers;
        public HashSet<INetworkComponent> Storages;

        public HashSet<INetworkComponent> this[NetworkRole role]
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
        public bool Empty => !Enumerable.Any(FullSet);

        public NetworkComponentSet(NetworkDef def, INetworkComponent parent)
        {
            this.def = def;
            this.parent = parent;

            CachedStrings = new string[1];

            FullSet = new HashSet<INetworkComponent>();
            Transmitters = new HashSet<INetworkComponent>();
            Producers = new HashSet<INetworkComponent>();
            Consumers = new HashSet<INetworkComponent>();
            Storages = new HashSet<INetworkComponent>();

            StructuresByRole = new Dictionary<NetworkRole, HashSet<INetworkComponent>>();
            StructuresByRole.Add(NetworkRole.All, FullSet);
            StructuresByRole.Add(NetworkRole.Transmitter, Transmitters);
            StructuresByRole.Add(NetworkRole.Producer, Producers);
            StructuresByRole.Add(NetworkRole.Consumer, Consumers);
            StructuresByRole.Add(NetworkRole.Storage, Storages);

            /*
            foreach (NetworkRole value in typeof(NetworkRole).GetEnumValues())
            {
                StructuresByRole.Add(value, new HashSet<INetworkComponent>());
            }
            */

        }

        public bool AddNewComponent(INetworkComponent component)
        {
            if (FullSet.Contains(component) || component == null) return false;
            AddComponent(component);
            return true;
        }

        private void AddComponent(INetworkComponent component)
        {
            if (component.NetworkDef != def) return;
            if (FullSet.Contains(component)) return;
            FullSet.Add(component);

            if (component.NetworkRole.HasFlag(NetworkRole.Controller))
            {
                Controller = component;
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Transmitter))
            {
                Transmitters.Add(component);
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Producer))
            {
                Producers.Add(component);
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Consumer))
            {
                Consumers.Add(component);
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Storage))
            {
                Storages.Add(component);
            }
            UpdateString(0);
        }

        public void RemoveComponent(INetworkComponent component)
        {
            if (!FullSet.Contains(component)) return;
            if (component.NetworkRole.HasFlag(NetworkRole.Controller))
            {
                Controller = null;
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Transmitter))
            {
                Transmitters.Remove(component);
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Producer))
            {
                Producers.Remove(component);
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Consumer))
            {
                Consumers.Remove(component);
            }
            if (component.NetworkRole.HasFlag(NetworkRole.Storage))
            {
                Storages.Remove(component);
            }
            FullSet.Remove(component);
            UpdateString(0);
        }

        //
        public void ParentDestroyed()
        {
            foreach (var ns in FullSet)
            {
                ns.Notify_NewComponentRemoved(parent);
            }
        }

        //
        public void UpdateString(int index)
        {
            CachedStrings ??= new string[1];
            if (index == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"CONTROLLER: {Controller?.Parent.Thing}");
                sb.AppendLine("Transmitters: ");
                foreach (var ns in Transmitters)
                {
                    sb.AppendLine($"    - {ns.Parent.Thing}");
                }
                sb.AppendLine("Producers: ");
                foreach (var ns in Producers)
                {
                    sb.AppendLine($"    - {ns.Parent.Thing}");
                }
                sb.AppendLine("Consumers: ");
                foreach (var ns in Consumers)
                {
                    sb.AppendLine($"    - {ns.Parent.Thing}");
                }
                sb.AppendLine("Storages: ");
                foreach (var ns in Storages)
                {
                    sb.AppendLine($"    - {ns.Parent.Thing}");
                }
                sb.AppendLine($"Total Count: {FullSet.Count}");
                CachedStrings[0] = sb.ToString();
            }
        }

        public string CachedString(int index)
        {
            if (CachedStrings[index] == null)
                UpdateString(index);
            return CachedStrings[index];
        }

        public override string ToString()
        {
            return CachedString(0);
        }
    }
}
