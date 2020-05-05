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
    public class TiberiumFieldRuleset
    {
        public List<ThingGroupChance> floraOptions;
        public List<TerrainFloat> terrainOptions;
        public List<WeightedThing> crystalOptions;
        public bool allowFlora = true;

        [Unsaved]
        private float maxWeight;

        public TiberiumCrystalDef RandomTiberiumType()
        {
            return (TiberiumCrystalDef) crystalOptions.RandomElementByWeight(t => t.weight).thing;
        }

        public TRThingDef RandomPlant()
        {
            return (TRThingDef)floraOptions.SelectMany(o => o.plants).RandomElementByWeight(p => p.weight).thing;
        }

        public TRThingDef PlantAt(float distance, float maxDistance)
        {
            //"Chance" in this case is "DistancePercent"
            return (TRThingDef)floraOptions.Where(p => distance >= maxDistance * p.chance).SelectMany(p => p.plants).RandomElementByWeight(p => p.weight).thing;
        }

        public float MaxFloraWeight
        {
            get
            {
                if (maxWeight == 0)
                    maxWeight = floraOptions.Max(t => t.plants.Max(p => p.weight));
                return maxWeight;
            }
        }

        public float ChanceFor(TRThingDef plant, float atDistance, float maxDistance)
        { 
            float distanceChance = 1f - Mathf.InverseLerp(0f, maxDistance, atDistance);
            WeightedThing thing = floraOptions.SelectMany(f => f.plants).First(w => w.thing == plant);
            var weightChance = Mathf.InverseLerp(0f, MaxFloraWeight, thing.weight);
            var lerpedChance = Mathf.Lerp(distanceChance, 1f, Mathf.Clamp01(weightChance - (1f - distanceChance)));
            return lerpedChance; //Mathf.Lerp(distanceChance, 1f, Mathf.InverseLerp(0f, MaxFloraWeight, thing?.weight  ?? 0));
        }

        public IEnumerable<TerrainFloat> TerrainOptionsFor(TerrainDef terrain)
        {
            return terrainOptions.Where(t => ((TiberiumTerrainDef)t.terrainDef).SupportsTerrain(terrain));
        }

        public TerrainDef RandomTerrain()
        {
            return terrainOptions.RandomElementByWeight(t => t.value).terrainDef;
        }
    }

    public class AreaMutator : IExposable
    {
        private readonly float radius;
        private float maxRadius = 0;
        private float speed = 1;
        private Map map;
        private IntVec3 center;
        private List<IntVec3> remainingCells;
        private TiberiumField tibField;

        private readonly int mutationTicks;
        private readonly int startTick;
        private bool finished;

        public TiberiumFieldRuleset ruleset;

        //Reload Values after load
        public AreaMutator(IntVec3 center, Map map, TiberiumFieldRuleset ruleset, int mutationTicks, float speed = 1, TiberiumField field = null)
        {

        }

        public AreaMutator(List<IntVec3> cells, IntVec3 center, Map map, TiberiumFieldRuleset ruleset, int mutationTicks, float speed = 1, TiberiumField field = null)
        {
            remainingCells = cells;
            this.map = map;
            this.center = center;
            this.ruleset = ruleset;
            this.speed = speed;
            this.mutationTicks = mutationTicks;
            startTick = GenTicks.TicksGame;
            tibField = field;
            SetValuesFor(remainingCells);
        }

        public AreaMutator(IntVec3 center, Map map, float radius, TiberiumFieldRuleset ruleset, int mutationTicks, float speed = 1, TiberiumField field = null)
        {
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

        public void ExposeData()
        {
            //Save Values
            Scribe_Values.Look(ref finished, "finished");
        }

        public void Tick()
        {
            if (finished) return;
            if (remainingCells.NullOrEmpty() )
            {
                Finish();
                return;
            }
            //Iterate Over Remaining Cells
            for (int i = remainingCells.Count - 1; i >= 0; i--)
            {
                IntVec3 cell = remainingCells[i];
                if (center.DistanceTo(cell) > CurrentRadius) continue;

                remainingCells.Remove(cell);

                //Process Cell
                if (MutateCell(cell))
                    tibField?.AddFieldCell(cell);
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
            flooder.TryMakeFlood(out remainingCells, center, GenRadial.NumCellsInRadius(radius));
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
            Log.Message("TerrainChance: " + terrain + " " + terrain.plantChanceFactor);
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
            fieldString += "\nRemaining Cells: " + remainingCells.Count;
            fieldString += "\n" + (mutationTicks - (GenTicks.TicksGame - startTick)) + " Ticks Remaining";
            return fieldString;
        }

        private bool drawCells = false;
        public void DrawArea()
        {
            if(drawCells)
                GenDraw.DrawFieldEdges(remainingCells, Color.blue);
        }

        public IEnumerable<Gizmo> Gizmos()
        {
            yield return new Command_Action()
            {
                defaultLabel = "Remaining Cells",
                action = delegate { drawCells = !drawCells; }
            };
        }
    }
}
