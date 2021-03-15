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


        //Static Data
        private int mutationTicks;
        private int mutationInterval;
        private int seekerCellCount;
        private int speed = 1;
        public int desiredCells = 0;
        private float desiredRadius = 0;

        //Dynamic Data
        private int ticksLeft = -1;
        private bool finished;

        public TiberiumFieldRuleset Ruleset;

        //Dynamic Mutator
        private List<IntVec3> MutatedArea = new List<IntVec3>();
        private List<IntVec3> CurrentCells = new List<IntVec3>();
        private List<IntVec3> NewCells = new List<IntVec3>();

        public void ExposeData()
        {
            //Save Values
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref center, "center");
            Scribe_Values.Look(ref mutationTicks, "mutationTicks");
            Scribe_Values.Look(ref mutationInterval, "mutationInterval");
            Scribe_Values.Look(ref speed, "speed");
            Scribe_Values.Look(ref desiredCells, "desiredCells");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
            Scribe_Values.Look(ref finished, "finished");

            Scribe_Collections.Look(ref MutatedArea, "mutatedArea");
            Scribe_Collections.Look(ref CurrentCells, "currentCells");
            Scribe_Collections.Look(ref NewCells, "newCells");
        }

        //Reload Values after load
        public AreaMutator()
        {
        }

        public AreaMutator(TiberiumFieldRuleset ruleset, TiberiumField field = null)
        {
            this.Ruleset = ruleset;
            tibField = field;
        }

        public AreaMutator(IntVec3 center, float desiredRadius, Map map, TiberiumFieldRuleset ruleset, int mutationTicks, int speed = 1, TiberiumField field = null)
        {
            //remaining = new CellArea(map);
            this.map = map;
            this.center = center;
            this.Ruleset = ruleset;
            this.speed = speed;
            this.mutationTicks = mutationTicks;
            this.ticksLeft = mutationTicks;
            this.desiredRadius = desiredRadius;
            tibField = field;

            //Setup
            this.desiredRadius = desiredRadius;
            desiredCells = GenRadial.NumCellsInRadius(desiredRadius);
            mutationInterval = this.mutationTicks / desiredCells;

            CurrentCells = field?.MainProducer.OccupiedRect().ExpandedBy(1).Cells.ToList();
            seekerCellCount = CurrentCells.Count;
        }

        public bool HasBlossom => !tibField.BlossomTree.DestroyedOrNull();
        public bool Finished => finished || Math.Abs(ProgressPct - 1f) <= 0;
        public float ProgressPct => (mutationTicks - ticksLeft) / (float) mutationTicks;

        public void Tick()
        {
            if (finished) return;

            //Iterate Over Remaining Cells
            if (tibField.MainProducer.IsHashIntervalTick(mutationInterval))
            {
                for (var k = CurrentCells.Count - 1; k >= 0; k--)
                {
                    var cell = CurrentCells[k];
                    if (!cell.IsValid || !cell.InBounds(map))
                    {
                        CurrentCells.Remove(cell);
                        continue;
                    }
                    //Mutate 
                    if (MutateCell(cell))
                    {
                        MutatedArea.Add(cell);
                        tibField.AddFieldCell(cell, map);
                        desiredCells--;
                        //When mutation successfull, seek a new neighbor
                        if (TryGetNextCell(cell, out IntVec3 nextCell))
                        {
                            NewCells.Add(nextCell);
                        }
                    }
                    CurrentCells.Remove(cell);
                }
                if (desiredCells > 0 && NewCells.Count < seekerCellCount)
                {
                    while (NewCells.Count < seekerCellCount)
                    {
                        //Find New Edge Cell
                        var allPossibleCells = MutatedArea.SelectMany(GenAdjFast.AdjacentCellsCardinal);
                        var allAdjacentOnly = allPossibleCells.Except(MutatedArea).ToList();
                        if (allAdjacentOnly.NullOrEmpty())
                        {
                            break;
                        }
                        var randomElement = allAdjacentOnly.RandomElement();
                        if(randomElement.InBounds(map))
                            NewCells.Add(randomElement);
                    }
                }
                CurrentCells.AddRange(NewCells);
                NewCells.Clear();
            }

            if (ticksLeft <= 0 || desiredCells <= 0)
            {
                Finish();
                return;
            }

            if (ticksLeft > 0)
                ticksLeft -= speed;
        }

        private bool TryGetNextCell(IntVec3 from, out IntVec3 nextCell)
        {
            nextCell = IntVec3.Invalid;
            bool Predicate(IntVec3 c) => !c.IsSuppressed(map) && (!c.IsBlocked(map, out bool byPlant) || byPlant) && Ruleset.AllowTerrain(c.GetTerrain(map));
            int attempts = 0;
            while (!nextCell.InBounds(map) || NewCells.Contains(nextCell) || MutatedArea.Contains(nextCell) || !Predicate(nextCell))
            {
                if (attempts > 8)
                {
                    nextCell = from;
                    return false;
                }
                nextCell = GenAdjFast.AdjacentCellsCardinal(from).RandomElement();
                attempts++;
            }
            return true;
        }

        private bool MutateCell(IntVec3 cell)
        {
            if (!TryMutateTerrainAt(cell, out TiberiumTerrainDef newTerr)) return false;
            if (cell.IsBlocked(map, out bool byPlant) && !byPlant) return false;
            if (cell.HasTiberium(map)) return true;

            if (Ruleset.SpawnsTib && TRUtils.Chance(Ruleset.tiberiumDensity))
                GenTiberium.SpawnTiberium(cell, map, tibField.MainProducer.TiberiumTypes.RandomElement(),
                    tibField.MainProducer);
            else if (Ruleset.allowFlora)
                TryMutateFlora(cell, newTerr);
        
            return true;
        }

        private bool TryMutateTerrainAt(IntVec3 pos, out TiberiumTerrainDef newTerr)
        {
            TerrainDef oldTerr = pos.GetTerrain(map);
            if (oldTerr.IsTiberiumTerrain())
            {
                newTerr = (TiberiumTerrainDef)oldTerr;
                return true;
            }
            newTerr = Ruleset.RandomOutcome(oldTerr) as TiberiumTerrainDef;
            return newTerr != null && newTerr.TryCreateOn(pos, map, out newTerr);
        }

        private void TryMutateFlora(IntVec3 pos, TiberiumTerrainDef terrain)
        {
            var oldPlant = pos.GetPlant(map);
            if (oldPlant is TiberiumPlant) return;

            float distance = center.DistanceTo(pos);
            //float chance = 1f - Mathf.InverseLerp(0f, maxRadius, distance);

            //if (!TRUtils.Chance(ruleset.ChanceFor() * terrain.plantChanceFactor)) return;
            TRThingDef flora = Ruleset.PlantAt(distance, desiredRadius);
            if (!TRUtils.Chance(Ruleset.ChanceFor(flora, distance, desiredRadius) * terrain.plantChanceFactor)) return;

            if (flora == null) return;
            Thing plant = ThingMaker.MakeThing(flora);

            if (plant is Plant p) p.Growth = TRUtils.Range(0.3f, 0.9f);
            oldPlant?.DeSpawn();
            GenSpawn.Spawn(plant, pos, map);
        }

        public TiberiumProducer CreateBlossom()
        {
            bool Predicate(IntVec3 x) => x.InBounds(map) && !x.IsSuppressed(map) && tibField.FieldCells.Contains(x);
            var potentialCells = tibField.FieldCells.Where(Predicate).ToList();
            if (potentialCells.EnumerableNullOrEmpty()) return null;
            var randomCell = potentialCells.RandomElementByWeight(t =>
            {
                var dist = t.DistanceTo(center);
                if (dist < desiredRadius * 0.75f)
                    return 0;
                return dist / desiredRadius;
            });
            if (!randomCell.IsValid) return null;
            var blossom = GenTiberium.BlossomTreeFrom(tibField.MainProducer.def);
            if (blossom == null) return null;
            var blossomTree = (TiberiumProducer)GenSpawn.Spawn(blossom, randomCell, map);
            tibField.BlossomTree = blossomTree;
            return blossomTree;
        }

        private void Finish()
        {
            finished = true;
            if (Ruleset.createBlossom && !HasBlossom)
            {
                CreateBlossom();
            }
            MutatedArea.Clear();
            NewCells.Clear();
            CurrentCells.Clear();
        }

        public string InspectString()
        {
            if (finished) return "Fully Mutated";
            string fieldString = "Area Mutation:";//"TR_TibMutatorLabel".Translate(((TiberiumCrystalDef) ruleset.crystalOptions.First().thing).TiberiumValueType.ToString());
            fieldString += $"\nProgress: {ProgressPct.ToStringPercent()}";
            fieldString += $"\nRemaining Cells: {desiredCells}";
            fieldString += $"\n{(ticksLeft)} Ticks Remaining";
            return fieldString;
        }

        private bool drawCells = false;

        public void DrawArea()
        {
            if (drawCells)
            {
                GenDraw.DrawFieldEdges(MutatedArea, Color.blue);
                GenDraw.DrawFieldEdges(CurrentCells, Color.green);
                GenDraw.DrawFieldEdges(NewCells, Color.cyan);
            }
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
