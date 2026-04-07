using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using TeleCore.FlowCore.IO;
using TeleCore.Loader;
using Verse;

namespace TeleCore.FlowCore;

public class CompNetwork : FXThingComp, INetworkStructure
{
    //Debug
    protected static bool DebugConnectionCells;

    #region Fields

    private PipeNetworkMapInfo _mapInfo;
    private List<NetworkPart> _allNetParts;
    private Dictionary<NetworkDef, INetworkPart> _netPartByDef;
    //private IFXLayerProvider? _fxProvider;
    private Gizmo_NetworkOverview networkInfoGizmo;

    #endregion

    //
    public INetworkPart this[NetworkDef def] => _netPartByDef.TryGetValue(def, out var value) ? value : null;

    //
    public CompProperties_Network Props => (CompProperties_Network)props;
    public CompPowerTrader CompPower { get; private set; }
    public CompFlickable CompFlick { get; private set; }
    public CompFX CompFX { get; private set; }
    public NetworkIO GeneralIO { get; private set; }

    //
    protected virtual bool IsWorkingOverride => true;
    public Gizmo_NetworkOverview NetworkGizmo => networkInfoGizmo ??= new Gizmo_NetworkOverview(this);

    //
    public Thing Thing => parent;
    public List<NetworkPart> NetworkParts => _allNetParts;
    public NetworkPart SelectedPart => NetworkGizmo.SelectedPart;

    public bool IsPowered => CompPower?.PowerOn ?? true;
    public bool IsWorking => IsWorkingOverride;

    //
    public virtual bool RoleIsActive(NetworkRole role)
    {
        return true;
    }

    public virtual bool CanInteractWith(INetworkPart otherPart)
    {
        return true;
    }

    public virtual void NetworkPostTick(INetworkPart netPart, bool isPowered)
    {
    }

    public virtual void Notify_ReceivedValue()
    {
    }

    //
    public void Notify_StructureAdded(INetworkStructure other)
    {
        //structureSet.AddNewStructure(other);
    }

    public void Notify_StructureRemoved(INetworkStructure other)
    {
        //structureSet.RemoveStructure(other);
    }

    //
    public virtual bool AcceptsValue(NetworkValueDef value)
    {
        return true;
    }

    public virtual bool CanConnectToOther(INetworkStructure other)
    {
        return true;
    }

    //SaveData
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref _allNetParts, "networkParts", LookMode.Deep, this);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (_allNetParts.NullOrEmpty())
            {
                TLog.Warning($"Could not load network parts for {parent}... Correcting.");
            }
            else
            {
                for (var i = 0; i < _allNetParts.Count; i++)
                {
                    var netPart = _allNetParts[i];
                    netPart.PostLoadInit(Props.networks[i]);
                }
            }
        }
    }

    //Init Construction
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        //Init Fields
        _netPartByDef = new Dictionary<NetworkDef, INetworkPart>(Props.networks.Count);

        //Get Comps and other Infos
        _mapInfo = parent.Map.TeleCore().NetworkInfo;
        CompPower = parent.TryGetComp<CompPowerTrader>();
        CompFlick = parent.TryGetComp<CompFlickable>();
        CompFX = parent.TryGetComp<CompFX>();

        //Generate Instanced Data
        GeneralIO = new NetworkIO(Props.generalIOConfig, parent.Position, parent.Rotation);

        //Create NetworkParts
        if (respawningAfterLoad && _allNetParts.Count != Props.networks.Count)
        {
            TLog.Warning($"Spawning {parent} after load with missing parts... Correcting.");
        }

        //
        if (!respawningAfterLoad)
        {
            _allNetParts = new List<NetworkPart>(Math.Max(1, Props.networks.Count));
        }

        for (var i = 0; i < Props.networks.Count; i++)
        {
            var partConfig = Props.networks[i];
            NetworkPart? part = null;

            //Create part if it doesnt exist
            var exists = _allNetParts.Exists(p => p is { Config: not null } && p.Config.networkDef == partConfig.networkDef);
            if (!exists)
            {
                part = (NetworkPart)Activator.CreateInstance(partConfig.workerType, this, partConfig);
                _allNetParts.Add(part);
            }

            part ??= _allNetParts[i];
            _netPartByDef.Add(partConfig.networkDef, part);
            part.PartSetup(respawningAfterLoad);
        }

        //Ensure that new nearby junctions add themselves to the network
        foreach (var part in _allNetParts)
        {
            part.CheckNeighborJunctions();
        }

        _mapInfo.Notify_NewNetworkStructureSpawned(this);
    }

    //Deconstruction
    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        _mapInfo.Notify_NetworkStructureDespawned(this);

        foreach (var networkPart in NetworkParts)
        {
            networkPart.PostDestroy(mode, previousMap);
        }
    }

    //
    public bool HasPartFor(NetworkDef networkDef)
    {
        return _netPartByDef.ContainsKey(networkDef);
    }

    //UI
    public override void PostDraw()
    {
        base.PostDraw();
        foreach (var networkPart in NetworkParts) networkPart.Draw();
        //TODO: legacy debug data
        // if (DebugConnectionCells && Find.Selector.IsSelected(parent))
        // {
        //     GenDraw.DrawFieldEdges(networkPart.CellIO.OuterConnnectionCells.Select(t => t.IntVec).ToList(), Color.cyan);
        //     GenDraw.DrawFieldEdges(networkPart.CellIO.InnerConnnectionCells.ToList(), Color.green);
        // }
    }

    public override void PostPrintOnto(SectionLayer layer)
    {
        base.PostPrintOnto(layer);
        foreach (var networkPart in NetworkParts)
        {
            networkPart.Print(layer);
        }
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder();
        foreach (var networkSubPart in NetworkParts) sb.AppendLine(networkSubPart.InspectString());

        /*TODO: ADD THIS TO COMPONENT DESC
        if (!Network.IsWorking)
            sb.AppendLine("TR_MissingNetworkController".Translate());
        //TODO: Make reasons for multi roles
        if (!Network.ValidFor(Props.NetworkRole, out string reason))
        {
            sb.AppendLine("TR_MissingConnection".Translate() + ":");
            if (!reason.NullOrEmpty())
            {
                sb.AppendLine("   - " + reason.Translate());
            }
        }
        */

        return sb.ToString().TrimStart().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        /*
        foreach (var gizmo in networkParts.Select(c => c.SpecialNetworkDescription))
        {
        yield return gizmo;
        }
        */

        yield return NetworkGizmo;

        foreach (var networkPart in NetworkParts)
        {
            foreach (var partGizmo in networkPart.GetPartGizmos())
                yield return partGizmo;
        }

        foreach (var g in base.CompGetGizmosExtra())
            yield return g;

        if (!DebugSettings.godMode) yield break;

        //TODO: Legacy debug command
        // yield return new Command_Action()
        // {
        //     defaultLabel = "Draw Networks",
        //     action = delegate
        //     {
        //         foreach (var networkPart in NetworkParts)
        //         {
        //             _mapInfo[networkPart.Config.networkDef].DEBUG_ToggleShowNetworks();
        //         }
        //     }
        // };

        yield return new Command_Action
        {
            defaultLabel = "Draw Connections",
            action = delegate { DebugConnectionCells = !DebugConnectionCells; }
        };
    }

    #region FX Implementation

    // ## Layers ##
    // 0 - Container
    // 
    public override bool FX_ProvidesForLayer(FXArgs args)
    {
        if (args.categoryTag == "FXNetwork")
            return true;
        return false;
    }

    public override CompPowerTrader FX_PowerProviderFor(FXArgs args)
    {
        return null;
    }

    //TODO: Only add this where necessary, shouldnt be base behaviour
    // public override bool? FX_ShouldDraw(FXLayerArgs args)
    // {
    //     return args.index switch
    //     {
    //         1 => _allNetParts.Any(t => t?.HasConnection ?? false),
    //         _ => true
    //     };
    // }

    public override float? FX_GetOpacity(FXLayerArgs args)
    {
        return 1f;
    }

    public override float? FX_GetRotation(FXLayerArgs args)
    {
        return null;
    }

    public override float? FX_GetRotationSpeedOverride(FXLayerArgs args)
    {
        return null;
    }

    public override float? FX_GetAnimationSpeedFactor(FXLayerArgs args)
    {
        return null;
    }

    public override Color? FX_GetColor(FXLayerArgs args)
    {
        return args.index switch
        {
            //TODO: Access color from flowsystem
            //0 => _allNetParts[0].Container.Color,
            _ => Color.white
        };
    }

    public override Vector3? FX_GetDrawPosition(FXLayerArgs args)
    {
        return parent.DrawPos;
    }

    public override Func<RoutedDrawArgs, bool> FX_GetDrawFunc(FXLayerArgs args)
    {
        return null!;
    }

    public override bool? FX_ShouldThrowEffects(FXEffecterArgs args)
    {
        return base.FX_ShouldThrowEffects(args);
    }

    public override void FX_OnEffectSpawned(FXEffecterSpawnedEventArgs spawnedEventArgs)
    {
    }

    #endregion
}