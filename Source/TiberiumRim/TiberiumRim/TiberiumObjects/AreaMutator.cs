using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AreaMutator : IExposable
    {
        private readonly TiberiumField tibField;

        private Map map;
        private IntVec3 center;

        private CellArea remaining;

        private float maxRadius = 0;

        private float radius;
        private float speed = 1;
        private int mutationTicks;
        private int startTick;
        private bool finished;

        public TiberiumFieldRuleset ruleset;

        public void ExposeData()
        {
            //Save Values
            Scribe_References.Look(ref map, "map");
            Scribe_Deep.Look(ref remaining, "remaining");
            Scribe_Values.Look(ref center, "center");
            Scribe_Values.Look(ref finished, "finished");
            Scribe_Values.Look(ref startTick, "startTick");
            Scribe_Values.Look(ref mutationTicks, "mutationTicks");
            Scribe_Values.Look(ref speed, "speed");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref maxRadius, "maxRadius");
        }

        //Reload Values after load
        public AreaMutator()
        {
        }

        public AreaMutator(TiberiumFieldRuleset ruleset, TiberiumField field = null)
        {
            this.ruleset = ruleset;
            tibField = field;
        }

        public AreaMutator(List<IntVec3> cells, IntVec3 center, Map map, TiberiumFieldRuleset ruleset, int mutationTicks, float speed = 1, TiberiumField field = null)
        {
            remaining = new CellArea(map);
            remaining.AddRange(cells);
            this.map = map;
            this.center = center;
            this.ruleset = ruleset;
            this.speed = speed;
            this.mutationTicks = mutationTicks;
            startTick = GenTicks.TicksGame;
            tibField = field;
            SetValuesFor(remaining.Cells);
        }

        public AreaMutator(IntVec3 center, Map map, float radius, TiberiumFieldRuleset ruleset, int mutationTicks, float speed = 1, TiberiumField field = null)
        {
            remaining = new CellArea(map);
            this.map = map;
            this.center = center;
            this.radius = radius;
            this.ruleset = ruleset;
            this.speed = speed;
            this.mutationTicks = mutationTicks;
            startTick = GenTicks.TicksGame;
            tibField = field;
            SetupArea();
        }

        public bool Finished => Math.Abs(ProgressPct - 1) <= 0;
        public float ProgressPct => ((GenTicks.TicksGame - startTick) * speed)/(float)mutationTicks;
        public float CurrentRadius => maxRadius * ProgressPct;

        public void Tick()
        {
            if (finished) return;
            if (remaining.Empty())
            {
                Finish();
                return;
            }
            //Iterate Over Remaining Cells
            for (int i = remaining.Count - 1; i >= 0; i--)
            {
                IntVec3 cell = remaining[i];
                if (center.DistanceTo(cell) > CurrentRadius) continue;

                remaining.Remove(cell);

                //Process Cell
                if (MutateCell(cell))
                    tibField?.AddFieldCell(cell, map);
            }
        }

        private void SetupArea()
        {
            bool Predicate(IntVec3 c) => c.SupportsTiberiumTerrain(map);
            void Action(IntVec3 c)
            {
                float curDist = center.DistanceTo(c);
                if (curDist > maxRadius)
                    maxRadius = curDist;
            }
            TiberiumFloodInfo flooder = new TiberiumFloodInfo(map, Predicate, Action);
            flooder.TryMakeFlood(out List<IntVec3> temp, center, GenRadial.NumCellsInRadius(radius));
            remaining.AddRange(temp);
        }

        private void SetValuesFor(List<IntVec3> cells)
        {
            maxRadius = cells.Max(c => center.DistanceTo(c));
        }

        private bool MutateCell(IntVec3 cell)
        {
            if (!TryMutateTerrainAt(cell, out TiberiumTerrainDef newTerr)) return false;
            if(ruleset.allowFlora && cell.GetFirstBuilding(map) == null)
                TrySpawnFloraAt(cell, newTerr);
            return true;
        }

        public bool TryMutateTerrainAt(IntVec3 pos, out TiberiumTerrainDef newTerr)
        {
            newTerr = null;
            TerrainDef oldTerr = pos.GetTerrain(map);
            var options = ruleset.TerrainOptionsFor(oldTerr).ToList();
            if (!options.Any()) return false;
            newTerr = options.RandomElementByWeight(t => t.value).terrainDef as TiberiumTerrainDef;
            if (newTerr == null) return false;
            if (oldTerr.IsTiberiumTerrain())
            {
                newTerr = (TiberiumTerrainDef)oldTerr;
                return false;
            }
            return newTerr.TryCreateOn(pos, map, out newTerr);
        }

        public void TrySpawnFloraAt(IntVec3 pos, TiberiumTerrainDef terrain)
        {
            if (pos.GetPlant(map) is TiberiumPlant) return;

            float distance = center.DistanceTo(pos);
            //float chance = 1f - Mathf.InverseLerp(0f, maxRadius, distance);

            //if (!TRUtils.Chance(ruleset.ChanceFor() * terrain.plantChanceFactor)) return;
            TRThingDef flora = ruleset.PlantAt(distance, maxRadius);
            if (!TRUtils.Chance(ruleset.ChanceFor(flora, distance, maxRadius) * terrain.plantChanceFactor)) return;

            if (flora == null) return;
            Thing plant = ThingMaker.MakeThing(flora);

            if (plant is Plant p) p.Growth = TRUtils.Range(0.1f, 0.55f);
            GenSpawn.Spawn(plant, pos, map);
        }

        private void Finish()
        {
            finished = true;
        }

        public string InspectString()
        {
            if (finished) return "Fully Mutated";
            string fieldString = "Area Mutation:";//"TR_TibMutatorLabel".Translate(((TiberiumCrystalDef) ruleset.crystalOptions.First().thing).TiberiumValueType.ToString());
            fieldString += "\nProgress: " + ProgressPct.ToStringPercent();
            fieldString += "\nRemaining Cells: " + remaining.Count;
            fieldString += "\n" + (mutationTicks - (GenTicks.TicksGame - startTick)) + " Ticks Remaining";
            return fieldString;
        }

        private bool drawCells = false;
        public void DrawArea()
        {
            if(drawCells)
                GenDraw.DrawFieldEdges(remaining.Cells, Color.blue);
        }

        public IEnumerable<Gizmo> Gizmos()
        {
            yield return new Command_Action()
            {
                defaultLabel = "Show Remaining Cells",
                action = delegate { drawCells = !drawCells; }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Finish Area Mutation",
                action = delegate
                {
                    Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + ((mutationTicks - (GenTicks.TicksGame - startTick))));
                }
            };
        }
    }
}
