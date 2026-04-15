using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TiberiumRim
{
    public class HarvesterKindDef : PawnKindDef
    {
        public float maxStorage;

        public ThingDef destroyedThingDef;
    }

    [StaticConstructorOnStartup]
    public class Harvester : Mechanical_Pawn, IHarvestPreferenceSettable
    {
        public new HarvesterKindDef kindDef;

        public Building_Refinery mainRefinery;

        public List<Building_Refinery> availableRefineries = new List<Building_Refinery>();

        private IntVec3 homePosition = IntVec3.Invalid;

        public TiberiumCrystalDef TiberiumDefToPrefer;

        public TiberiumContainer Container;

        public bool shouldStopHarvesting = false;

        // True: Any - False: Most Value
        public bool harvestModeBool = false;

        private bool eradicateMoss = false;

        // ProgressBar

        private static readonly Material UnfilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);

        private static readonly Material FilledMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 1f, 1f, 1f), ShaderDatabase.MetaOverlay);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Building_Refinery>(ref mainRefinery, "mainRefinery");
            Scribe_Values.Look<bool>(ref shouldStopHarvesting, "shouldStopHarvesting");
            Scribe_Values.Look<IntVec3>(ref homePosition, "homePosition");
            Scribe_Values.Look<bool>(ref harvestModeBool, "harvestType");
            Scribe_Values.Look<bool>(ref eradicateMoss, "eradicateMoss");
            Scribe_Deep.Look<TiberiumContainer>(ref Container, "TiberiumContainer");
            Scribe_Collections.Look<Building_Refinery>(ref availableRefineries, "availableRefineries", LookMode.Reference);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.kindDef = (HarvesterKindDef)base.kindDef;
            if (!respawningAfterLoad)
            {
                Container = new TiberiumContainer(kindDef.maxStorage);
                this.homePosition = this.mainRefinery.InteractionCell;
                UpdateRefineriesOrAddNewMain(null);
            }
        }

        public override void DeSpawn()
        {
            foreach (Building_Refinery refinery in availableRefineries)
            {
                if (refinery.harvesterList.Contains(this))
                {
                    refinery.harvesterList.Remove(this);
                }
            }
            base.DeSpawn();
        }     

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {          
            TiberiumCrystalDef spawnDef = null;
            if (Container.GetTotalStorage > 0)
            {
                spawnDef = TRUtils.CrystalDefFromType(Container.MainType);
                float radius = 1 + ((Container.GetTotalStorage / this.kindDef.maxStorage) * 5);
                int cells = GenRadial.NumCellsInRadius(radius);
                GenExplosion.DoExplosion(this.Position, this.Map, radius, DamageDefOf.Bomb, this, 10, null, null, null, spawnDef, (Container.GetTotalStorage / (Container.GetTotalStorage / 25)) / cells);
            }
            GenSpawn.Spawn(this.kindDef.destroyedThingDef, this.Position, this.Map);            
            this.DeSpawn();
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Downed)
            {
                this.Kill(null);
            }
            if (MainRefineryLost)
            {                
                if (AvailableRefinery != null)
                {
                    this.UpdateRefineriesOrAddNewMain(null);
                }
            }
        }

        public void UpdateRefineriesOrAddNewMain(Building_Refinery ignore)
        {
            foreach (Building_Refinery refinery in Map.listerBuildings.allBuildingsColonist.FindAll((Building x) => x is Building_Refinery && x != ignore))
            {
                if (refinery != null)
                {
                    if (!availableRefineries.Contains(refinery))
                    {
                        availableRefineries.Add(refinery);
                        if (!refinery.harvesterList.Contains(this))
                        {
                            refinery.harvesterList.Add(this);
                        }
                        if (MainRefineryLost)
                        {
                            mainRefinery = refinery;
                            homePosition = mainRefinery.InteractionCell;
                            Messages.Message("NewRefinerySet".Translate(new object[] { this.def.LabelCap }), refinery, MessageTypeDefOf.NeutralEvent);
                        }
                    }
                }
            }
        }

        public IntVec3 HomePosition
        {
            get
            {
                if (AvailableRefinery != null)
                {
                    return AvailableRefinery.InteractionCell;
                }
                return homePosition;
            }
        }

        public Building_Refinery AvailableRefinery
        {
            get
            {
                if(this.mainRefinery != null)
                {
                    return this.mainRefinery;
                }
                foreach(Building_Refinery refinery in availableRefineries)
                {
                    if (refinery != null)
                    {
                        return refinery;
                    }
                }
                return null;
            }
        }

        public Building_Refinery AvailableRefineryToUnload
        {
            get
            {
                if (this.mainRefinery != null)
                {
                    if (!this.mainRefinery.NetworkComp.Container.CapacityFull)
                    {
                        return this.mainRefinery;
                    }
                }
                foreach (Building_Refinery refinery in availableRefineries)
                {
                    if (refinery != null)
                    {
                        if (!refinery.NetworkComp.Container.CapacityFull)
                        {
                            return refinery;
                        }
                    }
                }         
                return null;
            }
        }

        public bool MainRefineryLost
        {
            get
            {
                if (this.mainRefinery != null)
                {
                    return false;
                }
                return true;
            }
        }

        public bool Harvesting
        {
            get
            {
                if (this.CurJob != null)
                {
                    return this.CurJob.def.defName.Contains("SearchAndHarvestTiberium");
                }
                return false;
            }
        }

        public bool Unloading
        {
            get
            {
                if (this.CurJob != null)
                {
                    return this.CurJob.def == DefDatabase<JobDef>.GetNamed("ReturnToRefineryToUnload");
                }
                return false;
            }
        }

        public bool CanWork
        {
            get
            {
                return this.mainRefinery != null;
            }
        }

        public bool ShouldEradicate
        {
            get
            {
                return this.eradicateMoss;
            }
        }

        public bool ShouldHarvest
        {
            get
            {
                if (this.shouldStopHarvesting || this.Container.CapacityFull || (this.ShouldEradicate ? !this.Map.GetComponent<MapComponent_TiberiumHandler>().TiberiumExists : !this.Map.GetComponent<MapComponent_TiberiumHandler>().HarvestableTiberiumExists))
                {
                    return false;
                }
                return true;
            }
        }

        public bool ShouldUnload
        {
            get
            {
                if (this.Container.GetTotalStorage > 0)
                {
                    if (!this.ShouldHarvest && this.AvailableRefineryToUnload.CanBeRefinedAt)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ShouldIdle
        {
            get
            {
                if (!this.ShouldHarvest || this.AvailableRefineryToUnload == null)
                {
                    if (CurJob != null)
                    {
                        if (!this.CanReach(CurJob.targetA, PathEndMode.OnCell, Danger.Some))
                        {
                            if (!this.ShouldUnload)
                            {
                                return true;
                            }
                            else if (!this.CanReserve(CurJob.targetA))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public TiberiumCrystalDef GetTiberiumDefToPrefer()
        {
            return this.TiberiumDefToPrefer;
        }

        public void SetTiberiumDefToPrefer(TiberiumCrystalDef tibDef)
        {
            this.TiberiumDefToPrefer = tibDef;
        }

        private bool IsSelected
        {
            get
            {
                if (Find.Selector.IsSelected(this))
                {
                    return true;
                }
                return false;
            }
        }

        public override void Draw()
        {            
            base.Draw();
            if (IsSelected)
            {
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = this.DrawPos;
                r.center.z = r.center.z + 1.5f;
                r.size = new Vector2(3f, 0.15f);
                r.fillPercent = (Container.GetTotalStorage/this.kindDef.maxStorage);
                r.filledMat = Harvester.FilledMat;
                r.unfilledMat = Harvester.UnfilledMat;
                r.margin = 0.12f;
                GenDraw.DrawFillableBar(r);
                if (Prefs.DevMode)
                {
                    if (this.CurJob.targetA != null)
                    {
                        CellRenderer.RenderCell(this.CurJob.targetA.Cell, 0.44f);
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }          

            if(this.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            Command_Action returnToRefinery = new Command_Action();
            returnToRefinery.icon = (shouldStopHarvesting ? ContentFinder<Texture2D>.Get("UI/Icons/Harvester_Return", true) : ContentFinder<Texture2D>.Get("UI/Icons/Harvester_Continue", true));
            returnToRefinery.defaultLabel = (shouldStopHarvesting ? "ReturnToRefineryIdling".Translate() : "ReturnToRefineryHarvesting".Translate());
            returnToRefinery.hotKey = KeyBindingDefOf.Misc1;
            returnToRefinery.action = delegate
            {
                this.shouldStopHarvesting = !this.shouldStopHarvesting;
            };
            yield return returnToRefinery;

            Command_Target selectRefinery = new Command_Target();
            selectRefinery.defaultLabel = "SelectRefineryLabel".Translate();
            selectRefinery.defaultDesc = "SelectRefineryDesc".Translate();
            selectRefinery.icon = ContentFinder<Texture2D>.Get("UI/Icons/Harvester_RefineryRequest");
            selectRefinery.targetingParams = TiberiumTargetingParameters.ForRefinery();
            selectRefinery.hotKey = KeyBindingDefOf.Misc2;
            selectRefinery.action = delegate (Thing target)
            {
                if (target != null)
                {
                    if (target.def.thingClass == typeof(Building_Refinery))
                    {
                        Building_Refinery refinery = target as Building_Refinery;
                        if (refinery != mainRefinery)
                        {
                            this.mainRefinery = refinery;
                            if (!refinery.harvesterList.Contains(this))
                            {
                                refinery.harvesterList.Add(this);
                            }
                            return;
                        }
                        Messages.Message("SelectRefinerySame".Translate(), MessageTypeDefOf.RejectInput);
                        return;
                    }
                    Messages.Message("SelectRefineryWrong".Translate(), MessageTypeDefOf.RejectInput);
                }
            };
            yield return selectRefinery;

            if (this.eradicateMoss == false)
            {
                Command_SetTiberiumToPrefer setTiberium = new Command_SetTiberiumToPrefer();
                setTiberium.defaultLabel = "SetTiberiumLabel".Translate();
                setTiberium.defaultDesc = "SetTiberiumDesc".Translate();
                setTiberium.icon = ContentFinder<Texture2D>.Get("UI/Icons/Harvester_TargetTib");
                setTiberium.settable = this;
                setTiberium.map = this.Map;
                yield return setTiberium;

                Command_Action resetTiberium = new Command_Action();
                resetTiberium.defaultLabel = "ResetTiberium".Translate();
                resetTiberium.icon = ContentFinder<Texture2D>.Get("UI/Icons/Stop");
                resetTiberium.action = delegate
                {
                    this.TiberiumDefToPrefer = null;
                };
                yield return resetTiberium;

                Command_Action setMode = new Command_Action();
                setMode.defaultLabel = this.harvestModeBool == true ? "HarvesterModeNearest".Translate() : "HarvesterModeValue".Translate();
                setMode.defaultDesc = "HarvesterModeDesc".Translate();
                setMode.icon = (this.harvestModeBool == true ? ContentFinder<Texture2D>.Get("UI/Icons/Harvester_TargetClose") : ContentFinder<Texture2D>.Get("UI/Icons/Harvester_TargetWealth"));
                setMode.hotKey = KeyBindingDefOf.Misc3;
                setMode.action = delegate
                {
                    this.harvestModeBool = !this.harvestModeBool;
                };
                yield return setMode;
            }

            Command_Action eradicateMoss = new Command_Action();
            eradicateMoss.defaultLabel = this.eradicateMoss == true ? "ErMossActive".Translate() : "ErMossInActive".Translate();
            eradicateMoss.defaultDesc = "ErMossDesc".Translate();
            eradicateMoss.icon = (this.eradicateMoss == true ? ContentFinder<Texture2D>.Get("UI/Icons/MossEr_Active") : ContentFinder<Texture2D>.Get("UI/Icons/MossEr_InActive"));
            eradicateMoss.hotKey = KeyBindingDefOf.Misc4;
            eradicateMoss.action = delegate
            {
                this.eradicateMoss = !this.eradicateMoss;
            };
            yield return eradicateMoss;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.GetInspectString());
            sb.AppendLine("StoredTib".Translate() + ": "+ Mathf.RoundToInt(Container.GetTotalStorage));
            if (this.TiberiumDefToPrefer != null)
            {
                sb.AppendLine("CurPrefType".Translate() + ": "+ this.TiberiumDefToPrefer.LabelCap);
            }
            if (DebugSettings.godMode)
            {
                sb.AppendLine("Debug:");
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Should Eradicate: " + ShouldEradicate);
            }
            return sb.ToString().TrimEndNewlines();        
        }
    }
}
