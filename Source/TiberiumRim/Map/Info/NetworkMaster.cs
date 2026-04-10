using System.Collections.Generic;
using RimWorld;
using TR.Networks;
using UnityEngine;
using Verse;

namespace TR.Info;

public class NetworkMaster
{
    //Debug
    private static bool ShouldShowNetwork;
    private readonly Map map;

    public NetworkStructureSet MainStructureSet;
    public int MasterID = -1;
    public Dictionary<Network, List<IntVec3>> NetworkCells = new();
    public bool[] networkGrid;
    public Network[] NetworkGrid;
    public List<Network> Networks = new();

    public NetworkMaster(Map map, NetworkType network)
    {
        this.map = map;
        networkGrid = new bool[map.cellIndices.NumGridCells];
    }

    public INetworkStructure MainNetworkStructure { get; set; }

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
        foreach (var network in Networks) network.Tick();
    }

    public void DrawNetwork()
    {
        if (!ShouldShowNetwork) return;
        foreach (var network in Networks) network.Draw();
        for (var i = 0; i < networkGrid.Length; i++)
        {
            var cell = networkGrid[i];
            if (cell) CellRenderer.RenderCell(map.cellIndices.IndexToCell(i), 0.75f);
        }
    }

    public Network NetworkAt(IntVec3 c)
    {
        return NetworkGrid[map.cellIndices.CellToIndex(c)];
    }


    public bool NetworkStructureAt(IntVec3 c)
    {
        return networkGrid[map.cellIndices.CellToIndex(c)];
    }

    public Network RegenerateNetwork(INetworkStructure root)
    {
        Network newNet = null;
        var closedSet = new HashSet<INetworkStructure>();
        var openSet = new HashSet<INetworkStructure> { root };
        var currentSet = new HashSet<INetworkStructure>();
        while (openSet.Count > 0)
        {
            foreach (var structure in openSet)
            {
                structure.Network = newNet;
                newNet.AddStructure(structure);
                closedSet.Add(structure);
            }

            (currentSet, openSet) = (openSet, currentSet);
            openSet.Clear();
            foreach (var structure in currentSet)
            foreach (var c in structure.ConnectionCells)
            {
                var thingList = c.GetThingList(map);
                foreach (var thing in thingList)
                {
                    if (!Fits(thing, out var newStructure)) continue;
                    if (newStructure.NetworkType == root.NetworkType && !closedSet.Contains(newStructure) &&
                        newStructure.ConnectsTo(structure))
                    {
                        map.mapDrawer.MapMeshDirty(c, MapMeshFlagDefOf.Buildings);
                        structure.StructureSet.AddNewStructure(newStructure);
                        newStructure.StructureSet.AddNewStructure(structure);
                        openSet.Add(newStructure);
                        break;
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
        for (var i = 0; i < NetworkCells[tnw].Count; i++)
            NetworkGrid[map.cellIndices.CellToIndex(NetworkCells[tnw][i])] = tnw;
    }

    public void DeregisterNetwork(Network tnw)
    {
        if (!Networks.Contains(tnw)) return;
        for (var i = 0; i < NetworkCells[tnw].Count; i++)
            NetworkGrid[map.cellIndices.CellToIndex(NetworkCells[tnw][i])] = null;
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