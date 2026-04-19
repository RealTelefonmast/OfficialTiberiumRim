using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class MapComponent_TNWManager : MapComponent
{
    //Debug
    public static bool ShowNetworks = true;

    [TweakValue("MapComponent_TNW", 0f, 100f)]
    public static bool DrawBool = false;

    public List<TR.Harvester> AllHarvesters = new();
    public StructureSet MainStructureSet = new();
    public int MasterID = -1;
    public Dictionary<TiberiumNetwork, List<IntVec3>> networkCells = new();

    public List<TiberiumNetwork> Networks = new();
    public HarvesterReservationManager ReservationManager;

    public bool[] tnwGrid;

    public MapComponent_TNWManager(Map map) : base(map)
    {
        ReservationManager = new HarvesterReservationManager(map);
        tnwGrid = new bool[map.cellIndices.NumGridCells];
    }

    public CompTNW_TNC NetworkController { get; set; }

    public List<TNW_Refinery> AllRefineries
    {
        get { return Networks.SelectMany(n => n.NetworkSet.Refineries) as List<TNW_Refinery>; }
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        var i = 0;

        if (DrawBool || TiberiumRimSettings.settings.ShowNetworkValues) 
            DrawTNWNetGrid();

        if (ShowNetworks)
        {
            foreach (var network in Networks)
            {
                GenDraw.DrawFieldEdges(network.NetworkSet.FullList.SelectMany(b => b.OccupiedRect().Cells).ToList(),
                    ColorByNum(i));
                i++;
            }
        }
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        foreach (var network in Networks) network.Tick();
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

    public TiberiumNetwork MakeNewNetwork(CompTNW root, TiberiumNetwork forNetwork = null)
    {
        var newNet = forNetwork ?? new TiberiumNetwork(this);
        HashSet<CompTNW> closedSet = new();
        HashSet<CompTNW> openSet = new() { root };
        HashSet<CompTNW> currentSet = new();
        while (openSet.Count > 0)
        {
            foreach (CompTNW item in openSet)
            {
                item.Network = newNet;
                newNet.AddStructure(item);
                closedSet.Add(item);
            }

            HashSet<CompTNW> hashSet = currentSet;
            currentSet = openSet;
            openSet = hashSet;
            openSet.Clear();
            foreach (CompTNW tnwb in currentSet)
            foreach (IntVec3 c in tnwb.CardinalConnectionCells)
            {
                var thingList = c.GetThingList(tnwb.parent.Map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    var newTnwb = thingList[i].TryGetComp<CompTNW>();
                    if (newTnwb != null && !closedSet.Contains(newTnwb) && newTnwb.ConnectsTo(tnwb))
                    {
                        map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Buildings);
                        map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Things);
                        tnwb.StructureSet.AddNewStructure(newTnwb, c);
                        newTnwb.StructureSet.AddNewStructure(tnwb, c + IntVec3.North);
                        openSet.Add(newTnwb);
                        break;
                    }
                }
            }
        }

        return newNet;
    }

    public bool ConnectionAt(IntVec3 c)
    {
        return tnwGrid[map.cellIndices.CellToIndex(c)];
    }

    public void DrawTNWNetGrid()
    {
        for (var i = 0; i < tnwGrid.Length; i++)
        {
            var cell = tnwGrid[i];
            if (cell)
                CellRenderer.RenderCell(map.cellIndices.IndexToCell(i), 0.75f);
        }
        /*
        Rand.PushState();
        foreach(TiberiumNetwork net in Networks)
        {
            var cells = networkCells[net];
            foreach(IntVec3 c in cells)
            {
                Rand.Seed = net.GetHashCode();
                CellRenderer.RenderCell(c, Rand.Value);
            }
        }
        Rand.PopState();
        */
    }

    private TiberiumNetwork NetworkAt(IntVec3 cell)
    {
        return Networks.Find(n => n.NetworkCells().Contains(cell));
    }

    public void RegisterNetwork(TiberiumNetwork tnw)
    {
        tnw.NetworkID = MasterID += 1;
        Networks.Add(tnw);
        networkCells.Add(tnw, tnw.NetworkCells());
        for (var i = 0; i < networkCells[tnw].Count; i++)
            tnwGrid[map.cellIndices.CellToIndex(networkCells[tnw][i])] = true;
    }

    public void DeregisterNetwork(TiberiumNetwork tnw)
    {
        if (Networks.Contains(tnw))
        {
            for (var i = 0; i < networkCells[tnw].Count; i++)
                tnwGrid[map.cellIndices.CellToIndex(networkCells[tnw][i])] = false;
            Networks.Remove(tnw);
            networkCells.Remove(tnw);
        }
    }

    public void RegisterHarvester(TR.Harvester harvester)
    {
        ReservationManager.RegisterHarvester(harvester);
    }

    public void DeregisterHarvester(TR.Harvester harvester)
    {
        ReservationManager.DeregisterHarvester(harvester);
        MainStructureSet.Refineries.ForEach(r => r.RemoveHarvester(harvester));
    }
}