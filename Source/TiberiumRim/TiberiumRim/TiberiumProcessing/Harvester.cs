using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections;

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
        public int maxStorage;
        public float explosionRadius = 7;
        public ThingDef wreckDef;
        public List<TiberiumValueType> acceptedTypes = new List<TiberiumValueType>();
    }

    [StaticConstructorOnStartup]
    public class Harvester : MechanicalPawn
    {
        public new HarvesterKindDef kindDef;
        public TiberiumContainer Container;
        public bool forceReturn = false;

        private int waitingTicks = -1;
        private Building refineryBuilding;
        private CompTNW_Refinery mainRefinery;
        private IntVec3 homePos = IntVec3.Invalid;

        //SearchSettings
        public HarvestMode harvestMode = HarvestMode.Nearest;
        public TiberiumProducer producerToFocus;
        public TiberiumCrystalDef preferredType;

        // ProgressBar
        private static readonly Material UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);
        private static readonly Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 1f, 1f, 1f), ShaderDatabase.MetaOverlay);

        public override void ExposeData()
        {
            base.ExposeData();             
            Scribe_References.Look(ref refineryBuilding, "mainRefineryParent");
            Scribe_Values.Look(ref forceReturn, "forceReturn");
            Scribe_Values.Look(ref homePos, "homePosition");
            Scribe_Values.Look(ref harvestMode, "harvestMode");
            Scribe_Deep.Look(ref Container, "TiberiumContainer", new object[] { this });
            if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                mainRefinery = refineryBuilding.GetComp<CompTNW_Refinery>();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TNWManager.RegisterHarvester(this);
            this.kindDef = (HarvesterKindDef)base.kindDef;
            if (!respawningAfterLoad)
            {
                Container = new TiberiumContainer(kindDef.maxStorage, kindDef.acceptedTypes, this);
                if (refineryBuilding == null)
                { UpdateRefineries(); }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TNWManager.DeregisterHarvester(this);
            base.DeSpawn(mode);
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if(Container.TotalStorage > 0)
            {
                var spawnDef = TRUtils.CrystalDefFromType(Container.MainValueType, out bool isGas);
                float radius = kindDef.explosionRadius * Container.StoredPercent;
                int damage = (int)(10 * Container.StoredPercent);
                //TODO: Add Tiberium damagedef
                GenExplosion.DoExplosion(Position, Map,radius, DamageDefOf.Bomb, this, damage, 5, null, null, null, null, spawnDef, 0.18f);
            }
            GenSpawn.Spawn(kindDef.wreckDef, Position, Map);
            base.DeSpawn(DestroyMode.KillFinalize);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Downed)
            {
                this.Kill(null);
            }
            if (waitingTicks > 0)
            {
                waitingTicks--;
            }
            if (MainRefineryLost)
            {
                UpdateRefineries();
            }
        }

        public override Color[] ColorOverrides => new Color[] { Container.Color };
        public override float[] OpacityFloats => new float[] { Container.StoredPercent };
        public override bool[] DrawBools => new bool[] { true };

        public TiberiumCrystal HarvestTarget
        {
            get
            {
                if(!TNWManager.ReservationManager.TargetValidFor(this))
                {
                    TNWManager.ReservationManager.TryUpdate();
                }
                return TNWManager.ReservationManager.Reservations[this];
            }
        }

        public void SetMainRefinery(Building building)
        {
            if (refineryBuilding == building)
            { return; }

            if (mainRefinery != null)
            { mainRefinery.harvesters.Remove(this); }

            refineryBuilding = building;
            mainRefinery = building.TryGetComp<CompTNW_Refinery>();
            if (!mainRefinery.harvesters.Contains(this))
            { mainRefinery.harvesters.Add(this); }

            homePos = building.InteractionCell;
            Messages.Message("TR_HarvesterNewRefinery".Translate(this.def.LabelCap), building, MessageTypeDefOf.NeutralEvent);
        }

        public void SetToWait()
        {
            waitingTicks = GenTicks.SecondsToTicks(15);
        }

        public Building Refinery => refineryBuilding;

        public CompTNW_Refinery CurrentRefinery
        {
            get
            {
                if (mainRefinery.CanBeRefinedAt)
                {
                    return mainRefinery;
                }
                return AvailableRefinery;
            }
        }

        private CompTNW_Refinery AvailableRefinery
        {
            get
            {
                return TNWManager.MainStructureSet.Refineries.Find(r => r.CanBeRefinedAt);
            }
        }

        public void UpdateRefineries(Building forceMain = null, CompTNW_Refinery toIgnore = null)
        {
            if (forceMain != null)
            {
                SetMainRefinery(forceMain);
                return;
            }
            foreach (CompTNW_Refinery refinery in TNWManager.MainStructureSet.Refineries)
            {
                if (refinery != null && refinery != toIgnore)
                {
                    SetMainRefinery((Building)refinery.parent);
                }
            }
        }

        public TiberiumCrystal CurrentHarvestTarget
        {
            get
            {
                return TNWManager.ReservationManager.Reservations[this];
            }
        }

        public IntVec3 IdlePos
        {
            get
            {
                if(this.CanReach(homePos, PathEndMode.OnCell, Danger.Deadly))
                {
                    return homePos;
                }
                return Position;
            }
        }

        private bool TiberiumForModeAvailable
        {
            get
            {
                if (harvestMode == HarvestMode.Moss)
                {
                    return TiberiumManager.MossAvailable;
                }
                return TiberiumManager.TiberiumAvailable;
            }
        }

        public HarvesterPriority CurrentPriority
        {
            get
            {
                if (!Drafted)
                {
                    if (!IsWating && !ForcedReturn && !Unloading && !Container.CapacityFull && TiberiumForModeAvailable)
                    {
                        return HarvesterPriority.Harvest;
                    }
                    if (Container.StoredPercent > 0 && CanUnload)
                    {
                        return HarvesterPriority.Unload;
                    }
                    return HarvesterPriority.Idle;
                }
                return HarvesterPriority.None;
            }
        }

        private Texture2D TextureForMode
        {
            get
            {
                switch (harvestMode)
                {
                    case HarvestMode.Nearest:
                        return TiberiumContent.HarvesterNearest;
                    case HarvestMode.Value:
                        return TiberiumContent.HarvesterValue;
                    case HarvestMode.Moss:
                        return TiberiumContent.HarvesterMoss;
                }
                return BaseContent.BadTex;
            }
        }

        public bool IsWating => waitingTicks > 0;
        public bool ForcedReturn => forceReturn || mainRefinery.recallHarvesters;
        public bool CanUnload => CurrentRefinery != null ? this.CanReserve(CurrentRefinery.parent) : false;
        public bool MainRefineryLost => refineryBuilding.DestroyedOrNull() || mainRefinery == null;
        public bool Unloading => this.CurJobDef == TiberiumDefOf.UnloadAtRefinery;

        public bool ShouldIdle => CurrentPriority == HarvesterPriority.Idle;
        public bool ShouldHarvest => CurrentPriority == HarvesterPriority.Harvest;
        public bool ShouldUnload => CurrentPriority == HarvesterPriority.Unload;

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
                r.filledMat = FilledMat;
                r.unfilledMat = UnfilledMat;
                r.margin = 0.12f;
                GenDraw.DrawFillableBar(r);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine();
            if (DebugSettings.godMode)
            {
                sb.AppendLine("##Debug##");
                if (IsWating)
                {
                    sb.AppendLine("Waiting: " + waitingTicks.TicksToSeconds());
                }
                sb.AppendLine("Exact Storage: " + Container.TotalStorage);
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Mode: " + harvestMode);
                sb.AppendLine("Tiberium f/mode Available: " + TiberiumForModeAvailable);
                sb.AppendLine("Current Harvest Target: " + CurrentHarvestTarget);               
                sb.AppendLine("Valid: " + TNWManager.ReservationManager.TargetValidFor(this) + " CanBeHarvested: " + CurrentHarvestTarget?.CanBeHarvestedBy(this) + " Spawned: " + CurrentHarvestTarget?.Spawned + " Destroyed: " + CurrentHarvestTarget?.Destroyed);
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (this.Faction == Faction.OfPlayer)
            {
                foreach (Gizmo g in Container.GetGizmos())
                {
                    yield return g;
                }

                yield return new Command_Action
                {
                    defaultLabel = "TR_HarvesterMode".Translate(),
                    defaultDesc = "TR_HarvesterModeDesc".Translate(),
                    icon = TextureForMode,
                    action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        list.Add(new FloatMenuOption("TRHMode_Nearest".Translate(), delegate ()
                        {
                            harvestMode = HarvestMode.Nearest;
                        }));
                        list.Add(new FloatMenuOption("TRHMode_Valuable".Translate(), delegate ()
                        {
                            harvestMode = HarvestMode.Value;
                        }));
                        list.Add(new FloatMenuOption("TRHMode_Moss".Translate(), delegate ()
                        {
                            harvestMode = HarvestMode.Moss;
                        }));
                        FloatMenu menu = new FloatMenu(list);
                        menu.vanishIfMouseDistant = true;
                        Find.WindowStack.Add(menu);
                    },
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
                        if(thing != null)
                        {
                            if (thing is Building b && thing.TryGetComp<CompTNW_Refinery>() != null)
                            {
                                UpdateRefineries(b);                             
                            }
                        }
                    },
                };
            }
        }
    }
}
