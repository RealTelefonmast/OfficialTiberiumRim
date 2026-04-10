using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class MapComponent_TNWManager : MapComponent
    {
        public List<TiberiumNetwork> Networks = new List<TiberiumNetwork>();
        public Dictionary<TiberiumNetwork, List<IntVec3>> networkCells = new Dictionary<TiberiumNetwork, List<IntVec3>>();
        public StructureSet MainStructureSet = new StructureSet();
        public List<Harvester> AllHarvesters = new List<Harvester>();
        public HarvesterReservationManager ReservationManager;
        public int MasterID = -1;

        public bool[] tnwGrid;

        private CompTNW_TNC networkController;

        //Debug
        public static bool ShowNetworks = true;

        public MapComponent_TNWManager(Map map) : base(map)
        {
            ReservationManager = new HarvesterReservationManager(map);
            tnwGrid = new bool[map.cellIndices.NumGridCells];
        }

        [TweakValue("MapComponent_TNW", 0f, 100f)]
        public static bool DrawBool = false;

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            int i = 0;

            if (DrawBool || TiberiumRimSettings.settings.ShowNetworkValues)
            {
                DrawTNWNetGrid();
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            foreach(TiberiumNetwork network in Networks)
            {
                network.Tick();
            }
        }

        public CompTNW_TNC NetworkController { get => networkController; set => networkController = value; }

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
            TiberiumNetwork newNet = forNetwork ?? new TiberiumNetwork(this);
            HashSet<CompTNW> closedSet = new HashSet<CompTNW>();
            HashSet<CompTNW> openSet = new HashSet<CompTNW>() { root };
            HashSet<CompTNW> currentSet = new HashSet<CompTNW>();
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
                {
                    foreach (IntVec3 c in tnwb.CardinalConnectionCells)
                    {
                        List<Thing> thingList = c.GetThingList(tnwb.parent.Map);
                        for (int i = 0; i < thingList.Count; i++)
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
            for (int i = 0; i < networkCells[tnw].Count; i++)
            {
                tnwGrid[map.cellIndices.CellToIndex(networkCells[tnw][i])] = true;
            }
        }

        public void DeregisterNetwork(TiberiumNetwork tnw)
        {
            if (Networks.Contains(tnw))
            {
                for (int i = 0; i < networkCells[tnw].Count; i++)
                {
                    tnwGrid[map.cellIndices.CellToIndex(networkCells[tnw][i])] = false;
                }
                Networks.Remove(tnw);
                networkCells.Remove(tnw);
            }
        }

        public void RegisterHarvester(Harvester harvester)
        {
            ReservationManager.RegisterHarvester(harvester);
        }

        public void DeregisterHarvester(Harvester harvester)
        {
            ReservationManager.DeregisterHarvester(harvester);
            MainStructureSet.Refineries.ForEach(r => r.RemoveHarvester(harvester));
        }
    }
}
