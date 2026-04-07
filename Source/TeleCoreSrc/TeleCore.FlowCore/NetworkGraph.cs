using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public struct NetEdge
    {
        internal readonly int _weight;

        //Direction
        private readonly INetworkComponent fromNode;
        private readonly INetworkComponent toNode;
    }

    public class NetworkGraph
    {
        //Graph Data
        private int _masterID = 0;
        private Dictionary<INetworkComponent, LinkedList<INetworkComponent>> _adjacencyLists;
        private Dictionary<(INetworkComponent, INetworkComponent), NetEdge> _edges;

        //Network Data
        protected NetworkDef def;
        protected NetworkRank networkRank = NetworkRank.Alpha;
        protected NetworkGraphMaster parentHolder;
        protected Map map;

        //Data Cache
        protected NetworkComponentSet componentSet;
        protected NetworkContainerSet containerSet;

        //
        public int NodeCount => _adjacencyLists.Count;
        public int EdgeCount => _edges.Count;

        //
        public NetworkDef Def => def;
        public NetworkRank NetworkRank => networkRank;
        public int ID { get; private set; } = -1;

        public NetworkGraphMaster NetworkParent => parentHolder;
        public NetworkComponentSet ComponentSet => componentSet;
        public NetworkContainerSet ContainerSet => containerSet;
        public INetworkStructure NetworkController => ComponentSet.Controller?.Parent;

        public virtual bool IsWorking => !def.UsesController || (NetworkController?.IsPowered ?? false);
        public virtual float TotalNetworkValue => ContainerSet.TotalNetworkValue;
        public virtual float TotalStorageNetworkValue => ContainerSet.TotalStorageValue;

        public NetworkGraph(NetworkDef def, Map map, NetworkGraphMaster parent)
        {
            this.def = def;
            this.parentHolder = parent;
            this.map = map;
            componentSet = new NetworkComponentSet(def, null);
            containerSet = new NetworkContainerSet();
        }

        public virtual void NetworkTick()
        {

        }

        public virtual void Draw()
        {

        }

        #region NodeStuff
        public void AddNode(INetworkComponent node)
        {
            _adjacencyLists.Add(node, new LinkedList<INetworkComponent>());
        }

        public void AddEdge(INetworkComponent source, INetworkComponent dest, NetEdge value)
        {
            _edges.Add((source, dest), value);
            
            if (!_adjacencyLists.TryGetValue(source, out var listSource))
            {
                AddNode(source);
                listSource = _adjacencyLists[source];
            }
            listSource.AddFirst(dest);
        }

        //
        public IEnumerable<INetworkComponent> GetAdjacentNodes(INetworkComponent node)
        {
            return _adjacencyLists[node];
        }

        public bool TryGetEdge(INetworkComponent source, INetworkComponent dest, out NetEdge value)
        {
            return _edges.TryGetValue((source, dest), out value);
        }

        public void AddNodeFrom(INetworkComponent comp)
        {
            AddNode(comp);

            _masterID++;
        }
        #endregion


    }
}
