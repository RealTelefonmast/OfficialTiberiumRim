using System.Collections.Generic;
using System.Text;
using RimWorld;
using TeleCore;
using TeleCore.FlowCore;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public enum HarvestMode
    {
        Nearest,
        Value,
        Moss,
    }

    public enum HarvesterPriority
    {
        Drafted,
        Harvest,
        Unload,
        Idle
    }

    public class HarvesterKindDef : MechanicalPawnKindDef
    {
        public float unloadValue = 0.125f;
        public float harvestValue = 0.125f;
        public ThingDef wreckDef;
        public ContainerConfig containerConfig;
        //public List<NetworkValueDef> handledValues;
    }

    public class Harvester : MechanicalPawn, IContainerImplementer<NetworkValueDef, IContainerHolderNetworkThing, NetworkContainerThing<IContainerHolderNetworkThing>>, IContainerHolderNetworkThing
    {
        private NetworkContainerThing<IContainerHolderNetworkThing> _containerInt;
        public new HarvesterKindDef kindDef => (HarvesterKindDef) base.kindDef;
        protected NetworkContainerThing<IContainerHolderNetworkThing> container;

        //Data
        private IntVec3 lastKnownPos;
        private CompTNS_Refinery compRefinery;
        private Comp_AnimationRenderer compAnimation;

        //Settings
        private bool forceReturn = false;
        private HarvestMode harvestMode = HarvestMode.Nearest;
        private TiberiumProducer preferredProducer;
        private TiberiumCrystalDef preferredType;

        public CompTNS_Refinery RefineryComp => compRefinery;
        
        public Comp_AnimationRenderer Animator => compAnimation;

        public NetworkContainerThing<IContainerHolderNetworkThing> Container => _containerInt;

        public NetworkDef NetworkDef { get; }
        
        public Building Refinery
        {
            get => ParentBuilding;
            set => ParentBuilding = value;
        }

        public bool AtRefinery => Position == Refinery?.InteractionCell;

        public bool AnyAvailableRefinery(out Building building)
        {
            building = null;
            return false;
        }

        public IntVec3 IdlePos => RefineryComp?.PositionFor(this) ?? lastKnownPos;

        public HarvestMode HarvestMode
        {
            get => harvestMode;
            private set => harvestMode = value;
        }
        public TiberiumProducer PreferredProducer
        {
            get => preferredProducer;
            private set => preferredProducer = value;
        }
        public TiberiumCrystalDef PreferredType
        {
            get => preferredType;
            private set => preferredType = value;
        }

        public bool ForceReturn
        {
            get => forceReturn;
            private set => forceReturn = value;
        }

        //User Control
        private bool TimeOut => idleTicksLeft > 0;
        private bool ShouldReturnToIdle => ForceReturn || RefineryComp.RecallHarvesters || TimeOut;
        public bool PlayerInterrupt => ShouldReturnToIdle || Drafted;

        //Data Bools
        private bool ContainerFull => Container.Full;

        private bool HasAvailableTiberium => HarvestMode == HarvestMode.Moss ? TiberiumManager.MossAvailable : TiberiumManager.TiberiumAvailable;

        //Priority Bools
        private bool ShouldIdle    => Container.Empty && (!HasAvailableTiberium || (Container.TotalStored > 0 && RefineryComp.Container.Full));
        private bool ShouldHarvest => !ContainerFull && HasAvailableTiberium;//CurrentPriority == HarvesterPriority.Harvest;
        private bool ShouldUnload  => ContainerFull || (container.TotalStored > 0 && !HasAvailableTiberium);

        private bool CanHarvest => !IsUnloading; // !ContainerFull && HasAvailableTiberium;
        private bool CanUnload  => Container.TotalStored > 0 && RefineryComp.CanBeRefinedAt;

        public bool IsHarvesting
        {
            get
            {
                if (CurJob == null)
                    return false;
                if (jobs.curDriver is JobDriver_HarvestTiberium harvest)
                    return true;
                return false;
            }
        }

        public bool IsUnloading
        {
            get
            {
                if (CurJob == null)
                    return false;
                if (jobs.curDriver is JobDriver_UnloadAtRefinery unload)
                    return true;
                return false;
            }
        }

        public HarvesterPriority CurrentPriority
        {
            get
            {
                if (Drafted) return HarvesterPriority.Drafted;
                if (ShouldReturnToIdle)
                {
                    if (CanUnload)
                        return HarvesterPriority.Unload;
                    return HarvesterPriority.Idle;
                }
                if (ShouldHarvest && CanHarvest)
                {
                    return HarvesterPriority.Harvest;
                }
                if (ShouldUnload && CanUnload)
                {
                    return HarvesterPriority.Unload;
                }
                return HarvesterPriority.Idle;
            }
        }

        public Thing Thing => this;
        public bool ShowStorageGizmo { get; }

        public void Notify_AddedContainerValue(NetworkValueDef def, float value)
        {
        }

        public void Notify_ContainerStateChanged(NotifyContainerChangedArgs<NetworkValueDef> args)
        {
            throw new System.NotImplementedException();
        }

        public string ContainerTitle => "TODO: Harvester Container";
        //ValueContainerBase<NetworkValueDef> Container => (BaseContainer<NetworkValueDef>) this.Container
        public bool DropsContents => false;
        public bool LeavesPhysicalContainer => false;

        //FX
        public override bool FX_ProvidesForLayer(FXArgs args) => args.index == 0;
        public override Color? FX_GetColor(FXLayerArgs args) => Container.Color;
        public override float? FX_GetOpacity(FXLayerArgs args) => Container.StoredPercent;
        public override bool? FX_ShouldDraw(FXLayerArgs args) => true;

        public void Notify_ContainerFull() { }
        public void Notify_ContainerStateChanged()
        {
        }

        public void Notify_RefineryDestroyed(CompTNS_Refinery notifier)
        {
            ResolveNewRefinery(notifier);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Settings
            Scribe_Values.Look(ref forceReturn, "ForceReturn");
            Scribe_Values.Look(ref harvestMode, "HarvestMode");
            Scribe_References.Look(ref preferredProducer, "prefProducer");
            Scribe_Defs.Look(ref preferredType, "prefType");
            //Data
            Scribe_Container.Look(ref container, kindDef.containerConfig, this, "tibContainer");
            Scribe_Values.Look(ref lastKnownPos, "lastPos");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                compAnimation = this.GetComp<Comp_AnimationRenderer>();
                if (Refinery != null)
                {
                    compRefinery = Refinery.GetComp<CompTNS_Refinery>();
                }
                else
                {
                    ResolveNewRefinery();
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compAnimation = this.GetComp<Comp_AnimationRenderer>();
            if (!respawningAfterLoad)
            {
                container = new NetworkContainerThing<IContainerHolderNetworkThing>(kindDef.containerConfig, this);
                if (ParentBuilding == null)
                { 
                    ResolveNewRefinery(); 
                }
            }

            //TiberiumManager.HarvesterInfo.RegisterHarvester(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            base.Kill(dinfo, exactCulprit);
            Container.Notify_ParentDestroyed(DestroyMode.KillFinalize, Map);
            GenSpawn.Spawn(kindDef.wreckDef, Position, Map);
            this.DeSpawn(DestroyMode.KillFinalize);
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            //TODO: EVA Warning
            //GameComponent_EVA.EVAComp().ReceiveSignal();
        }

        public override void Tick()
        {
            base.Tick();
            if (idleTicksLeft > 0)
            {
                idleTicksLeft--;
            }
        }

        public bool CanHarvestTiberium(TiberiumCrystalDef crystal)
        {
            return crystal.IsMoss ? HarvestMode == HarvestMode.Moss : HarvestMode != HarvestMode.Moss;
        }

        public void SetMainRefinery(Building building, CompTNS_Refinery refinery, CompTNS_Refinery lastParent)
        {
            if(lastParent != null)
            {
                Refinery = null;
                lastParent.RemoveHarvester(this);
                Messages.Message("TR_HarvesterNewRefinery".Translate(), new LookTargets(building, this), MessageTypeDefOf.NeutralEvent);
            }
            lastKnownPos = building.InteractionCell;
            Refinery = building;
            compRefinery = refinery;
            compRefinery.AddHarvester(this);
        }

        private void ResolveNewRefinery(CompTNS_Refinery lastParent = null)
        {
            var Refineries = NetworkInfo[TiberiumDefOf.TiberiumNetwork].TotalPartSet[NetworkRole.Producer];
            if (Refineries.Count <= 0) return;

            foreach (var refinery in Refineries)
            {
                if (refinery.Parent is not CompTNS_Refinery) continue;
                if (refinery.Parent == lastParent) continue;
                SetMainRefinery((Building)refinery.Parent.Thing, (CompTNS_Refinery)refinery.Parent, lastParent);
                return;
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (Find.Selector.IsSelected(this) && Find.CameraDriver.CurrentZoom <= CameraZoomRange.Middle)
            {
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = DrawPos;
                r.center.z += 1.5f;
                r.size = new Vector2(3, 0.15f);
                r.fillPercent = Container.StoredPercent;
                r.filledMat = TiberiumContent.Harvester_FilledBar;
                r.unfilledMat = TiberiumContent.Harvester_EmptyBar;
                r.margin = 0.12f;
                GenDraw.DrawFillableBar(r);
            }
        }

        private Texture2D HarvestModeTexture
        {
            get
            {
                return HarvestMode switch
                {
                    HarvestMode.Nearest => TiberiumContent.HarvesterNearest,
                    HarvestMode.Value => TiberiumContent.HarvesterValue,
                    HarvestMode.Moss => TiberiumContent.HarvesterMoss,
                    _ => BaseContent.BadTex
                };
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (Faction != Faction.OfPlayer) yield break;

            if (DebugSettings.godMode)
            {
            }

            foreach (var g in Container.GetGizmos())
            {
                yield return g;
            }

            //Preferences
            yield return new Command_Action
            {
                defaultLabel = "TR_HarvesterMode".Translate(),
                defaultDesc = "TR_HarvesterModeDesc".Translate(),
                icon = HarvestModeTexture,
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("TRHMode_Nearest".Translate(),
                        delegate () { harvestMode = HarvestMode.Nearest; }));
                    list.Add(new FloatMenuOption("TRHMode_Valuable".Translate(),
                        delegate () { harvestMode = HarvestMode.Value; }));
                    list.Add(new FloatMenuOption("TRHMode_Moss".Translate(),
                        delegate () { harvestMode = HarvestMode.Moss; }));
                    FloatMenu menu = new FloatMenu(list);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                },
            };

            /*
            yield return new Command_Target
            {
                defaultLabel = "TR_ProducerPrefLabel".Translate(),
                defaultDesc = "TR_ProducerPrefDesc".Translate(),
                icon = BaseContent.BadTex,
                action = delegate (Thing thing)
                {

                }
            };


            yield return new Command_Action
            {
                defaultLabel = "TR_TypePrefLabel".Translate(),
                defaultDesc = "TR_TypePrefDesc".Translate(),
                icon = BaseContent.BadTex,
                action =
                {

                }
            };
            */

            yield return new Command_Action
            {
                defaultLabel = ForceReturn ? "TR_HarvesterHarvest".Translate() : "TR_HarvesterReturn".Translate(),
                defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
                icon = ForceReturn ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
                action = delegate
                {
                    ForceReturn = !ForceReturn;
                    this.jobs.EndCurrentJob(JobCondition.InterruptForced);
                },
            };

            yield return new Command_Target
            {
                defaultLabel = "TR_HarvesterRefinery".Translate(),
                defaultDesc = "TR_HarvesterRefineryDesc".Translate(),
                icon = TiberiumContent.HarvesterRefinery,
                targetingParams = RefineryTargetInfo.ForHarvester(),
                action = delegate(LocalTargetInfo target){
                    if (target == null) return;
                    Thing thing = target.Thing;
                    if (thing is Building building)
                    {
                        var refinery = thing.TryGetComp<CompTNS_Refinery>();
                        if (refinery != null)
                            SetMainRefinery(building, refinery, RefineryComp);
                        //UpdateRefineries(b);
                    }
                }
            };
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine();
            if (DebugSettings.godMode)
            {
                sb.AppendLine("##Debug##");
                sb.AppendLine("CurJob: " + this.CurJob);
                sb.AppendLine("Priority: " + CurrentPriority);
                sb.AppendLine("Has Tiberium: " + HasAvailableTiberium);
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Can Harvest: " + CanHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Can Unload: " + CanUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Player Interrupted: " + PlayerInterrupt);
                sb.AppendLine("Is Unlading: " + IsUnloading);
                sb.AppendLine("Is Harvesting: " + IsHarvesting);

                /*
                if (IsWaiting)
                {
                    sb.AppendLine("Waiting: " + waitingTicks.TicksToSeconds());
                }

                sb.AppendLine("Drafted: " + Drafted);
                sb.AppendLine("Is Waiting: " + IsWaiting);
                sb.AppendLine("Forced Return: " + ForcedReturn);
                sb.AppendLine("Unloading: " + Unloading);
                sb.AppendLine("Capacity Full: " + Container.CapacityFull);
                sb.AppendLine("Tiberium f/Role Available: " + HasAvailableTiberium);
                sb.AppendLine("Can Unload: " + CanUnload);

                sb.AppendLine("Exact Storage: " + Container.TotalStored);
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Mode: " + harvestMode);
                sb.AppendLine("HarvesterQueue: " + HarvestQueue.Count);
                sb.AppendLine("Contained In Queue:" + HarvestQueue.ToStringSafeEnumerable());
                */
                //sb.AppendLine("Current Harvest Target: " + CurrentHarvestTarget);               
                //sb.AppendLine("Valid: " + TNWManager.ReservationManager.TargetValidFor(this) + " CanBeHarvested: " + CurrentHarvestTarget?.CanBeHarvestedBy(this) + " Spawned: " + CurrentHarvestTarget?.Spawned + " Destroyed: " + CurrentHarvestTarget?.Destroyed);
            }
            return sb.ToString().TrimEndNewlines();
        }

        private int failedTibSearches;
        private int idleTicksLeft;

        public void Notify_CouldNotFindTib()
        {
            failedTibSearches++;
            if (failedTibSearches > 10)
            {
                //Start idle hour
                idleTicksLeft = Mathf.RoundToInt(GenDate.TicksPerHour * 0.75f);
                failedTibSearches = 0;
            }
        }
        
    }
}
