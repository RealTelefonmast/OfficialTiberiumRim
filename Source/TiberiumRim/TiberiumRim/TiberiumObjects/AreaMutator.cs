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
        private int ticksLeft = -1;
        private int mutationTicks;
        private bool finished;

        public TiberiumFieldRuleset ruleset;

        public void ExposeData()
        {
            //Save Values
            Scribe_References.Look(ref map, "map");
            Scribe_Deep.Look(ref remaining, "remaining");
            Scribe_Values.Look(ref center, "center");
            Scribe_Values.Look(ref finished, "finished");
            Scribe_Values.Look(ref mutationTicks, "mutationTicks");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
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
            this.ticksLeft = mutationTicks;
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
            this.ticksLeft = mutationTicks;
            tibField = field;
            SetupArea();
        }

        public bool Finished => finished || Math.Abs(ProgressPct - 1f) <= 0;
        public float ProgressPct => (mutationTicks - ticksLeft) / (float) mutationTicks;
        public float MaxRadius => maxRadius;
        public float CurrentRadius => MaxRadius * ProgressPct;

        public void Tick()
        {
            if (finished) return;
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

            if (ticksLeft <= 0 || remaining.Empty())
            {
                Finish();
                return;
            }

            if (ticksLeft > 0)
                ticksLeft -= (int)speed;
        }

        private void SetupArea()
        {
            bool Predicate(IntVec3 c) => c.InBounds(map) && tibField.Producer.TiberiumTypes.Any(t => c.AllowsTiberiumTerrain(map, t));
            void Action(IntVec3 c)
            {
                float curDist = center.DistanceTo(c);
                if (curDist > MaxRadius)
                    maxRadius = curDist;
            }
            //TiberiumFloodInfo flooder = new TiberiumFloodInfo(map, Predicate, Action);
            //flooder.TryMakeFlood(out List<IntVec3> temp, center, GenRadial.NumCellsInRadius(radius));
            List<IntVec3> temp = TerrainGenerator.RandomRootPatch(tibField.Producer.OccupiedRect().ExpandedBy(1), map, radius, 8, Predicate, Action).ToList();
            remaining.AddRange(temp);
        }

        private void SetValuesFor(List<IntVec3> cells)
        {
            maxRadius = cells.Max(c => center.DistanceTo(c));
        }

        private bool MutateCell(IntVec3 cell)
        {
            if (!TryMutateTerrainAt(cell, out TiberiumTerrainDef newTerr)) return false;
            if(ruleset.allowFlora && cell.Standable(map))
                TrySpawnFloraAt(cell, newTerr);
            return true;
        }

        private bool TryMutateTerrainAt(IntVec3 pos, out TiberiumTerrainDef newTerr)
        {
            newTerr = null;
            TerrainDef oldTerr = pos.GetTerrain(map);
            if (oldTerr.IsTiberiumTerrain())
            {
                newTerr = (TiberiumTerrainDef)oldTerr;
                return false;
            }
            newTerr = ruleset.RandomOutcome(oldTerr) as TiberiumTerrainDef;
            return newTerr != null && newTerr.TryCreateOn(pos, map, out newTerr);
        }

        private void TrySpawnFloraAt(IntVec3 pos, TiberiumTerrainDef terrain)
        {
            var oldPlant = pos.GetPlant(map);
            if (oldPlant is TiberiumPlant) return;

            float distance = center.DistanceTo(pos);
            //float chance = 1f - Mathf.InverseLerp(0f, maxRadius, distance);

            //if (!TRUtils.Chance(ruleset.ChanceFor() * terrain.plantChanceFactor)) return;
            TRThingDef flora = ruleset.PlantAt(distance, MaxRadius);
            if (!TRUtils.Chance(ruleset.ChanceFor(flora, distance, MaxRadius) * terrain.plantChanceFactor)) return;

            if (flora == null) return;
            Thing plant = ThingMaker.MakeThing(flora);

            if (plant is Plant p) p.Growth = TRUtils.Range(0.3f, 0.9f);
            oldPlant?.DeSpawn();
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
            fieldString += $"\nProgress: {ProgressPct.ToStringPercent()}";
            fieldString += $"\nRemaining Cells: {remaining.Count}";
            fieldString += $"\n{(ticksLeft)} Ticks Remaining";
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
            if (!DebugSettings.godMode) yield break;

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
                    ticksLeft = 0;
                    //Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + ((mutationTicks - (ticksLeft))));
                }
            };
        }
    }
}
