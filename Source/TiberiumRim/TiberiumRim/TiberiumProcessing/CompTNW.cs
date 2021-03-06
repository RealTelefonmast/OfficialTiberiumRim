﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public enum TNWMode
    {
        None,
        Storage,
        Consumer,
        Producer
    }

    public class CompTNW : ThingComp, IFXObject, IContainerHolder
    {
        public CompPowerTrader CompPower;
        public CompFlickable CompFlick;
        public CompFX CompFX;
        public MapComponent_TNWManager TNWManager;
        public MapComponent_Tiberium TiberiumManager;

        //
        public StructureSet StructureSet;

        private TiberiumNetwork network;
        private TiberiumContainer container;
        public List<IntVec3> pipeExtensionCells = new List<IntVec3>();
        private List<IntVec3> cardinalCells = new List<IntVec3>();

        public bool ShouldLeak => false;
        public bool HasConnection => StructureSet.Pipes.Any();

        public TNWMode NetworkMode => Props.tnwbMode;
        public CompProperties_TNW Props => (CompProperties_TNW)props;
        public TiberiumContainer Container { get => container; set => container = value; }

        //Debug
        private static bool DebugConnectionCells = false;

        //FX Set
        public ExtendedGraphicData ExtraData => (parent as IFXObject)?.ExtraData ?? new ExtendedGraphicData();
        public virtual Vector3[] DrawPositions => new Vector3[] { parent.DrawPos, parent.DrawPos, parent.DrawPos };
        public virtual Color[] ColorOverrides => new Color[] { Container.Color, Color.white, Color.white };
        public virtual float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public virtual float?[] RotationOverrides => new float?[] { null, null, null };
        public virtual bool[] DrawBools => new bool[] { true, HasConnection, true };
        public virtual Action<FXGraphic>[] Actions => null;
        public virtual Vector2? TextureOffset => null;
        public virtual Vector2? TextureScale => null;
        public virtual bool ShouldDoEffecters => true;
        public virtual CompPower ForcedPowerComp => null;

        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref container, "container", new object[] { this.parent, this });
            base.PostExposeData();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Container = new TiberiumContainer(Props.maxStorage, Props.supportedTypes, this.parent, this);
            }
            CompPower = parent.TryGetComp<CompPowerTrader>();
            CompFlick = parent.TryGetComp<CompFlickable>();
            CompFX = parent.TryGetComp<CompFX>();
            TNWManager = parent.Map.GetComponent<MapComponent_TNWManager>();
            TNWManager.MainStructureSet.AddNewStructure(this);
            TiberiumManager = parent.Map.GetComponent<MapComponent_Tiberium>();
            UpdateConnections(out TiberiumNetwork n);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            StructureSet.ParentDestroyed();
            if (Container.TotalStorage > 0f && mode != DestroyMode.Vanish)
            {
                if ((mode == DestroyMode.Deconstruct || mode == DestroyMode.Refund) && Props.dropsContents)
                {
                    PortableContainer container = (PortableContainer)ThingMaker.MakeThing(TiberiumDefOf.PortableContainer);
                    container.PostSetup(Container);
                    GenSpawn.Spawn(container, parent.Position, previousMap);
                }
                else
                {
                    int i = 0;
                    List<TiberiumCrystal> crystals = Container.PotentialCrystals();
                    Predicate<IntVec3> pred = c => c.InBounds(previousMap) && c.GetEdifice(previousMap) == null;
                    Action<IntVec3> action = delegate (IntVec3 c)
                    {
                        TiberiumCrystal crystal = crystals.ElementAtOrDefault(i);
                        if (crystal != null)
                        {
                            GenSpawn.Spawn(crystal, c, previousMap);
                            crystals.Remove(crystal);
                        }
                        i++;
                    };
                    TiberiumFloodInfo flood = new TiberiumFloodInfo(previousMap, pred, action);
                    flood.TryMakeFlood(out List<IntVec3> floodedCells, parent.OccupiedRect(), crystals.Count);
                }
            }
            base.PostDestroy(mode, previousMap);
            Network?.NotifyPotentialSplit(this);
        }

        public virtual void StructureSetOnAdd(CompTNW tnw, IntVec3 cell)
        {
            pipeExtensionCells.AddRange(CardinalConnectionCells.Intersect(tnw.InnerConnectionCells));
        }

        public virtual void StructureSetOnRemove(CompTNW tnw)
        {
            pipeExtensionCells.RemoveAll(c => tnw.InnerConnectionCells.Contains(c));
        }

        public void UpdateConnections(out TiberiumNetwork network)
        {
            StructureSet = new StructureSet(this);
            network = Network = new TiberiumNetwork(this, TNWManager);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!Network.IsWorking || !(CompPower?.PowerOn ?? false)) return;

            switch (NetworkMode)
            {
                case TNWMode.Storage:
                    DistributeValues();
                    break;
                case TNWMode.Consumer:
                    CollectValues();
                    break;
                case TNWMode.Producer:
                    StoreValues();
                    break;
            }
        }

        public virtual void CollectValues()
        {
            if (Container.CapacityFull) return;
            foreach (var storage in Network.NetworkSet.Storages)
            {
                if (storage.Container.Empty) continue;
                storage.Container.TryTransferTo(Container, storage.Container.AllStoredTypes.FirstOrDefault(), 1);
            }
        }

        public virtual void StoreValues()
        {
            if (!Container.HasStorage) return;
            foreach (var storage in Network.NetworkSet.Storages)
            {
                if (storage.Container.CapacityFull) continue;
                foreach (var tibType in Container.AllStoredTypes)
                {
                    Container.TryTransferTo(storage.Container, tibType, 1);
                }
            }
        }

        public virtual void DistributeValues()
        {
            if (!Container.HasStorage) return;
            foreach (var storage in Network.NetworkSet.Consumers)
            {
                if (storage.Container.CapacityFull) continue;
                foreach (var tibType in Container.AllStoredTypes)
                {
                    Container.TryTransferTo(storage.Container, tibType, 1);
                }
            }
        }

        public virtual void Notify_ContainerFull()
        {
        }

        public TiberiumNetwork Network
        {
            get => network;
            set
            {
                if (network != null && network != value)
                {
                    TNWManager.DeregisterNetwork(network);
                }
                network = value;
            }
        }

        public virtual IEnumerable<IntVec3> InnerConnectionCells => parent.OccupiedRect().Cells;
        public virtual IEnumerable<IntVec3> CardinalConnectionCells
        {
            get
            {
                if (cardinalCells.NullOrEmpty())
                {
                    foreach (IntVec3 c in GenAdj.CellsAdjacentCardinal(parent))
                    {
                        if (InnerConnectionCells.Any(v => v.AdjacentToCardinal(c)))
                        {
                            cardinalCells.Add(c);
                        }
                    }
                }
                return cardinalCells;
            }
        }

        public bool ConnectsTo(CompTNW other)
        {
            return CardinalConnectionCells.Any(other.InnerConnectionCells.Contains) && CompatibleWith(other);
        }

        private bool CompatibleWith(CompTNW other)
        {
            if (other.Network == null)
            {
                Log.Error(other.parent + " has missing Tiberium network even though it should have one.");
                return false;
            }

            return other.Network.NetworkMode == Network.NetworkMode;
        }

        public void PrintForGrid(SectionLayer layer)
        {
            TiberiumContent.TiberiumNetworkPipesOverlay.Print(layer, this.parent);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (!Network?.ValidFor(Props.tnwbMode, out string reason) ?? false)
            {
                Material mat = MaterialPool.MatFrom(TiberiumContent.MissingConnection, ShaderDatabase.MetaOverlay, Color.white);
                float num = (Time.realtimeSinceStartup + 397f * (float)(parent.thingIDNumber % 571)) * 4f;
                float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
                num2 = 0.3f + num2 * 0.7f;
                Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
                var c = parent.TrueCenter();
                Graphics.DrawMesh(MeshPool.plane08, new Vector3(c.x, AltitudeLayer.MetaOverlays.AltitudeFor(), c.z), Quaternion.identity, material, 0);
            }
            if (DebugConnectionCells && Find.Selector.IsSelected(parent))
            {
                GenDraw.DrawFieldEdges(pipeExtensionCells, Color.cyan);
                GenDraw.DrawFieldEdges(InnerConnectionCells.ToList(), Color.magenta);
                GenDraw.DrawFieldEdges(CardinalConnectionCells.ToList(), Color.green);
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            base.PostPrintOnto(layer);
            TiberiumContent.TiberiumNetworkPipes.Print(layer, parent);
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (!Network.IsWorking)
                sb.AppendLine("TR_MissingNetworkController".Translate());
            if (!Network.ValidFor(Props.tnwbMode, out string reason))
            {
                sb.AppendLine("TR_MissingConnection".Translate() + ":");
                if (!reason.NullOrEmpty())
                {
                    sb.AppendLine("   - " + reason.Translate());
                }
            }
            if (DebugSettings.godMode)
            {
                sb.AppendLine("Storage Mode: " + Container.AcceptedTypes.ToStringSafeEnumerable());
                sb.AppendLine("NetworkID: " + Network.NetworkID + " || " + "NetworkMode: " + Network.NetworkMode);
                sb.AppendLine("Has Connection: " + HasConnection);
                sb.AppendLine("Network Set Exists: " + (Network.NetworkSet != null));
                sb.AppendLine("Networks Silos: " + Network.NetworkSet.Silos.Count);
                sb.AppendLine("Networks Producers: " + Network.NetworkSet.Producers.Count);
                sb.AppendLine("Networks Consumers: " + Network.NetworkSet.Consumers.Count);
                sb.AppendLine("Networks Refineries: " + Network.NetworkSet.Refineries.Count);
                sb.AppendLine("Networks Storages: " + Network.NetworkSet.Storages.Count);
            }
            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            foreach (Gizmo g in Container.GetGizmos())
            {
                yield return g;
            }
            yield return new Designator_BuildFixed(parent.def);
            yield return new Designator_BuildFixed(TiberiumDefOf.TiberiumPipe);

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

    public class CompProperties_TNW : CompProperties
    {
        public int maxStorage = 0;
        public bool dropsContents = true;
        public bool storeEvenly = false;
        public List<TiberiumValueType> supportedTypes = new List<TiberiumValueType>();
        public TNWMode tnwbMode = TNWMode.None;
    }
}
