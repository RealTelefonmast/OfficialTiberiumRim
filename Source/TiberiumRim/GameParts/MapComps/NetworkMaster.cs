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

        public NetworkComponentSet TotalComponentSet;
        public int MasterID = -1;

        public NetworkDef NetworkType { get; }
        public INetworkComponent MainNetworkComponent { get; set; }

        //Debug
        private static bool ShouldShowNetwork = false;

        public NetworkMaster(Map map, NetworkDef networkDef)
        {
            this.map = map;
            NetworkType = networkDef;
            TotalComponentSet = new NetworkComponentSet(networkDef, null);
            networkBools = new bool[map.cellIndices.NumGridCells];
            networkGrid = new Network[map.cellIndices.NumGridCells];
        }

        public void RegisterComponent(INetworkComponent component)
        {
            TotalComponentSet.AddNewComponent(component);
            var network = RegenerateNetwork(component);
            RegisterNetwork(network);
        }

        public void DeregisterComponent(INetworkComponent component)
        {
            DeregisterNetworkPart(component);
            TotalComponentSet.RemoveComponent(component);
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


        public bool HasNetworkConnectionAt(IntVec3 c)
        {
            return networkBools[map.cellIndices.CellToIndex(c)];
        }

        public Network RegenerateNetwork(INetworkComponent root)
        {
            Log.Message($"Regenerating new net from {root.Parent.Thing}");
            Network newNet = new Network(root.NetworkDef, map, this);
            HashSet<INetworkComponent> closedSet = new HashSet<INetworkComponent>();
            HashSet<INetworkComponent> openSet = new HashSet<INetworkComponent>() { root };
            HashSet<INetworkComponent> currentSet = new HashSet<INetworkComponent>();
            while (openSet.Count > 0)
            {
                foreach (INetworkComponent component in openSet)
                {
                    component.Network = newNet;
                    newNet.AddComponent(component);
                    closedSet.Add(component);
                }
                HashSet<INetworkComponent> hashSet = currentSet;
                currentSet = openSet;
                openSet = hashSet;
                openSet.Clear();
                foreach (INetworkComponent component in currentSet)
                {
                    foreach (IntVec3 c in component.Parent.ConnectionCells)
                    {
                        List<Thing> thingList = c.GetThingList(map);
                        foreach (var thing in thingList)
                        {
                            if (!Fits(thing, component.NetworkDef, out INetworkComponent newComponent)) continue;
                            if (!closedSet.Contains(newComponent) && newComponent.ConnectsTo(component))
                            {
                                map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Buildings);
                                component.Notify_NewComponentAdded(newComponent);
                                newComponent.Notify_NewComponentAdded(component);
                                openSet.Add(newComponent);
                                break;
                            }
                        }
                    }
                }
            }
            return newNet;
        }

        //Check whether or not a thing is part of a network
        private bool Fits(Thing thing, NetworkDef forNetwork, out INetworkComponent component)
        {
            //component = thing as INetworkStructure;
            INetworkStructure structure = (thing as ThingWithComps)?.AllComps.Find(t => t is INetworkStructure) as INetworkStructure;
            component = structure?.NetworkParts.Find(c => c.NetworkDef == forNetwork);
            return component != null;
        }

        public void RegisterNetwork(Network tnw)
        {
            tnw.NetworkID = MasterID += 1;
            Networks.Add(tnw);
            NetworkCells.Add(tnw, tnw.NetworkCells);
            for (int i = 0; i < NetworkCells[tnw].Count; i++)
            {
                int index = map.cellIndices.CellToIndex(NetworkCells[tnw][i]);
                networkBools[index] = true;
                networkGrid[index] = tnw;
            }
        }

        public void DeregisterNetwork(Network tnw)
        {
            if (!Networks.Contains(tnw)) return;
            for (int i = 0; i < NetworkCells[tnw].Count; i++)
            {
                int index = map.cellIndices.CellToIndex(NetworkCells[tnw][i]);
                networkBools[index] = false;
                networkGrid[index] = null;
            }
            Networks.Remove(tnw);
            NetworkCells.Remove(tnw);
        }

        public void DeregisterNetworkPart(INetworkComponent component)
        {
            foreach (var cell in component.Parent.InnerConnectionCells)
            {
                int index = map.cellIndices.CellToIndex(cell);
                networkBools[index] = false;
                networkGrid[index] = null;
            }

            if (NetworkCells.ContainsKey(component.Network))
            {
                NetworkCells[component.Network].RemoveAll(component.Parent.InnerConnectionCells.Contains);
            }
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
