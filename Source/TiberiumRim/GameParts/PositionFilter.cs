using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TeleCore;
using TeleCore.Data.Events;
using UnityEngine;
using Verse;

namespace TR
{
    public class RadiationAvoidGrid : AvoidGridWorker
    {
        public RadiationAvoidGrid(Map map, AvoidGridDef def) : base(map, def)
        {
            var hediffGrid = map.Tiberium().TiberiumAffecter.HediffGrid;
            hediffGrid.AvoidGrid = this;
        }

        public override bool AffectsThing(Thing thing)
        {
            if (thing is Pawn pawn)
            {
                var healthComp = pawn.HealthComp();
                if (healthComp.HasGeiger)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetRadiation(IntVec3 pos, float pct)
        {
            SetAvoidValue(pos, (byte)Mathf.Lerp(0,255, pct));
        }
    }
    
    public class TiberiumAvoidGrid : AvoidGridWorker
    {
        public TiberiumAvoidGrid(Map map, AvoidGridDef def) : base(map, def) { }

        public override bool AffectsThing(Thing thing)
        {
            if (thing is Pawn pawn)
            {
                var healthComp = pawn.HealthComp();
                if (healthComp?.IsTiberiumImmune ?? false)
                {
                    return false;
                }
            }
            return true;
        }

        public override void Notify_CellChanged(CellChangedEventArgs args)
        {
            if (args.Thing is TiberiumCrater crystal)
            {
                if (args.ThingChangedArgs != null)
                {
                    switch (args.ThingChangedArgs.ChangeMode)
                    {
                        case ThingChangeFlag.Spawned:
                            SetAvoidValue(args.Cell, 255);
                            break;
                        case ThingChangeFlag.Despawning:
                            break;
                        case ThingChangeFlag.Despawned:
                            SetAvoidValue(args.Cell, 0);
                            break;
                        case ThingChangeFlag.StateChanged:
                            break;
                        case ThingChangeFlag.SentSignal:
                            break;
                    }
                }
            }
        }
    }
    
    public class PositionFilter
    {
        public List<TerrainDef>            terrainToAvoid = new ();
        public List<DefFloat<TerrainDef>> terrainToPrefer = new ();
        public List<ThingDef>             thingsToSpawnAt = new ();
        public List<ThingValue>          distanceToThings = new ();

        //public AreaCheck roofed   = AreaCheck.Avoid;
        //public AreaCheck homeArea = AreaCheck.Avoid;

        public IntVec3 FindCell(Map map)
        {
            return AllCells(map).RandomElement();
        }

        public IEnumerable<IntVec3> AllCells(Map map)
        {
            foreach (var cell in map.AllCells)
            {
                if(terrainToAvoid.Contains(cell.GetTerrain(map)))continue;
                if(terrainToPrefer.Any() && !terrainToPrefer.Any(ttp => TRandom.Chance(ttp.value))) continue;
                if(thingsToSpawnAt.Any() && !thingsToSpawnAt.Any(t => cell.GetFirstThing(map, t) != null)) continue;
                if(distanceToThings.Any() && distanceToThings.Any(t => map.listerThings.ThingsOfDef(t.ThingDef).Any(t2 => t2.Position.DistanceTo(cell) < t.value))) continue;
                yield return cell;
            }
        }

        public IEnumerable<IntVec3> AllCellsFitting(Map map, List<ThingDef> expectedThings)
        {
            var allCells = AllCells(map).ToList();
            foreach (var cell in allCells)
            {
                foreach (var thing in expectedThings)
                {
                    if (GenAdj.OccupiedRect(cell, Rot4.North, thing.size).Any(c => !allCells.Contains(c))) continue;
                    yield return cell;
                }
            }
        }

        public IEnumerable<IntVec3> NeededCellsFor(Map map, List<ThingDef> expectedThings)
        {
            var allCells = AllCells(map).ToList();
            var cells = new List<IntVec3>();
            foreach (var cell in allCells)
            {
                foreach (var thing in expectedThings)
                {
                    if (GenAdj.OccupiedRect(cell, Rot4.North, thing.size).Any(c => !allCells.Contains(c))) continue;
                    cells.Add(cell);
                }
            }

            for (int i = 0; i < expectedThings.Count; i++)
            {
                if (cells.TryRandomElement(out IntVec3 cell))
                    yield return cell;
            }
        }

        /*
        public List<IntVec3> FindCells(Map map, int needed, List<ThingValue> rewards = null, List<ThingDef> things = null)
        {
            List<IntVec3> spawnPositions = new List<IntVec3>();
            int minRoomSize = 0;
            if (!rewards.NullOrEmpty())
            {
                foreach (ThingValue tv in rewards)
                {
                    minRoomSize += (int)Math.Round(((double)tv.value / (double)tv.ThingDef.stackLimit), 0, MidpointRounding.AwayFromZero);
                }
            }
            List<IntVec3> AllCells = map.AllCells.Where(c => !c.Fogged(map) && c.Standable(map)).ToList();

            if (minRoomSize > 0f)
            {
                AllCells.RemoveAll(v => v.GetRoom(map)?.CellCount < minRoomSize);
            }
            if (!terrainToAvoid.NullOrEmpty())
            {
                AllCells.RemoveAll(v => terrainToAvoid.Contains(v.GetTerrain(map)));
            }
            if (!terrainToPrefer.NullOrEmpty())
            {
                AllCells.RemoveAll(v => !TRandom.Chance(terrainToPrefer.Find(t => t.terrainDef == v.GetTerrain(map)).weight));
            }
            if (!spawnAt.NullOrEmpty())
            {
                AllCells.RemoveAll(v => v.GetThingList(map)?.Any(t => !spawnAt.Contains(t.props)) ?? true);
            }
            if (!distanceToThings.NullOrEmpty())
            {
                AllCells.RemoveAll(v => distanceToThings.All(tv => map.listerThings.ThingsOfDef(tv.ThingDef).Any(t => v.DistanceTo(t.Position) < tv.value)));
            }
            if (homeArea == AreaCheck.Avoid)
            {
                AllCells.RemoveAll(v => map.areaManager.Home[v]);
            }
            if (homeArea == AreaCheck.Prefer)
            {
                AllCells.RemoveAll(v => !map.areaManager.Home[v]);
            }
            if (roofs == AreaCheck.Avoid)
            {
                AllCells.RemoveAll(v => v.Roofed(map));
            }
            if (roofs == AreaCheck.Prefer)
            {
                AllCells.RemoveAll(v => !v.Roofed(map));
            }
            if (!things.NullOrEmpty())
            {
                AllCells.RemoveAll(v => Enumerable.Any(things, t => !t.ThingFitsAt(map, v)));
            }
            int i = 0;
            int failsafe = 0;
            while (0 < count && failsafe < count * 10)
            {
                i++;
                failsafe++;
                AllCells.TryRandomElement(out IntVec3 cell);
                spawnPositions.Add(cell);
            }
            return spawnPositions;
        }
        */
    }

    public enum AreaCheck
    {
        Prefer,
        Avoid,
        Allow
    }
}
