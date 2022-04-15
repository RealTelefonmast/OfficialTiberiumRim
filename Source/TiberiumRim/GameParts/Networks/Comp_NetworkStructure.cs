using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_NetworkStructure : ThingComp, IFXObject, INetworkStructure
    {
        //Comp References
        private CompPowerTrader powerComp;
        private CompFlickable flickComp;
        private CompFX fxComp;
        private MapComponent_Tiberium TiberiumMapComp;

        //Fields
        private NetworkMapInfo NetworkInfo;
        private IntVec3[][] innerConnectionCellsByRot;
        private IntVec3[][] connectionCellsByRot;

        private List<NetworkComponent> networkParts = new();
        private Dictionary<NetworkDef, NetworkComponent> networkComponentByDef = new();

        //Debug
        protected static bool DebugConnectionCells = false;

        public NetworkComponent this[NetworkDef def] => networkComponentByDef[def];

        public Thing Thing => parent;

        //CompStuff
        public CompProperties_NetworkStructure Props => (CompProperties_NetworkStructure)base.props;

        public CompPowerTrader CompPower => powerComp;
        public CompFlickable CompFlick => flickComp;
        public CompFX CompFX => fxComp;

        public bool IsPowered => CompPower?.PowerOn ?? true;

        public List<NetworkComponent> NetworkParts => networkParts;

        //
        //public NetworkStructureSet NeighbourStructureSet { get => structureSet; protected set => structureSet = value; }

        //FX Data
        
        public ExtendedGraphicData ExtraData => (parent as IFXObject)?.ExtraData ?? new ExtendedGraphicData();
        public virtual Vector3[] DrawPositions => new Vector3[] { parent.DrawPos, parent.DrawPos, parent.DrawPos };
        public virtual Color[] ColorOverrides => new Color[] { NetworkParts[0].Container.Color, Color.white, Color.white };
        public virtual float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public virtual float?[] RotationOverrides => new float?[] { null, null, null };
        public virtual float?[] AnimationSpeeds => null;

        // !networkParts.NullOrEmpty() && networkParts.Any(t => t?.HasConnection ?? false)
        public virtual bool[] DrawBools
        {
            get
            {
                return new bool[] {true, false, true};
            }
        }

        public virtual Action<FXGraphic>[] Actions => null;
        public virtual Vector2? TextureOffset => null;
        public virtual Vector2? TextureScale => null;
        public virtual bool ShouldDoEffecters => true;
        public virtual CompPower ForcedPowerComp => null;

        public IntVec3[] InnerConnectionCells
        {
            get
            {
                return innerConnectionCellsByRot[parent.Rotation.AsInt] ??= Props.InnerConnectionCells(parent);
            }
        }

        public IntVec3[] ConnectionCells
        {
            get
            {
                if (connectionCellsByRot[parent.Rotation.AsInt] == null)
                {
                    var cellsOuter = new List<IntVec3>();
                    foreach (var edgeCell in parent.OccupiedRect().ExpandedBy(1).EdgeCells)
                    {
                        foreach (var inner in InnerConnectionCells)
                        {
                            if (edgeCell.AdjacentToCardinal(inner))
                            {
                                cellsOuter.Add(edgeCell);
                            }
                        }
                    }
                    connectionCellsByRot[parent.Rotation.AsInt] = cellsOuter.ToArray();
                }

                return connectionCellsByRot[parent.Rotation.AsInt];
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref networkParts, "networkParts", LookMode.Deep, this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (networkParts.NullOrEmpty())
                {
                    TLog.Warning($"Could not load network parts for {parent}... Correcting.");
                    return;
                }
                foreach (var newComponent in networkParts)
                {
                    networkComponentByDef.Add(newComponent.NetworkDef, newComponent);
                    newComponent.ComponentSetup(true);
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            innerConnectionCellsByRot = new IntVec3[4][];
            connectionCellsByRot = new IntVec3[4][];

            //Get cached data
            powerComp = parent.TryGetComp<CompPowerTrader>();
            flickComp = parent.TryGetComp<CompFlickable>();
            fxComp = parent.TryGetComp<CompFX>();

            TiberiumMapComp = parent.Map.Tiberium();
            NetworkInfo = TiberiumMapComp.NetworkInfo;

            //Create NetworkComponents
            if (!respawningAfterLoad || networkParts.NullOrEmpty())
            {
                if (respawningAfterLoad && networkParts.NullOrEmpty())
                {
                    TLog.Warning($"Spawning {parent} after load with null parts... Correcting.");
                }
                for (var i = 0; i < Props.networks.Count; i++)
                {
                    var compProps = Props.networks[i];
                    var newComponent = new NetworkComponent(this, compProps, i);
                    networkParts.Add(newComponent);
                    networkComponentByDef.Add(compProps.networkDef, newComponent);
                    newComponent.ComponentSetup(respawningAfterLoad);
                }
            }

            //Check for neighbor intersections


            //Regen network after all data is set
            NetworkInfo.Notify_NewNetworkStructureSpawned(this);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            //Regen network after all data is set
            NetworkInfo.Notify_NetworkStructureDespawned(this);

            foreach (var networkPart in NetworkParts)
            {
                networkPart.PostDestroy(mode, previousMap);
            }

            base.PostDestroy(mode, previousMap);
        }

        public override void CompTick()
        {
            base.CompTick();
            var isPowered = IsPowered;
            foreach (var networkPart in networkParts)
            {
                networkPart.NetworkCompTick(isPowered);
                NetworkCompProcessor(networkPart, isPowered);
            }
            NetworkTickCustom(isPowered);
        }

        public override void ReceiveCompSignal(string signal)
        {
            TLog.Message($"Receiving signal: '{signal}'");
            base.ReceiveCompSignal(signal);
        }

        protected virtual void NetworkTickCustom(bool isPowered)
        {

        }

        protected virtual void NetworkCompProcessor(NetworkComponent netComp, bool isPowered)
        {
        }

        public virtual bool AcceptsValue(NetworkValueDef value)
        {
            return true;
        }

        //Data Notifiers
        public void Notify_StructureAdded(INetworkStructure other)
        {

            //structureSet.AddNewStructure(other);
        }

        public void Notify_StructureRemoved(INetworkStructure other)
        {

            //structureSet.RemoveStructure(other);
        }

        public bool ConnectsTo(INetworkStructure other)
        {
            return ConnectionCells.Any(other.InnerConnectionCells.Contains);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (DebugConnectionCells && Find.Selector.IsSelected(parent))
            {
                GenDraw.DrawFieldEdges(ConnectionCells.ToList(), Color.cyan);
                GenDraw.DrawFieldEdges(InnerConnectionCells.ToList(), Color.green);
            }

            foreach (var networkPart in NetworkParts)
            {
                networkPart.Draw();
            }
        }

        public void PrintForGrid(SectionLayer layer)
        {
            foreach (var networkPart in NetworkParts)
            {
                networkPart.NetworkDef.OverlayGraphic?.Print(layer, Thing, 0);
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            foreach (var networkPart in NetworkParts)
            {
                networkPart.NetworkDef.TransmitterGraphic?.Print(layer, Thing, 0);
            }
            base.PostPrintOnto(layer);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
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
            sb.AppendLine($"Current Networks: {networkParts.Count}");
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

            foreach (var networkPart in networkParts)
            {
                foreach (var partGizmo in networkPart.GetPartGizmos())
                {
                    yield return partGizmo;
                }
            }

            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (!DebugSettings.godMode) yield break;

            yield return new Command_Action()
            {
                defaultLabel = "Draw Networks",
                action = delegate
                {
                    foreach (var networkPart in networkParts)
                    {
                        NetworkInfo[networkPart.NetworkDef].ToggleShowNetworks();
                    }
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Draw Connections",
                action = delegate
                {
                    DebugConnectionCells = !DebugConnectionCells;
                }
            };
        }
    }

    public class CompProperties_NetworkStructure : CompProperties
    {
        public List<NetworkComponentProperties> networks;
        public string connectionPattern;

        public CompProperties_NetworkStructure()
        {
            this.compClass = typeof(Comp_NetworkStructure);
        }

        public IntVec3[] InnerConnectionCells(Thing parent)
        {
            if (connectionPattern == null) return parent.OccupiedRect().ToArray();

            var pattern = PatternByRot(parent.Rotation, parent.def.size);
            var rect = parent.OccupiedRect();
            var rectList = rect.ToArray();
            var cellsInner = new List<IntVec3>();

            int width = parent.RotatedSize.x;
            int height = parent.RotatedSize.z;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int actualIndex = y * width + x;
                    int inv = ((height - 1) - y) * width + x;

                    var c = pattern[inv];
                    if (c == '+') 
                        cellsInner.Add(rectList[actualIndex]);
                }
            }
            return cellsInner.ToArray();
        }

        private string PatternByRot(Rot4 rotation, IntVec2 size)
        {
            var patternArray = String.Concat(connectionPattern.Split('|')).ToCharArray();

            int xWidth = size.x;
            int yHeight = size.z;

            if (rotation == Rot4.East)
            {
                return new string(Rotate(patternArray, xWidth, yHeight, 0));
            }
            if (rotation == Rot4.South)
            {
                return new string(Rotate(patternArray, xWidth, yHeight, 1));
            }
            if (rotation == Rot4.West)
            {
                return new string(Rotate(patternArray, xWidth, yHeight, 2));
            }

            return new string(patternArray);
        }

        private char[] Rotate(char[] arr, int width, int height, int rotationInt = 0)
        {
            char[] newArray = new char[arr.Length];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int indexToRotate = y * width + x;
                    int transposed = (x * height) + ((height - 1) - y);

                    newArray[transposed] = arr[indexToRotate];
                }
            }

            if (rotationInt > 0)
                return Rotate(newArray, height, width, --rotationInt);
            return newArray;
        }
    }
}
