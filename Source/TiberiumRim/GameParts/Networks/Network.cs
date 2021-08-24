using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class Network : IExposable
    {
        //
        protected NetworkDef def;
        protected NetworkMaster networkParent;
        protected Map map;

        protected NetworkRank networkRank = NetworkRank.Alpha;
        public int NetworkID = -1;

        protected NetworkComponentSet componentSet;
        protected NetworkContainerSet containerSet;

        //
        public INetworkStructure NetworkController => ComponentSet.Controller?.Parent;

        //
        public virtual bool IsWorking => NetworkController?.IsPowered ?? false;
        public virtual float TotalNetworkValue => ContainerSet.TotalNetworkValue;
        public virtual float TotalStorageNetworkValue => ContainerSet.TotalStorageValue;

        public NetworkRank NetworkRank => networkRank;

        public List<IntVec3> NetworkCells { get; set; }

        public NetworkMaster NetworkParent => networkParent;
        public NetworkComponentSet ComponentSet => componentSet;
        public NetworkContainerSet ContainerSet => containerSet;

        public Network(NetworkDef def, Map map, NetworkMaster parent)
        {
            this.networkParent = parent;
            this.map = map;
            componentSet = new NetworkComponentSet(def, null);
            containerSet = new NetworkContainerSet();
            NetworkCells = new List<IntVec3>();
        }

        public virtual void ExposeData()
        {

        }

        public virtual void Tick()
        {

        }

        public virtual void Draw()
        {

        }

        //
        public float NetworkValueFor(NetworkValueDef valueDef)
        {
            return ContainerSet.TotalValueByType[valueDef];
        }

        public float NetworkValueFor(NetworkRole ofRole)
        {
            return ContainerSet.TotalValueByRole[ofRole];
        }

        public float NetworkValueFor(NetworkValueDef valueDef, NetworkRole ofRole)
        {
            return ContainerSet.ValueByTypeByRole[ofRole][valueDef];
        }

        //
        public bool ValidFor(NetworkRole role, out string reason)
        {
            reason = string.Empty;
            NetworkRole[] values = (NetworkRole[])Enum.GetValues(typeof(NetworkRole));
            foreach (var value in values)
            {
                if ((role & value) == value)
                {
                    switch (value)
                    {
                        case NetworkRole.Consumer:
                            reason = "TR_ConsumerLack";
                            return ComponentSet.FullSet.Any(x => x.NetworkRole.HasFlag(NetworkRole.Storage) || x.NetworkRole.HasFlag(NetworkRole.Producer));
                        case NetworkRole.Producer:
                            reason = "TR_ProducerLack";
                            return ComponentSet.FullSet.Any(x => x.NetworkRole.HasFlag(NetworkRole.Storage) || x.NetworkRole.HasFlag(NetworkRole.Consumer));
                        case NetworkRole.Transmitter:
                            break;
                        case NetworkRole.Storage:
                            break;
                        case NetworkRole.None:
                            break;
                        case NetworkRole.Controller:
                            break;
                        case NetworkRole.AllContainers:
                            break;
                        case NetworkRole.All:
                            break;
                        default: break;
                    }
                }
            }
            return true;
        }

        public void AddComponent(INetworkComponent component)
        {
            if (ComponentSet.AddNewComponent(component))
                NetworkCells.AddRange(component.Parent.InnerConnectionCells);

            ContainerSet.AddNewContainerFrom(component);
        }

        public void RemoveComponent(INetworkComponent component)
        {
            ComponentSet.RemoveComponent(component);
            containerSet.RemoveContainerFrom(component);
            foreach (var cell in component.Parent.InnerConnectionCells)
            {
                NetworkCells.Remove(cell);
            }
        }

        //Network Gen
        public void Notify_MarkDirty()
        {

        }

        /*
        public void Notify_PotentialSplit(INetworkStructure from)
        {
            from.Network = null;
            Network newNet = null;
            foreach (INetworkStructure root in from.NeighbourStructureSet.FullSet)
            {
                if (root.Network != newNet)
                {
                    newNet = root.Network = new Network(this.networkType, map, NetworkParent);
                }
            }
        }
        */

        public override string ToString()
        {
            return $"{def}[{networkRank}]";
        }

        public string GreekLetter
        {
            get
            {
                switch (networkRank)
                {
                    case NetworkRank.Alpha:
                        return "α";
                    case NetworkRank.Beta:
                        return "β";
                    case NetworkRank.Gamma:
                        return "γ";
                    case NetworkRank.Delta:
                        return "δ";
                    case NetworkRank.Epsilon:
                        return "ε";
                }
                return "";
            }
        }
    }
}
