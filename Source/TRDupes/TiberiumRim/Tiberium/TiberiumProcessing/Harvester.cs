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
        All,
        Value,
        Moss
    }

    public class HarvesterKindDef : MechanicalPawnKindDef
    {
        public int maxStorage;
        public ThingDef destroyedThingDef;
    }

    [StaticConstructorOnStartup]
    public class Harvester : MechanicalPawn
    {
        public new HarvesterKindDef kindDef;     
        public TiberiumCrystalDef TiberiumDefToPrefer;
        public TiberiumContainer Container;
        public bool idleAtRefinery = false;
        public HarvestMode harvestMode = HarvestMode.All;

        private TNW_Refinery mainRefinery;
        private IntVec3 idlePosition = IntVec3.Invalid;
        private TiberiumCrystal currentHarvestTarget;
        // ProgressBar
        private static readonly Material UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);
        private static readonly Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 1f, 1f, 1f), ShaderDatabase.MetaOverlay);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mainRefinery, "mainRefinery");
            Scribe_Values.Look(ref idleAtRefinery, "idleAtRefinery");
            Scribe_Values.Look(ref idlePosition, "idlePosition");
            Scribe_Values.Look(ref harvestMode, "harvestMode");
            Scribe_Deep.Look(ref Container, "TiberiumContainer");
        }

        public void PreSetup(TNW_Refinery refinery)
        {
            mainRefinery = refinery;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.kindDef = (HarvesterKindDef)base.kindDef;
            if (!respawningAfterLoad)
            {
                Container = new TiberiumContainer(kindDef.maxStorage);
                this.idlePosition = this.MainRefinery.InteractionCell;
                UpdateRefineries();
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            //Remove Harvester from all refineries
            base.DeSpawn(mode);
        }

        public override void Tick()
        {
            base.Tick();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Downed? " + Downed);
            sb.AppendLine("FullBodyBusy? " + stances.FullBodyBusy);
            sb.AppendLine("Has StoryTracker? " + (this.story != null));
            sb.AppendLine("Violent Disabled: " + story?.WorkTagIsDisabled(WorkTags.Violent));
            sb.AppendLine("Tool User: " + RaceProps.ToolUser);
            sb.AppendLine("No Fire Fighting: " + story?.WorkTagIsDisabled(WorkTags.Firefighting));
            sb.AppendLine("Melee Verbs Null? " + (this.meleeVerbs == null));
            sb.AppendLine("Native Verbs Null? " + (natives == null));
            //Log.Message(sb.ToString());

            if (this.Downed)
            {
                this.Kill(null);
            }
            if (MainRefineryLost)
            {
                UpdateRefineries();
            }
        }

        public override Color[] ColorOverrides => new Color[1] { Container.Color };
        public override float[] OpacityFloats => new float[1] { Container.StoredPct };
        public override bool[] DrawBools => new bool[1] { true };

        public IEnumerator CalculateTiberium()
        {
            Predicate<IntVec3> predicate = x => x.IsValid && x.Standable(Map) && currentHarvestTarget == null;
            Action<IntVec3> action = delegate (IntVec3 c)
            {
                if (currentHarvestTarget == null)
                {
                    currentHarvestTarget = c.GetTiberium(Map);
                    if (currentHarvestTarget.HarvestValue < 1f || !this.CanReserve(currentHarvestTarget) || !this.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly))
                    {
                        currentHarvestTarget = null;
                    }
                }
            };
            Map.floodFiller.FloodFill(Position, predicate, action);
            yield return null;
        }

        public TiberiumCrystal HarvestTarget
        {
            get
            {
                return currentHarvestTarget;
            }
            set
            {
                currentHarvestTarget = value;
            }
        }

        public TNW_Refinery RefineryToUnload
        {
            get
            {
                if (!mainRefinery.DestroyedOrNull())
                {
                    if (!mainRefinery.Container.CapacityFull)
                    {
                        return mainRefinery;
                    }
                }
                return TiberiumNet.AllRefineries.Find(r => !r.DestroyedOrNull() && !r.Container.CapacityFull);
            }
        }

        public TNW_Refinery MainRefinery
        {
            get
            {
                if (!mainRefinery.DestroyedOrNull())
                {
                    return mainRefinery;
                }
                return AvailableRefinery;
            }
            set
            {
                mainRefinery = value;
            }
        }

        private TNW_Refinery AvailableRefinery
        {
            get
            {
                return TiberiumNet.AllRefineries.Find(r => !r.DestroyedOrNull() && !r.boundHarvesters.Contains(this));
            }
        }

        public void UpdateRefineries(TNW_Refinery forceMain = null)
        {
            foreach (TNW_Refinery refinery in TiberiumNet.AllRefineries)
            {
                if (refinery != null)
                {
                    if (!refinery.boundHarvesters.Contains(this))
                    {
                        refinery.boundHarvesters.Add(this);
                    }
                    if (MainRefineryLost)
                    {
                        SetMainRefinery(refinery);
                    }                  
                }
                if (forceMain != null)
                {
                    SetMainRefinery(forceMain);
                }
            }
        }

        public void SetMainRefinery(TNW_Refinery refinery)
        {
            mainRefinery = refinery;
            idlePosition = MainRefinery.InteractionCell;
            Messages.Message("NewRefinerySet".Translate(this.def.LabelCap), refinery, MessageTypeDefOf.NeutralEvent);
        }

        public IntVec3 IdlePos
        {
            get
            {
                if(this.CanReach(idlePosition, PathEndMode.OnCell, Danger.Deadly))
                {
                    return idlePosition;
                }
                return Position;
            }
        }

        private bool TiberiumAvailable
        {
            get
            {
                return TiberiumManager.TiberiumCrystals.Any(c => c.HarvestValue >= 1f);
            }
        }

        private bool MossAvailable
        {
            get
            {
                return TiberiumManager.TiberiumMoss.Count() > 0;
            }
        }

        private bool TiberiumForModeAvailable
        {
            get
            {
                if(harvestMode == HarvestMode.Moss)
                {
                    return MossAvailable;
                }
                return TiberiumAvailable;
            }
        }

        public bool MainRefineryLost
        {
            get
            {
                return MainRefinery == null;
            }
        }

        public bool ShouldIdle
        {
            get
            {
                if (!(ShouldUnload && CanUnload))
                {
                    return idleAtRefinery || !TiberiumForModeAvailable || (ShouldUnload && !CanUnload);
                }
                return false;
            }
        }

        public bool ShouldHarvest
        {
            get
            {
                return !Unloading && !ShouldIdle && !Container.CapacityFull && TiberiumForModeAvailable; 
            }
        }

        public bool ShouldUnload
        {
            get
            {
                return (Container.CapacityFull || (Container.StoredPct > 0f && !TiberiumForModeAvailable));
            }
        }

        public bool CanUnload
        {
            get
            {
                if (!RefineryToUnload.DestroyedOrNull())
                {
                    return RefineryToUnload.CanBeRefinedAt && this.CanReserve(RefineryToUnload);
                }
                return false;
            }
        }

        public bool Unloading
        {
            get
            {
                return this.CurJobDef == TiberiumDefOf.UnloadAtRefinery || ShouldUnload;
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
                r.fillPercent = Container.StoredPct;
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
            if (DebugSettings.godMode)
            {
                sb.AppendLine();
                sb.AppendLine("##Debug##");
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Tiberium f/mode Available: " + TiberiumForModeAvailable);
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
                yield return new Command_Action
                {
                    defaultLabel = idleAtRefinery ? "TR_Harvester_Return".Translate() : "TR_Harvester_Harvest".Translate(),
                    defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
                    icon = idleAtRefinery ? ContentFinder<Texture2D>.Get("UI/Icons/Network/Return") : ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvest"),
                    action = delegate
                    {
                        idleAtRefinery = !idleAtRefinery;
                    },
                };

                yield return new Command_Target
                {
                    defaultLabel = "TR_Harvester_RefinerySelect".Translate(),
                    defaultDesc = "TR_Harvester_RefinerySelectDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/NewRefinery"),
                    targetingParams = RefineryTargetInfo.ForHarvester(),
                    action = delegate (Thing thing)
                    {
                        if(thing != null)
                        {
                            TNW_Refinery refinery = thing as TNW_Refinery;
                            if (refinery != null)
                            {
                                UpdateRefineries(refinery);                             
                            }
                        }
                    },
                };
            }
        }
    }
}
