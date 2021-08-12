using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class NetworkMaster
    {
        private Map map;
        public bool[] networkBools;
        public Network[] networkGrid;

        public List<Network> Networks = new List<Network>();
        public Dictionary<Network, List<IntVec3>> NetworkCells = new Dictionary<Network, List<IntVec3>>();

        public NetworkStructureSet MainStructureSet;
        public int MasterID = -1;

        public NetworkType NetworkType { get; }
        public INetworkStructure MainNetworkStructure { get; set; }

        //Debug
        private static bool ShouldShowNetwork = false;

        public NetworkMaster(Map map, NetworkType network)
        {
            this.map = map;
            NetworkType = network;
            networkBools = new bool[map.cellIndices.NumGridCells];
        }

        public void RegisterComponent(INetworkStructure structure)
        {
            MainStructureSet.AddStructure(structure);
            var network = RegenerateNetwork(structure);
            RegisterNetwork(network);
        }

        public void DeregisterComponent(INetworkStructure structure)
        {
            MainStructureSet.RemoveStructure(structure);
        }

        public void ToggleShowNetworks()
        {
            ShouldShowNetwork = !ShouldShowNetwork;
        }

        public void TickNetwork()
        {
            foreach (var network in Networks)
            {
                network.Tick();
            }
        }

        public void DrawNetwork()
        {
            if (!ShouldShowNetwork) return;
            foreach (var network in Networks)
            {
                network.Draw();
            }
            for (var i = 0; i < networkBools.Length; i++)
            {
                var cell = networkBools[i];
                if (cell)
                {
                    CellRenderer.RenderCell(map.cellIndices.IndexToCell(i), 0.75f);
                }
            }
        }

        public Network NetworkAt(IntVec3 c)
        {
            return networkGrid[map.cellIndices.CellToIndex(c)];
        }


        public bool HasNetworkStrucureAt(IntVec3 c)
        {
            return networkBools[map.cellIndices.CellToIndex(c)];
        }

        public Network RegenerateNetwork(INetworkStructure root)
        {
            Network newNet = new Network(root.NetworkType, map, this);
            HashSet<INetworkStructure> closedSet = new HashSet<INetworkStructure>();
            HashSet<INetworkStructure> openSet = new HashSet<INetworkStructure>() { root };
            HashSet<INetworkStructure> currentSet = new HashSet<INetworkStructure>();
            while (openSet.Count > 0)
            {
                foreach (INetworkStructure structure in openSet)
                {
                    structure.Network = newNet;
                    newNet.AddStructure(structure);
                    closedSet.Add(structure);
                }
                HashSet<INetworkStructure> hashSet = currentSet;
                currentSet = openSet;
                openSet = hashSet;
                openSet.Clear();
                foreach (INetworkStructure structure in currentSet)
                {
                    foreach (IntVec3 c in structure.ConnectionCells)
                    {
                        List<Thing> thingList = c.GetThingList(map);
                        foreach (var thing in thingList)
                        {
                            if (!Fits(thing, out INetworkStructure newStructure)) continue;
                            if (newStructure.NetworkType == root.NetworkType && !closedSet.Contains(newStructure) && newStructure.ConnectsTo(structure))
                            {
                                map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Buildings);
                                structure.StructureSet.AddNewStructure(newStructure);
                                newStructure.StructureSet.AddNewStructure(structure);
                                openSet.Add(newStructure);
                                break;
                            }
                        }
                    }
                }
            }
            return newNet;
        }

        //Check whether or not a thing is part of a network
        private bool Fits(Thing thing, out INetworkStructure structure)
        {
            structure = thing as INetworkStructure;
            structure ??= (thing as ThingWithComps).AllComps.Find(t => t is INetworkStructure) as INetworkStructure;
            return structure != null;
        }

        public void RegisterNetwork(Network tnw)
        {
            tnw.NetworkID = MasterID += 1;
            Networks.Add(tnw);
            NetworkCells.Add(tnw, tnw.NetworkCells);
            for (int i = 0; i < NetworkCells[tnw].Count; i++)
            {
                networkGrid[map.cellIndices.CellToIndex(NetworkCells[tnw][i])] = tnw;
            }
        }

        public void DeregisterNetwork(Network tnw)
        {
            if (!Networks.Contains(tnw)) return;
            for (int i = 0; i < NetworkCells[tnw].Count; i++)
            {
                networkGrid[map.cellIndices.CellToIndex(NetworkCells[tnw][i])] = null;
            }
            Networks.Remove(tnw);
            NetworkCells.Remove(tnw);
        }

        private Color ColorByNum(int num)
        {
            switch (num)
            {
                case 0:
                    return Color.blue;
                case 1:
                    return Color.cyan;
                case 2:
                    return Color.green;
                case 3:
                    return Color.magenta;
                case 4:
                    return Color.red;
                case 5:
                    return Color.yellow;
            }
            return Color.white;
        }
    }
}
