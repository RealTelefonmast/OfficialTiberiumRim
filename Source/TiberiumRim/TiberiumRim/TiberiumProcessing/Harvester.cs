using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections;
using System.Runtime.CompilerServices;

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
        None,
        Idle,
        Harvest,
        Unload
    }

    public class HarvesterKindDef : MechanicalPawnKindDef
    {
        public float unloadValue = 0.125f;
        public float harvestValue = 0.125f;
        public int maxStorage;
        public float explosionRadius = 7;
        public ThingDef wreckDef;
        public List<TiberiumValueType> acceptedTypes = new List<TiberiumValueType>();
    }

    public class Harvester : MechanicalPawn, IContainerHolder
    {
        public new HarvesterKindDef kindDef;
        protected TiberiumContainer container;

        private LocalTargetInfo idlePosition;
        private int waitingTicksRemaining = -1;

        //Settings
        private bool forceReturn = false;
        private HarvestMode harvestMode = HarvestMode.Nearest;
        private TiberiumProducer preferredProducer;
        private TiberiumCrystalDef preferredType;


        public TiberiumContainer Container => container;

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

        public CompTNW_Refinery MainRefinery { get; set; }

        public bool ShouldNotifyEVA => false;
        public bool ForceReturn
        {
            get => forceReturn;
            private set => forceReturn = value;
        }

        public IntVec3 IdlePos => MainRefinery?.PositionFor(this) ?? Position;

        //Status Bools
        public bool ShouldIdle => CurrentPriority == HarvesterPriority.Idle;
        public bool ShouldHarvest => CurrentPriority == HarvesterPriority.Harvest;
        public bool ShouldUnload => CurrentPriority == HarvesterPriority.Unload;

        public bool MainRefineryLost => ParentBuilding.DestroyedOrNull() || MainRefinery == null;
        private bool HasAvailableTiberium => HarvestMode == HarvestMode.Moss ? TiberiumManager.MossAvailable : TiberiumManager.TiberiumAvailable;

        private bool ShouldReturn => ForceReturn || (MainRefinery?.RecallHarvesters ?? true);
        private bool IsWaiting => waitingTicksRemaining > 0;

        public bool CanHarvest => !Container.CapacityFull && HasAvailableTiberium;
        public bool CanUnload => MainRefinery != null && MainRefinery.CanBeRefinedAt && this.CanReserve(MainRefinery.parent);
        public bool Unloading => this.CurJobDef == TiberiumDefOf.UnloadAtRefinery;


        //FX Settings
        public override Color[] ColorOverrides => new Color[] { Container.Color };
        public override float[] OpacityFloats => new float[] { Container.StoredPercent };
        public override bool[] DrawBools => new bool[] { true };

        public HarvestMode HarvestMode
        {
            get => harvestMode;
            private set => harvestMode = value;
        }

        public HarvesterPriority CurrentPriority
        {
            get
            {
                if (Drafted) return HarvesterPriority.None;
                if (!IsWaiting && !ShouldReturn && !Unloading && CanHarvest)
                {
                    return HarvesterPriority.Harvest;
                }
                if (Container.StoredPercent > 0 && CanUnload)
                {
                    return HarvesterPriority.Unload;
                }
                return HarvesterPriority.Idle;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Settings
            Scribe_Values.Look(ref forceReturn, "ForceReturn;");
            Scribe_Values.Look(ref harvestMode, "HarvestMode");
            Scribe_References.Look(ref preferredProducer, "prefProducer");
            Scribe_Defs.Look(ref preferredType, "prefType");
            //Data
            Scribe_Deep.Look(ref container, "tibContainer");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                SetMainRefinery(parent, false);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.kindDef = (HarvesterKindDef)base.kindDef;
            if (!respawningAfterLoad)
            {
                container = new TiberiumContainer(kindDef.maxStorage, kindDef.acceptedTypes, this, this);
                if (ParentBuilding == null)
                { UpdateRefineries(); }
            }

            TiberiumManager.HarvesterInfo.RegisterHarvester(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumManager.HarvesterInfo.DeregisterHarvester(this);
            base.DeSpawn(mode);
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (Container.TotalStorage > 0)
            {
                var spawnDef = TRUtils.CrystalDefFromType(Container.MainValueType, out bool isGas);
                float radius = kindDef.explosionRadius * Container.StoredPercent;
                int damage = (int)(10 * Container.StoredPercent);
                //TODO: Add Tiberium damagedef
                GenExplosion.DoExplosion(Position, Map, radius, DamageDefOf.Bomb, this, damage, 5, null, null, null, null, spawnDef, 0.18f);
            }
            GenSpawn.Spawn(kindDef.wreckDef, Position, Map);
            this.DeSpawn(DestroyMode.KillFinalize);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            //TODO: Potential loss of tiberium
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Downed)
                this.Kill(null);
            if (waitingTicksRemaining > 0)
                waitingTicksRemaining--;
            if (MainRefineryLost)
                UpdateRefineries();
        }

        public bool CorrectModeFor(TiberiumCrystalDef crystalDef)
        {
            return crystalDef.IsMoss ? HarvestMode == HarvestMode.Moss : HarvestMode != HarvestMode.Moss;
        }

        public void UpdateRefineries(Building setMain = null, CompTNW toIgnore = null)
        {
            if (setMain != null)
            {
                SetMainRefinery(setMain, false);
                return;
            }

            if (TNWManager.MainStructureSet.Refineries.NullOrEmpty()) return;

            foreach (var refinery in TNWManager.MainStructureSet.Refineries.Where(refinery =>!refinery.parent.DestroyedOrNull() && refinery != toIgnore))
            {
                SetMainRefinery((Building) refinery.parent);
            }
        }

        public void SetMainRefinery(Building building, bool newRef = true)
        {
            if (ParentBuilding == building) return;
            //Clear Current Refinery
            MainRefinery?.RemoveHarvester(this);

            ParentBuilding = building;
            MainRefinery   = building.TryGetComp<CompTNW_Refinery>();
            MainRefinery.AddHarvester(this);

            if(newRef) Messages.Message("TR_HarvesterNewRefinery".Translate(this.def.LabelCap), new LookTargets(building, this), MessageTypeDefOf.NeutralEvent);
        }


        public void Notify_ContainerFull() { }

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

            yield return new Command_Action
            {
                defaultLabel = forceReturn ? "TR_HarvesterHarvest".Translate() : "TR_HarvesterReturn".Translate(),
                defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
                icon = forceReturn ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
                action = delegate
                {
                    forceReturn = !forceReturn;
                    this.jobs.EndCurrentJob(JobCondition.InterruptForced);
                },
            };

            yield return new Command_Target
            {
                defaultLabel = "TR_HarvesterRefinery".Translate(),
                defaultDesc = "TR_HarvesterRefineryDesc".Translate(),
                icon = TiberiumContent.HarvesterRefinery,
                targetingParams = RefineryTargetInfo.ForHarvester(),
                action = delegate (Thing thing)
                {
                    if (thing != null)
                    {
                        if (thing is Building b && thing.TryGetComp<CompTNW_Refinery>() != null)
                        {
                            UpdateRefineries(b);
                        }
                    }
                },
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
                sb.AppendLine("Tiberium f/mode Available: " + HasAvailableTiberium);
                sb.AppendLine("Can Unload: " + CanUnload);

                sb.AppendLine("Exact Storage: " + Container.TotalStorage);
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
    }
}
