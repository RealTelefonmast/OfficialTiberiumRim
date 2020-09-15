﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /* TIBERIUM PRODUCER
     * The Tiberium Producer is the main source of Tiberium
     * Functionality:
     * Upon First Spawn:
     * Creates area of initial mutation area
     * Sets time until full maturity
     * Resets all counters
     * 
     * Phases:
     * Maturing - The inital mutation area slowly grows from center to outmost cell. (CellMutator Class?)
     * Spawning Blossoms
     * Spawning Tiberium Lattice
     * 
     * */

    public class TiberiumProducer : TRBuilding
    {
        public new TiberiumProducerDef def => (TiberiumProducerDef)base.def;

        private AreaMutator areaMutator;
        private TiberiumField tiberiumField;

        //Values
        protected int ticksUntilTiberium = -1;
        protected int ticksUntilSpore = -1;
        protected int ticksUntilBlossomTree = -1;

        //DebugSettings
        public bool turnOffLight = false;
        protected bool showField = false;
        protected bool showAffect = false;
        protected bool showTileIterator = false;
        protected bool showPotentialField = false;

        public TiberiumFieldRuleset Ruleset => def?.tiberiumFieldRules;

        public virtual bool ShouldSpawnTiberium => !TiberiumTypes.EnumerableNullOrEmpty() && IsMature;

        protected bool IsMature => (areaMutator?.Finished ?? true) || areaMutator.ProgressPct >= def.spawner.minProgressToSpread;
        protected virtual float GrowthRadius => def.spawner.growRadius;
        protected virtual int MutationTicks => (int)(GenDate.TicksPerDay * def.daysToMature);

        public TiberiumField TiberiumField => tiberiumField;
        public List<IntVec3> FieldCells => TiberiumField.FieldCells;
        public IEnumerable<TiberiumCrystalDef> TiberiumTypes => Ruleset.crystalOptions.NullOrEmpty() ? null : Ruleset.crystalOptions.Select(t => t.thing as TiberiumCrystalDef);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            Log.Message("   -Spawning Producer");
            base.SpawnSetup(map, respawningAfterLoad);
            
            if (respawningAfterLoad) return;
            //Init of saved components - Done Once

            //Init Tiberium Field
            tiberiumField = new TiberiumField(this);
            Log.Message("   -TiberiumField Set");
            //Init Tickers
            ResetTiberiumCounter();
            ResetSporeCounter();
            ResetBlossomCounter();

            foreach (var cell in this.OccupiedRect())
            {
                //TiberiumTypes.RandomElement().SpreadOutcomesAt(cell, map, out TerrainDef topTerr, out TerrainDef underTerr, out _);
                GenTiberium.SetTerrain(cell, map, TiberiumTypes.RandomElement());
            }

            //AreaMutator, sets and processes the initial corruption Area
            areaMutator = new AreaMutator(Position, map, GrowthRadius, def.tiberiumFieldRules, MutationTicks, 1f, tiberiumField);
            Log.Message("   -AreaMutator Set");
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref ticksUntilTiberium, "ticksUntilTiberium");
            Scribe_Values.Look(ref ticksUntilSpore, "ticksUntilSpore");
            Scribe_Values.Look(ref ticksUntilBlossomTree, "ticksUntilBlossomTree");
            Scribe_Deep.Look(ref tiberiumField, "tiberiumField");
            Scribe_Deep.Look(ref areaMutator, "areaMutator", def.tiberiumFieldRules, tiberiumField);
        }

        public override void Tick()
        {
            base.Tick();
            if (!Spawned) return;

            areaMutator?.Tick();
            tiberiumField?.Tick();

            if(areaMutator?.Finished ?? true)
                SpawningTick();
        }

        private void SpawningTick()
        {
            if (ticksUntilSpore == 0)
            {
                //if(TrySpawnBlossomSpore()) 
                    //ResetSporeCounter();
            }

            if (ticksUntilTiberium == 0 )
            {
                SpawnTiberium();
                ResetTiberiumCounter();
            }

            if (ticksUntilBlossomTree == 0)
            {
                //GrowBlossomTree();
                ResetBlossomCounter();
            }

            if (ticksUntilBlossomTree > 0)
                ticksUntilBlossomTree--;
            if (ticksUntilSpore > 0)
                ticksUntilSpore--;
            if (ticksUntilTiberium > 0)
                ticksUntilTiberium--;
        }

        public void AddBoundCrystal(TiberiumCrystal crystal)
        {
            tiberiumField.AddTiberium(crystal);
            if (crystal.def.HarvestType == HarvestType.Unharvestable) return;
            tiberiumField.AddFieldCell(crystal.Position, crystal.Map);
        }

        public void RemoveBoundCrystal(TiberiumCrystal crystal)
        {
            tiberiumField.RemoveTiberium(crystal);
            tiberiumField.RemoveFieldCell(crystal.Position, crystal.Map);
        }

        //TODO: Re-Add Spores, currently instant spawn
        private void SpawnTiberium(bool forceSpawn = false)
        {
            if (!forceSpawn && !ShouldSpawnTiberium) return;
            int spores;
            switch (def.spawner.spawnMode)
            {
                case TiberiumSpawnMode.Direct:
                    SpawnDirect(TiberiumTypes.RandomElement());
                    break;
                case TiberiumSpawnMode.Spore:
                    SpawnSpore(TiberiumTypes.RandomElement());
                    break;
                case TiberiumSpawnMode.SporeBurst:
                    //TODO: Use Actual Spores
                    spores = TRUtils.Range(def.spawner.explosionRange);
                    for (int i = 0; i < spores; i++)
                    {
                        var tibDef = TiberiumTypes.RandomElement();
                        CellFinder.TryFindRandomCellNear(Position, Map, (int)def.spawner.sporeExplosionRadius, c => c.CanSendSporeTo(Map, tibDef), out IntVec3 dest);
                        GenTiberium.SpawnTiberium(dest, Map, tibDef, this);
                    }
                    //GenTiberium.SpawnSpore(this.OccupiedRect(), def.spawner.sporeExplosionRadius, Map, TiberiumTypes.RandomElement(), this, spores, true);
                    break;
                case TiberiumSpawnMode.SporeExplosion:
                    spores = TRUtils.Range(def.spawner.explosionRange);
                    for (int i = 0; i < spores; i++)
                    {
                        var tibDef = TiberiumTypes.RandomElement();
                        CellFinder.TryFindRandomCellNear(Position, Map, (int)def.spawner.sporeExplosionRadius, c => c.CanSendSporeTo(Map, tibDef), out IntVec3 dest);
                        GenTiberium.SpawnTiberium(dest, Map, tibDef, this);
                    }
                    GenExplosion.DoExplosion(this.Position, Map, 6.76f, DamageDefOf.Bomb, this);
                    break;
            }
        }

        private void SpawnDirect(TiberiumCrystalDef def)
        {
            //var cells = this.CellsAdjacent8WayAndInside().Where(c => def.AllowsTiberiumAt(c, Map, out _, out _)).ToList();
            //if (cells.Any())
            GenTiberium.SpawnTiberium(this.RandomAdjacentCell8Way(), Map, def, this);
        }

        private void SpawnSpore(TiberiumCrystalDef def)
        {
            var cells = FieldCells.Where(c => !c.Roofed(Map) && GenTiberium.AllowsTiberiumAt(c, Map, def, out _)).ToList();
            if (cells.Any())
                GenTiberium.SpawnTiberium(cells.RandomElement(), Map, def, this);
            //  GenTiberium.SpawnSpore(this.OccupiedRect(), cells.RandomElement(), Map, def, this);
        }

        private void ResetBlossomCounter()
        {
            ticksUntilBlossomTree = TRUtils.Range(30000, 60000);
        }

        private void ResetTiberiumCounter()
        {
            if (def.spawner != null)
                ticksUntilTiberium = TRUtils.Range(def.spawner.spawnInterval);
        }

        private void ResetSporeCounter()
        {
            if(def.spore != null)
                ticksUntilSpore = TRUtils.Range(def.spore.spawnInterval);
        }

        private bool showForceGrow;
        private bool showGrowFrom;
        private bool showGrowTo;
        private bool showAffected;
        private bool showTiberium;

        public override void Draw()
        {
            base.Draw();
            areaMutator?.DrawArea();
            tiberiumField?.DrawField();

            //DebugDraw
            TiberiumGrid grid = TiberiumComp.TiberiumInfo.GetGrid();
            //Draw Tiberium Cells
            if(showTiberium)
                GenDraw.DrawFieldEdges(grid.tiberiumGrid.ActiveCells.ToList(), Color.red);
            //Draw CellPaths
            //if(showForceGrow)
               // GenDraw.DrawFieldEdges(grid.alwaysGrowFrom.ActiveCells.ToList(), Color.blue);
            //Draw From
            if (showGrowFrom)
                GenDraw.DrawFieldEdges(grid.growFromGrid.ActiveCells.ToList(), Color.green);
            //Draw To
            if (showGrowTo)
                GenDraw.DrawFieldEdges(grid.growToGrid.ActiveCells.ToList(), Color.cyan);
            //Draw Affected Cells
            if (showAffected)
                GenDraw.DrawFieldEdges(grid.affectedCells.ActiveCells.ToList(), Color.magenta);
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.GetInspectString());
            if (DebugSettings.godMode)
            {
                if (areaMutator != null)
                    sb.AppendLine(areaMutator.InspectString());
                if (tiberiumField != null)
                    sb.AppendLine(tiberiumField.InspectString());
            }
            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            foreach (var gizmo in areaMutator.Gizmos())
            {
                yield return gizmo;
            }

            foreach (var gizmo in tiberiumField.Gizmos())
            {
                yield return gizmo;
            }

            if (!DebugSettings.godMode) yield break;

            if (!TiberiumTypes.EnumerableNullOrEmpty())
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn " + TiberiumTypes?.RandomElement().label,
                    action = delegate
                    {
                        foreach (var pos in GenAdj.CellsAdjacent8Way(this))
                        {
                            GenTiberium.SpawnTiberium(pos, Map, TiberiumTypes.RandomElement(), this);
                        }
                    }
                };
            }

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Speed Up Growth ",
                action = delegate
                {
                    tiberiumField.DEBUGFastGrowth();
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show Tiberium",
                action = delegate { showTiberium = !showTiberium; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show ForceGrow",
                action = delegate { showForceGrow = !showForceGrow; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show GrowFrom",
                action = delegate { showGrowFrom = !showGrowFrom; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show GrowTo",
                action = delegate { showGrowTo = !showGrowTo; }
            };
            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Show Affected",
                action = delegate { showAffected = !showAffected; }
            };
        }
    }
}
