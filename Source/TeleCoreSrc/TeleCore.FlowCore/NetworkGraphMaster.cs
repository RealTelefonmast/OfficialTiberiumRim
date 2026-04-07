using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public class NetworkGraphMaster
    {
        internal int _MasterID;

        private Map map;
        private BoolGrid validationGrid;
        private NetworkGraph[] lookUpGrid;

        //
        private NetworkComponent[] unfinishedEdges;

        private List<NetworkGraph> allNetworks;

        //Debug
        internal static bool DEBUG_DrawNetwork = false;

        public NetworkDef NetworkDef { get; }

        public NetworkGraphMaster(Map map, NetworkDef networkDef)
        {
            this.map = map;
            NetworkDef = networkDef;
            validationGrid = new BoolGrid(map);
            lookUpGrid = new NetworkGraph[map.cellIndices.NumGridCells];
        }

        public void RegisterComponent(NetworkComponent netPart, Comp_NetworkStructure netComp)
        {

        }

        public void Deregister(NetworkComponent netPart, Comp_NetworkStructure netComp)
        {

        }

        public void Notify_ThingForNetworkSpawned(Thing thing)
        {
            var netComp = thing.TryGetComp<Comp_NetworkStructure>();
            if (netComp == null) return;

            foreach (var part in netComp.NetworkParts)
            {
                //Add Node
                if(part.IsNode)
                    NodeSpawned(netComp, part);

                //Add Edge
                if(part.IsEdge)
                    EdgeSpawned(netComp, part);
            }

            //Try connect to existing graph
            foreach (var connection in netComp.ConnectionCells)
            {
                var graphAt = lookUpGrid[connection.Index(map)];
                if (graphAt == null) continue;

                AddNodeToGraph(graphAt, netComp);
            }

            //Otherwise create Graph
            AddNodeToGraph(new NetworkGraph(), netComp);
        }

        private void NodeSpawned(Comp_NetworkStructure netComp, NetworkComponent part)
        {
            //Add to graph

            //Merge Graph
        }

        private void EdgeSpawned(Comp_NetworkStructure netComp, NetworkComponent part)
        {
            //Add to graph

            //Merge Graph
        }

        public void Notify_ThingForNetworkDespawned(Thing thing)
        {

        }

        //Graphs
        private void AddNodeToGraph(NetworkGraph graph, Comp_NetworkStructure netComp)
        {
            graph.AddNodeFrom(netComp);

            //
            foreach (var cell in netComp.InnerConnectionCells)
            {
                lookUpGrid[cell.Index(map)] = graph;
                validationGrid[cell] = true;
            }
        }
    }
}
