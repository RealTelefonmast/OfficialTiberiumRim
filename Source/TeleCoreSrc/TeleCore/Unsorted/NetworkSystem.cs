using System;
using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted
{
    internal enum DelayedNetworkActionType
    {
        Register,
        Deregister
    }

    internal struct DelayedNetworkAction
    {
        internal DelayedNetworkActionType type;
        internal INetworkComponent component;

        public DelayedNetworkAction(DelayedNetworkActionType type, INetworkComponent component)
        {
            this.type = type;
            this.component = component;
        }
    }

    public class NetworkSystem
    {
        private Map map;
        private BoolGrid validationGrid;
        private Network[] networkAccessGrid;

        private List<Network> allNetworks = new List<Network>();
        private List<DelayedNetworkAction> delayedActions = new();

        public NetworkDef NetworkDef { get; }
        public NetworkComponentSet FullComponentSet;
        public List<Network> AllNetworksList => allNetworks;

        public NetworkSystem()
        {
        }

        public void RegisterComponent(INetworkComponent component)
        {
            delayedActions.Add(new DelayedNetworkAction(DelayedNetworkActionType.Register, component));
        }

        public void DeregisterComponent(INetworkComponent component)
        {
            delayedActions.Add(new DelayedNetworkAction(DelayedNetworkActionType.Deregister, component));
        }

        public void TickNetworks()
        {
            //Update Networks
            UpdateDelayedActions();

            for (var i = 0; i < allNetworks.Count; i++)
            {
                allNetworks[i].Tick();
            }
        }

        private void UpdateDelayedActions()
        {
            if (delayedActions.Count <= 0) return;

            foreach (var delayedAction in delayedActions)
            {
                switch (delayedAction.type)
                {
                    case DelayedNetworkActionType.Register:
                        break;
                    case DelayedNetworkActionType.Deregister:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            delayedActions.Clear();
        }
    }

    public class NetworkSystemMas
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

        public NetworkSystem(Map map, NetworkDef networkDef)
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
            var network = RegenerateNetwork(component, out var oldNets);
            foreach (var oldNet in oldNets)
            {
                DeregisterNetwork(oldNet);
            }
            RegisterNetwork(network);

            //Controller is set after net is regenerated
            if (component.IsMainController)
                MainNetworkComponent = component;
        }

        public void DeregisterComponent(INetworkComponent component)
        {
            DeregisterNetworkPart(component);
            TotalComponentSet.RemoveComponent(component);

            DeregisterNetwork(component.Network);

            //from.Network = null;
            Network newNet = null;
            foreach (var root in component.AdjacencySet.FullSet)
            {
                if (root.Network != newNet)
                {
                    newNet = RegenerateNetwork(root, out _);
                    RegisterNetwork(newNet);
                }
            }
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

        //Todo: Fix regen to queue like power net
        public Network RegenerateNetwork(INetworkComponent root, out HashSet<Network> oldNets)
        {


            oldNets = new HashSet<Network>();
            Network newNet = new Network(root.NetworkDef, map, this);
            HashSet<INetworkComponent> closedSet = new HashSet<INetworkComponent>();
            HashSet<INetworkComponent> openSet = new HashSet<INetworkComponent>() { root };
            HashSet<INetworkComponent> currentSet = new HashSet<INetworkComponent>();
            while (openSet.Count > 0)
            {
                foreach (INetworkComponent component in openSet)
                {
                    if (component.Network != null)
                    {
                        oldNets.Add(component.Network);
                    }
                    component.Network = newNet;
                    newNet.AddComponent(component);
                    closedSet.Add(component);
                }

                //
                (currentSet, openSet) = (openSet, currentSet);

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
            tnw.ID = MasterID += 1;
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
                NetworkCells[component.Network].RemoveAll((Predicate<IntVec3>) component.Parent.InnerConnectionCells.Contains);
            }
        }
    }
}
