using System.Collections.Generic;
using System.Linq;
using Verse;
using System;
using UnityEngine;
using RimWorld;

namespace TiberiumRim
{
    public class MapComponent_TiberiumHandler : MapComponent_WaterHandler
    {
        public HashSet<IntVec3> AffectedTiles = new HashSet<IntVec3>();

        public HashSet<TiberiumCrystal> AllTiberiumCrystals = new HashSet<TiberiumCrystal>();

        public HashSet<IntVec3> InhibitedCells = new HashSet<IntVec3>();

        public HashSet<IntVec3> SuppressedCells = new HashSet<IntVec3>();

        public HashSet<IntVec3> ForcedAllowedCells = new HashSet<IntVec3>();

        public int ProducerCount = new int();

        //Tiberium Biome Checker Part

        public WorldComponent_TiberiumSpread worldComp = Find.World.GetComponent<WorldComponent_TiberiumSpread>();
        private int mapTiles;

        public MapComponent_TiberiumHandler(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            mapTiles = (this.map.Size.ToIntVec2.x * this.map.Size.ToIntVec2.z);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref this.ProducerCount, "producerAmt");
            Scribe_Collections.Look(ref this.ForcedAllowedCells, "ForcedAllowedCells", LookMode.Value);
            Scribe_Collections.Look(ref this.SuppressedCells, "SuppressedCells", LookMode.Value);
            Scribe_Collections.Look(ref this.InhibitedCells, "InhibitedCells", LookMode.Value);
            Scribe_Collections.Look(ref this.AffectedTiles, "AffectedTiles", LookMode.Value);
            Scribe_Collections.Look(ref this.AllTiberiumCrystals, "AllTiberiumCrystals", LookMode.Reference);
            base.ExposeData();
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                SetPct();
            }
            if (Find.TickManager.TicksGame % GenTicks.TickLongInterval == 0)
            {
                if (TiberiumExists)
                {
                    AffectTile(AffectedTiles);
                }
            }
            if (DebugSettings.fastEcology)
            {
                HashSet<TiberiumCrystal> newHash = new HashSet<TiberiumCrystal>();
                newHash.AddRange(AllTiberiumCrystals);
                foreach(TiberiumCrystal crystal in newHash)
                {
                    crystal.TickLong();
                }
            }            
            base.MapComponentTick();
        }

        // -- Biome Checker --

        private int MapTile
        {
            get
            {
                return this.map.Tile;
            }
        }

        public float TileCount
        {
            get
            {
                int tilesCovered = 0;
                List<Thing> thingList = this.map.listerThings.AllThings.Where((Thing x) => x.def.coversFloor == true).ToList();
                foreach(Thing t in thingList)
                {
                    tilesCovered += t.OccupiedRect().Cells.Count();
                }
                return (mapTiles - tilesCovered);
            }
        }

        public void SetPct()
        {
            if (!worldComp.TiberiumTiles.ContainsKey(MapTile))
            {
                worldComp.TiberiumTiles.Add(MapTile, ((float)AllTiberiumCrystals.Count / TileCount));
            }
            else
            {
                worldComp.TiberiumTiles[MapTile] = ((float)AllTiberiumCrystals.Count / TileCount);
            }

        }

        // -- Bools --

        public bool TiberiumExists => AllTiberiumCrystals.Count > 0;

        public bool HarvestableTiberiumExists => AllTiberiumCrystals.Where((TiberiumCrystal x) => x.Harvestable).Count() > 0;

        public int MaxMonoliths => (map.Size.x * map.Size.y) / MainTCD.MainTiberiumControlDef.cellsPerMonolith;

        private bool CanSpawnMonolith(TiberiumCrystal parent)
        {
            if (map.listerThings.AllThings.FindAll((Thing x) => x.def.thingClass == typeof(Building_Monolith)).Count < MaxMonoliths)
            {
                if (parent.CanGrowToMonolith)
                {
                    int num = 0;
                    foreach (IntVec3 intVec in GenAdjFast.AdjacentCells8Way(parent.Position, parent.Rotation, parent.RotatedSize))
                    {
                        if (intVec.InBounds(map))
                        {
                            if (intVec.GetTiberium(map) != null && intVec.GetTiberium(map).CanGrowToMonolith)
                            {
                                num++;
                            }
                        }
                    }
                    if (num == 8)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void MonolithRise(Map map, ThingDef tower, TiberiumCrystal parent)
        {
            if (tower != null)
            {
                IntVec3 loc = parent.Position;
                GenSpawn.Spawn(tower, loc, map);
            }
        }

        public static List<TiberiumCrystal> Sources(IntVec3 c, Map map, bool returnOne = false)
        {
            List<TiberiumCrystal> Sources = new List<TiberiumCrystal>();
            foreach (IntVec3 v in GenAdjFast.AdjacentCells8Way(c))
            {
                if (v.InBounds(map))
                {
                    TiberiumCrystal source = v.GetTiberium(map);
                    if (source != null)
                    {
                        if (returnOne)
                        {
                            Sources.Add(source);
                            return Sources;
                        }
                        else
                        {
                            Sources.Add(source);
                        }
                    }
                }
            }
            return Sources;
        }

        public void AffectTile(HashSet<IntVec3> affectedTiles)
        {
            HashSet<IntVec3> tempHashSet = new HashSet<IntVec3>();
            tempHashSet.AddRange(affectedTiles);
            bool spawnedMonolith = false;

            foreach(IntVec3 affectedTile in tempHashSet)
            {
                if(affectedTiles.Contains(affectedTile))
                {
                    if (affectedTile.InBounds(map))
                    {
                        TiberiumCrystal parent = Sources(affectedTile, map, true).GetOneThing();
                        if(parent != null)
                        {
                            TiberiumCrystalDef def = parent.def;
                            DamageInfo damageEntity = new DamageInfo(DamageDefOf.Deterioration, (int)(def.tiberium.entityDamage.RandomInRange * TiberiumRimSettings.settings.ItemDamageMltp));
                            DamageInfo damageBuilding = new DamageInfo(DamageDefOf.Deterioration, (int)(def.tiberium.buildingDamage.RandomInRange * TiberiumRimSettings.settings.BuildingDamageMltp));
                            if(Rand.Chance(0.005f) && Find.TickManager.TicksGame % GenDate.TicksPerDay == 0 && def.monolithDef != null && !spawnedMonolith && CanSpawnMonolith(parent))
                            {
                                MonolithRise(map, def.monolithDef, parent);
                                spawnedMonolith = true;
                            }
                            List<Thing> thingList = new List<Thing>();
                            thingList.AddRange(affectedTile.GetThingList(this.map).Where(x => x.def.EverHaulable || x.def.thingClass.IsSubclassOf(typeof(Building)) || x.def.thingClass == typeof(Building) || x is Pawn));
                            for (int i = 0; i < thingList.Count; i++)
                            {
                                Thing thing = thingList[i];
                                if(thing != null && !thing.Destroyed && thing?.def.thingClass != def.thingClass && !parent.def.friendlyTo.Contains(thing.def))
                                {
                                    if(thing is Pawn)
                                    {
                                        if (thing as Pawn != null)
                                        {
                                            if(TiberiumRimSettings.settings.PawnDamage)
                                            TRUtils.AffectPawn(thing as Pawn, def, parent, map);
                                        }
                                    }
                                    else if (thing.def.EverHaulable && def.tiberium.entityDamage.min > 0)
                                    {
                                        if(TiberiumRimSettings.settings.EntityDamage)
                                        DamageEntities(thing, affectedTile, damageEntity, parent, def);
                                    }
                                    else if(def.tiberium.buildingDamage.min > 0)
                                    {
                                        if(TiberiumRimSettings.settings.BuildingDamage)
                                        DamageBuildings(thing, affectedTile, damageBuilding, parent, def);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DamageEntities(Thing thing, IntVec3 c, DamageInfo damage, TiberiumCrystal crystal, TiberiumCrystalDef def)
        {
            if (thing.def.thingClass != typeof(TiberiumChunk))
            {
                thing.TakeDamage(damage);
                if(thing != null && thing.def.thingCategories.Contains(ThingCategoryDef.Named("StoneChunks")))
                {
                    DamageOrCorruptWallsOrChunks(thing, damage, crystal, def, true);
                }
            }
            if (thing.def.IsCorpse)
            {
                Corpse body = (Corpse)thing;
                if (Rand.Chance(0.75f) && !body.InnerPawn.def.defName.Contains("_TBI") && !body.InnerPawn.RaceProps.IsMechanoid)
                {
                    if (body.AnythingToStrip())
                    {
                        body.Strip();
                    }
                    SpawnFiendOrVisceroid(c, body.InnerPawn);
                    thing.Destroy(DestroyMode.Vanish);
                    return;
                }
            }
        }

        public void DamageOrCorruptWallsOrChunks(Thing thing, DamageInfo dinfo, TiberiumCrystal parent, TiberiumCrystalDef def, bool thingIsChunk = false)
        {
            ThingDef RockOrChunk = null;
            IntVec3 loc = thing.Position;
            thing.TakeDamage(dinfo);
            if (thing != null && !thing.Destroyed && Rand.Chance(0.05f))
            {
                if (!thingIsChunk)
                {
                    if (thing.def.mineable)
                    {
                        if (def.corruptedWallDef != null)
                        {
                            RockOrChunk = def.corruptedWallDef;
                            goto Corrupt;
                        }
                        return;
                    }
                    return;
                }
                else
                {
                    if (def.corruptedChunkDef != null)
                    {
                        RockOrChunk = def.corruptedChunkDef;
                        goto Corrupt;
                    }
                    return;
                }
                Corrupt:
                if (loc.InBounds(map))
                {
                    thing.Destroy(DestroyMode.Vanish);
                    GenSpawn.Spawn(RockOrChunk, loc, map);
                    map.terrainGrid.SetTerrain(loc, def.defaultTerrain);
                }
            }
        }

        public virtual void SpawnFiendOrVisceroid(IntVec3 pos, Pawn p)
        {
            Pawn pawn = null;

            if (Rand.Chance(0.25f))
            {
                PawnKindDef creature = null;
                switch (p.def.defName)
                {
                    case "Thrumbo":
                        if (Rand.Chance(0.2f))
                        {
                            creature = PawnKindDef.Named("TiberiumTerror_TBI");
                            break;
                        }
                        creature = PawnKindDef.Named("Thrimbo_TBI");
                        break;

                    case "GrizzlyBear":
                        creature = PawnKindDef.Named("BigTiberiumFiend_TBI");
                        break;

                    case "PolarBear":
                        creature = PawnKindDef.Named("BigTiberiumFiend_TBI");
                        break;

                    case "Megascarab":
                        creature = PawnKindDef.Named("Tibscarab_TBI");
                        break;

                    case "WolfTimber":
                        creature = PawnKindDef.Named("TiberiumFiend_TBI");
                        break;

                    case "WolfArctic":
                        creature = PawnKindDef.Named("TiberiumFiend_TBI");
                        break;

                    case "FoxFennec":
                        creature = PawnKindDef.Named("SmallTiberiumFiend_TBI");
                        break;

                    case "FoxRed":
                        creature = PawnKindDef.Named("SmallTiberiumFiend_TBI");
                        break;

                    case "FoxArctic":
                        creature = PawnKindDef.Named("SmallTiberiumFiend_TBI");
                        break;

                    case "Cobra":
                        creature = PawnKindDef.Named("Crawler_TBI");
                        break;

                    case "Boomrat":
                        creature = PawnKindDef.Named("Boomfiend_TBI");
                        break;

                    case "Muffalo":
                        creature = PawnKindDef.Named("Tiffalo_TBI");
                        break;

                    case "Warg":
                        creature = PawnKindDef.Named("Spiner_TBI");
                        break;

                    default:
                        creature = PawnKindDef.Named("Visceroid_TBI");
                        break;
                }
                PawnGenerationRequest request = new PawnGenerationRequest(creature);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            else
            {
                Visceroid visceroid = null;
                PawnKindDef Visceroid = PawnKindDef.Named("Visceroid_TBI");
                PawnGenerationRequest request = new PawnGenerationRequest(Visceroid);
                visceroid = (Visceroid)PawnGenerator.GeneratePawn(request);
                if (p.ageTracker != null)
                {
                    visceroid.ageTracker.AgeBiologicalTicks = p.ageTracker.AgeBiologicalTicks;
                    visceroid.ageTracker.AgeChronologicalTicks = p.ageTracker.AgeBiologicalTicks;
                }
                else
                {
                    visceroid.ageTracker.AgeBiologicalTicks = 0;
                }

                visceroid.kindDefName = p.kindDef.label;

                if (p.Name?.ToString()?.Count() > 0)
                {
                    visceroid.oldName = p.Name.ToString();
                    visceroid.wasNamed = true;
                }                             

                GenSpawn.Spawn(visceroid, pos, map, p.Rotation);
                if (p.Faction == Faction.OfPlayer)
                {
                    Messages.Message("PawnConverted".Translate(new object[] { p.Name.ToString() }), MessageTypeDefOf.PawnDeath);
                }
                return;
            }
            GenSpawn.Spawn(pawn, pos, map, p.Rotation);
        }

        public void DamageBuildings(Thing thing, IntVec3 c, DamageInfo damage, TiberiumCrystal parent, TiberiumCrystalDef def)
        {
            if (!thing.def.defName.Contains("TBNS"))
            {
                DamageOrCorruptWallsOrChunks(thing, damage, parent, def);
            }

            if (thing.def == ThingDefOf.Grave)
            {
                thing.Destroy(DestroyMode.Deconstruct);
            }

            if (thing.def == ThingDefOf.SteamGeyser)
            {
                ThingDef tibG = ThingDef.Named("TiberiumGeyser");
                IntVec3 loc = thing.Position;
                thing.DeSpawn();
                GenSpawn.Spawn(tibG, loc, map);
            }
        }
    }
}
