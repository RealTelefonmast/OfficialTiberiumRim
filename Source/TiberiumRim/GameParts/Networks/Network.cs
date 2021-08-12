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
        protected NetworkMaster networkParent;

        protected NetworkType networkType;
        protected NetworkRank networkRank;
        public int NetworkID = -1;

        protected NetworkStructureSet structureSet;
        protected NetworkContainerSet containerSet;
        protected Map map;

        protected List<IntVec3> networkCells;

        //
        public INetworkStructure NetworkController { get; set; }

        //
        public virtual bool IsWorking => NetworkController.IsPowered;
        public virtual float TotalNetworkValue => ContainerSet.TotalNetworkValue;
        public virtual float TotalStorageNetworkValue => ContainerSet.TotalStorageValue;

        public List<IntVec3> NetworkCells => networkCells;

        public NetworkMaster NetworkParent => networkParent;
        public NetworkStructureSet StructureSet => structureSet;
        public NetworkContainerSet ContainerSet => containerSet;

        public Network(NetworkType type, Map map, NetworkMaster parent)
        {
            this.networkParent = parent;
            this.networkType = type;
            this.map = map;
            structureSet = new NetworkStructureSet();
            containerSet = new NetworkContainerSet();
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
        public float NetworkValueFor(Enum valueType)
        {
            return ContainerSet.TotalValueByType[valueType];
        }

        public float NetworkValueFor(NetworkRole ofRole)
        {
            return ContainerSet.TotalValueByRole[ofRole];
        }

        public float NetworkValueFor(Enum valueType, NetworkRole ofRole)
        {
            return ContainerSet.ValueByTypeByRole[ofRole][valueType];
        }

        //
        public bool ValidFor(NetworkRole role, out string reason)
        {
            reason = string.Empty;
            switch (role)
            {
                case NetworkRole.Consumer:
                    reason = "TR_ConsumerLack";
                    return StructureSet.FullSet.Any(x =>  x.NetworkRole == NetworkRole.Storage || x.NetworkRole == NetworkRole.Producer);
                case NetworkRole.Producer:
                    reason = "TR_ProducerLack";
                    return StructureSet.FullSet.Any(x => x.NetworkRole == NetworkRole.Storage || x.NetworkRole == NetworkRole.Consumer);
                case NetworkRole.Transmitter:
                    break;
                case NetworkRole.Storage:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
            return true;
        }

        public void AddStructure(INetworkStructure structure)
        {
            StructureSet.AddStructure(structure);
            ContainerSet.AddNewContainerFrom(structure);

            networkCells.AddRange(structure.ConnectionCells);
        }

        public void RemoveStructure(INetworkStructure structure)
        {
            structureSet.RemoveStructure(structure);
            foreach (var cell in structure.ConnectionCells)
            {
                networkCells.Remove(cell);
            }
        }

        //Network Gen
        public void Notify_MarkDirty()
        {

        }

        public void Notify_PotentialSplit(INetworkStructure from)
        {
            from.Network = null;
            Network newNet = null;
            foreach (INetworkStructure root in from.StructureSet.FullSet)
            {
                if (root.Network != newNet)
                {
                    newNet = root.Network = new Network(this.networkType, map, NetworkParent);
                }
            }
        }

        public override string ToString()
        {
            return $"{networkType}[{networkRank}]";
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
