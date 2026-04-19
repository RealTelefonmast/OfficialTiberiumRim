using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Enumerable = System.Linq.Enumerable;

namespace TiberiumRim;

public class TiberiumNetworkBuilding : FXBuilding
{
    private readonly List<IntVec3> cardinalCells = new();
    private readonly List<IntVec3> inputCells = new();
    private readonly List<IntVec3> outputCells = new();
    public new NetworkBuildingDef def;

    //Debug
    private bool DrawConnection;
    private Graphic flowArrow;
    private int minToTransfer;

    private TiberiumNetwork network;
    public NetworkMode predefinedMode = NetworkMode.Alpha;
    public StructureSet StructureSet;

    public TiberiumNetwork Network
    {
        get => network;
        set
        {
            if (network != value) Manager.DeregisterNetwork(network);
            network = value;
        }
    }

    public TiberiumContainer Container => CompTNW.Container;

    public CompTNW CompTNW => this.TryGetComp<CompTNW>();

    public MapComponent_TNWManager Manager => Map.GetComponent<MapComponent_TNWManager>();

    public List<TNW_Pipe> AdjacentPipes => StructureSet.Pipes;

    public List<TNW_Pipe> ConnectedPipes => StructureSet.Pipes;

    public virtual List<IntVec3> InputCells => inputCells;

    public virtual List<IntVec3> OutputCells => outputCells;


    public virtual IEnumerable<IntVec3> ConnectableCells => this.OccupiedRect().Cells;

    public virtual IEnumerable<IntVec3> CardinalConnectableCells
    {
        get
        {
            if (def.IOMode == TNWBIOMode.Static) return Enumerable.Concat(inputCells, OutputCells);
            return cardinalCells;
        }
    }

    public override Vector3[] DrawPositions => new[] { DrawPos, DrawPos, DrawPos };

    public override float[] OpacityFloats
    {
        get
        {
            var floats = new float[3] { 0f, 1f, 1f };
            floats[0] = CompTNW.Container.StoredPct;
            return floats;
        }
    }

    public override bool[] DrawBools
    {
        get { return new[] { true, DrawConnection || ConnectedPipes.Any(p => !p.DestroyedOrNull()), true }; }
    }

    public override Color[] ColorOverrides
    {
        get { return new[] { CompTNW.Container.Color, Color.white, Color.white }; }
    }

    public Graphic FlowArrow
    {
        get
        {
            if (flowArrow == null)
                flowArrow = GraphicDatabase.Get<Graphic_Single>("Buildings/Common/TiberiumNetwork/InputDirection");
            return flowArrow;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        def = base.def as NetworkBuildingDef;
        SetConnectionSpots();
        UpdateConnections();
        if (def.IOMode == TNWBIOMode.Static)
        {
            foreach (var c in def.InputCells) InputCells.Add(Position + c);
            foreach (var c in def.OutputCells) OutputCells.Add(Position + c);
        }
    }

    public void UpdateConnections()
    {
        StructureSet = new StructureSet(this);
        TNW_TNC parent = null;
        var networks = new List<TiberiumNetwork>();
        if (Network != null) parent = Network.Parent;
        if (!StructureSet.FullList.NullOrEmpty())
            networks = Enumerable.ToList(Enumerable.Select(StructureSet.FullList, b => b.Network));
        Network = new TiberiumNetwork(this, Manager, predefinedMode);
    }

    private void SetConnectionSpots()
    {
        foreach (var c in GenAdj.CellsAdjacentCardinal(this))
            if (Enumerable.Any(ConnectableCells, x => x.AdjacentToCardinal(c)))
                cardinalCells.Add(c);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        StructureSet.RemovingSet(this);
        Manager.DeregisterNetwork(Network);
        if (Container.GetTotalStorage > 0f && mode != DestroyMode.Vanish)
        {
            if ((mode == DestroyMode.Deconstruct || mode == DestroyMode.Refund) && CompTNW.Props.dropsContents)
            {
                var container = ThingMaker.MakeThing(TiberiumDefOf.PortableContainer) as PortableContainer;
                container.PostSetup(Container);
                GenSpawn.Spawn(container, Position, Map);
            }
            else
            {
                var i = 0;
                var crystals = Container.PotentialCrystals();
                Predicate<IntVec3> pred = c => c.InBounds(Map) && c.GetEdifice(Map) == null;
                var action = delegate(IntVec3 c)
                {
                    var crystal = Enumerable.ElementAtOrDefault(crystals, i);
                    if (crystal != null)
                    {
                        GenSpawn.Spawn(crystal, c, Map);
                        crystals.Remove(crystal);
                    }

                    i++;
                };
                var flood = new TiberiumFloodInfo(Map, crystals.Count, pred, action);
                flood.TryMakeFlood(out var floodedCells, this.OccupiedRect());
            }
        }

        base.Destroy(mode);
        Network.UpdateTiberiumNetwork(this);
    }

    public override void Tick()
    {
        base.Tick();
        if (CompTNW.compPower?.PowerOn ?? (true && Container.StoredPct > 0))
            foreach (var structure in StructureSet.FullList)
            {
                //CondLog("" + (Container.StoredPct <= 0.5f) + " " + (structure.Container.StoredPct >= 0.5f) + " " + (structure.Container.StoredPct == Container.StoredPct));
                if (structure is TNW_Pipe) return;
                if (Container.StoredPct <= 0.5f || structure.Container.StoredPct >= 0.5f ||
                    structure.Container.StoredPct == Container.StoredPct)
                    if (structure.Container.StoredPct >= 0.5f)
                        return;

                if (Container.GetTotalStorage > 0 && !structure.Container.CapacityFull)
                    foreach (TiberiumType type in Container.AllStoredTypes)
                        if (Container.TryTransferTo(structure.Container, type, 1))
                        {
                        }
            }
    }

    public virtual void ManagePipes()
    {
    }

    public bool CompatibleWith(TiberiumNetworkBuilding other)
    {
        if (other.Container.CanConnectTo(Container) && other.Network.NetworkMode == Network.NetworkMode) return true;
        return false;
    }

    public bool CanConnectTo(IntVec3 c, TiberiumNetworkBuilding other)
    {
        if ((ConnectsToCell(c) || ConnectsToTNWB(other)) && CompatibleWith(other)) return true;
        return false;
    }

    public virtual bool ConnectsToCell(IntVec3 c)
    {
        return Enumerable.Contains(CardinalConnectableCells, c);
    }

    public virtual bool ConnectsToTNWB(TiberiumNetworkBuilding other)
    {
        return Enumerable.Any(CardinalConnectableCells, other.ConnectableCells.Contains);
    }

    public override void Draw()
    {
        base.Draw();
        //GenDraw.DrawFieldEdges(CardinalConnectableCells.ToList(), Color.magenta);
        //GenDraw.DrawFieldEdges(ConnectableCells.ToList(), Color.blue);

        if (!(this is TNW_Pipe) &&
            (Find.Selector.IsSelected(this) || ConnectedPipes.Any(p => Find.Selector.IsSelected(p))))
        {
            foreach (var cell in InputCells)
            {
                var rot = Rot4.FromAngleFlat((Position - cell).AngleFlat);
                FlowArrow.Draw(cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), rot, this);
            }

            foreach (var cell in OutputCells)
            {
                var rot = Rot4.FromAngleFlat((Position - cell).AngleFlat);
                FlowArrow.Draw(cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), rot.Opposite, this);
            }
        }
    }

    public void PrintForGrid(SectionLayer layer)
    {
        TiberiumGraphics.TiberiumNetworkPipesOverlay.Print(layer, this);
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.GetInspectString().TrimEndNewlines());
        sb.AppendLine(
            "TR_ContainerContent".Translate() + ": " + Container.GetTotalStorage + "/" + Container.maxCapacity);
        if (DebugSettings.godMode)
        {
            sb.AppendLine("Storage Mode: " + Container.mode);
            sb.AppendLine("NetworkID: " + Network.NetworkID + " || " + "NetworkMode: " + Network.NetworkMode);
            sb.AppendLine("IOMode: " + def.IOMode);
        }

        return sb.ToString().TrimStart().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "Show Network",
                action = delegate { MapComponent_TNWManager.ShowNetworks = !MapComponent_TNWManager.ShowNetworks; }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Connection Overlay",
                action = delegate { DrawConnection = !DrawConnection; }
            };

            yield return new Command_Action
            {
                defaultLabel = "View StructureSet",
                defaultDesc = StructureSet.ToString()
            };

            yield return new Command_Action
            {
                defaultLabel = "Set Network Mode",
                action = delegate
                {
                    var list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("Set Alpha", delegate
                    {
                        Network.NetworkMode = NetworkMode.Alpha;
                        UpdateConnections();
                    }));
                    list.Add(new FloatMenuOption("Set Beta", delegate { Network.NetworkMode = NetworkMode.Beta; }));
                    list.Add(new FloatMenuOption("Set Delta", delegate { Network.NetworkMode = NetworkMode.Delta; }));
                    list.Add(
                        new FloatMenuOption("Set Epsilon", delegate { Network.NetworkMode = NetworkMode.Epsilon; }));
                    list.Add(new FloatMenuOption("Set Gamma", delegate { Network.NetworkMode = NetworkMode.Gamma; }));
                    var menu = new FloatMenu(list);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
            };
        }
    }
}