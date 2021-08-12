using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TiberiumRim.GameParts.MapComps;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public abstract class Comp_NetworkStructure : ThingComp, IFXObject, INetworkStructure, IContainerHolder
    {
        //Comp References
        protected CompPowerTrader powerComp;
        protected CompFlickable flickComp;
        protected CompFX fxComp;
        protected MapComponent_Tiberium TiberiumMapComp;

        //Fields
        protected NetworkMapInfo NetworkInfo;
        protected NetworkContainer container;

        protected NetworkStructureSet structureSet;
        protected Network network;

        protected IntVec3[] connectionCells;

        //Debug
        protected static bool DebugConnectionCells = false;

        //CompStuff
        public CompProperties_NetworkStructure Props => (CompProperties_NetworkStructure)base.props;

        public CompPowerTrader CompPower => powerComp;
        public CompFlickable CompFlick => flickComp;
        public CompFX CompFX => fxComp;

        //Network Data
        public bool IsPowered => parent.IsPoweredOn();
        public bool HasLeak => false;
        public bool HasConnection => StructureSet.Transmitters.Any();

        public NetworkContainer Container
        {
            get => container;
            set => container = value;
        }

        //FX Data
        public ExtendedGraphicData ExtraData => (parent as IFXObject)?.ExtraData ?? new ExtendedGraphicData();
        public virtual Vector3[] DrawPositions => new Vector3[] { parent.DrawPos, parent.DrawPos, parent.DrawPos };
        public virtual Color[] ColorOverrides => new Color[] { Color.white, Color.white, Color.white };
        public virtual float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public virtual float?[] RotationOverrides => new float?[] { null, null, null };
        public virtual bool[] DrawBools => new bool[] { true, HasConnection, true };
        public virtual Action<FXGraphic>[] Actions => null;
        public virtual Vector2? TextureOffset => null;
        public virtual Vector2? TextureScale => null;
        public virtual bool ShouldDoEffecters => true;
        public virtual CompPower ForcedPowerComp => null;

        public Thing Thing => parent;

        //
        public NetworkType NetworkType => Props.networkType;
        public NetworkRole NetworkRole => Props.networkRole;
        public NetworkStructureSet StructureSet { get => structureSet; protected set => structureSet = value; }

        public Network Network
        {
            get => network; 
            set => network = value;
        }

        public IEnumerable<IntVec3> ConnectionCells
        {
            get => connectionCells;
            protected set => connectionCells = value.ToArray();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = parent.TryGetComp<CompPowerTrader>();
            flickComp = parent.TryGetComp<CompFlickable>();
            fxComp = parent.TryGetComp<CompFX>();
            TiberiumMapComp = parent.Map.Tiberium();
            NetworkInfo = TiberiumMapComp.NetworkInfo;

            NetworkInfo.Notify_NewNetworkStructureSpawned(this);

            CreateContainer();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
        }


        //Data Notifiers
        public void Notify_StructureAdded(INetworkStructure other)
        {
            throw new NotImplementedException();
        }

        public void Notify_StructureRemoved(INetworkStructure other)
        {
            throw new NotImplementedException();
        }

        public bool ConnectsTo(INetworkStructure other)
        {
            throw new NotImplementedException();
        }

        protected void CreateContainer()
        {
            Container = (NetworkContainer)Activator.CreateInstance(Props.containerType, this, Props.maxStorage, Props.allowedTypes, Props.valueType);
        }

        //
        public override void CompTick()
        {
            base.CompTick();
        }

        public virtual void Notify_ContainerFull()
        {
        }

        public override void PostDraw()
        {
            base.PostDraw();
        }

        public void PrintForGrid(SectionLayer layer)
        {
            TiberiumContent.OverlayGraphicForNetwork(NetworkType).Print(layer, this.parent, 0);
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            TiberiumContent.ConnectionGraphicForNetwork(NetworkType).Print(layer, Thing, 0);
            base.PostPrintOnto(layer);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (!Network.IsWorking)
                sb.AppendLine("TR_MissingNetworkController".Translate());
            if (!Network.ValidFor(Props.networkRole, out string reason))
            {
                sb.AppendLine("TR_MissingConnection".Translate() + ":");
                if (!reason.NullOrEmpty())
                {
                    sb.AppendLine("   - " + reason.Translate());
                }
            }
            return sb.ToString().TrimStart().TrimEndNewlines();
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            yield return new Designator_BuildFixed(parent.def);
            yield return new Designator_BuildFixed(Props.networkDef.transmitter);

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "View Set",
                    defaultDesc = StructureSet.ToString(),
                    action = delegate { }
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
    }

    public class CompProperties_NetworkStructure : CompProperties
    {
        public NetworkType networkType;
        public NetworkRole networkRole = NetworkRole.Transmitter;

        public NetworkDef networkDef;

        public Type containerType = typeof(NetworkContainer);
        public Type valueType;
        public List<Enum> allowedTypes;

        public bool isMainNetworkStructure = false;

        public int maxStorage = 0;
        public bool dropContents = false;
        public bool storeEvenly = false;

        public CompProperties_NetworkStructure()
        {

        }
    }
}
