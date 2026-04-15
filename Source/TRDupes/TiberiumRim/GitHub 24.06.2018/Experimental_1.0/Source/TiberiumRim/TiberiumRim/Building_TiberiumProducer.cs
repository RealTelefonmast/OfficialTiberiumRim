using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumProducerDef : ThingDef
    {
        public int growRadius;
        public int terrainRadius = 1;

        public IntRange spawnIntervalRange = new IntRange(100, 100);

        public bool bindsToCrystals = false;

        public bool needsToActivate = true;

        public bool spawnsTerrain = false;

        public List<TiberiumCrystalDef> crystalDefs;
    }

    public class Building_TiberiumProducer : Building
    {
        public new TiberiumProducerDef def;

        public HashSet<TiberiumCrystal> boundCrystals = new HashSet<TiberiumCrystal>();

        private MapComponent_TiberiumHandler mapComp;

        private int ticksUntilSpawn;

        private int ticksUntilWoken = -1;

        private float spawnWealth = 0;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = (TiberiumProducerDef)base.def;
            this.mapComp = this.Map.GetComponent<MapComponent_TiberiumHandler>();
            if (!respawningAfterLoad)
            {
                SetGrowthRadius();
                RemoveGrass();
                SpawnTerrain(CrystalDef);
                ResetCountdown();                        
                mapComp.ProducerCount += 1;
                if (this.def.needsToActivate)
                {
                    spawnWealth += this.Map.PlayerWealthForStoryteller;
                    this.ticksUntilWoken = GenDate.TicksPerDay * Mathf.RoundToInt(this.HPVar);
                    this.HitPoints = Mathf.RoundToInt(this.SpawnHitpoints);
                }
            }
        }

        public TiberiumCrystalDef CrystalDef
        {
            get
            {
                return this.def.crystalDefs.Where((TiberiumCrystalDef x) => x != null).RandomElement();
            }
        }

        public override void DeSpawn()
        {
            ResetGrowthRadius();
            mapComp.ProducerCount -= 1;
            base.DeSpawn();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<TiberiumCrystal>(ref boundCrystals, "boundCrystals", LookMode.Reference);
            Scribe_Values.Look<int>(ref ticksUntilSpawn, "ticksUntilSpawn");
            Scribe_Values.Look<int>(ref ticksUntilWoken, "ticksUntilWoken");
            Scribe_Values.Look<float>(ref spawnWealth, "spawnWealth");
        }

        public override void TickRare()
        {
            if (!IsResearchBound)
            {
                if (IsWoken)
                {
                    base.TickRare();
                    DestroyWalls();
                    ticksUntilSpawn -= 250;
                    if (ticksUntilSpawn <= 0)
                    {
                        this.TryDoSpawn();
                        this.ResetCountdown();
                    }
                }
                else
                {
                    this.ticksUntilWoken -= GenTicks.TickRareInterval;

                    float x = (float)this.MaxHitPoints;
                    float y = (float)GenDate.TicksPerDay;
                    float z = (float)GenTicks.TickRareInterval;

                    float num = ((x - SpawnHitpoints) / (y * 4f)) * z;
                    if (this.HitPoints < this.MaxHitPoints)
                    {
                        this.HitPoints += Mathf.RoundToInt(num);
                    }
                    else { this.HitPoints = this.MaxHitPoints; }
                }
            }
            else
            {
                if (boundCrystals.Count > 0)
                {
                    KillBoundCrystals();
                }
                foreach (IntVec3 v in this.OccupiedRect().ExpandedBy(2).EdgeCells.ToList<IntVec3>())
                {
                    if (v.InBounds(Map))
                        v.GetTiberium(Map)?.Destroy();
                }
            }
        }

        public float SpawnHitpoints
        {
            get
            {
                return this.MaxHitPoints / HPVar;
            }
        }

        public float HPVar
        {
            get
            {
                float num = (this.spawnWealth / 99999f);
                float num2 = (num > 1 ? 1 : num);
                return 7f - num2*6f;
            }
        }

        public bool IsWoken
        {
            get
            {
                return this.ticksUntilWoken <= 0;
            }
        }

        public bool IsResearchBound
        {
            get
            {
                Building Researcher = null;
                Researcher = (Building)this.Map.thingGrid.ThingAt(this.Position, ThingDef.Named("TiberiumResearchCrane_TBNS"));
                if(Researcher != null)
                {
                    return true;
                }
                return false;
            }
        }

        public void KillBoundCrystals()
        {
            foreach(TiberiumCrystal crystal in boundCrystals)
            {
                if(crystal != null && !crystal.Destroyed)
                {
                    crystal.Destroy(DestroyMode.KillFinalize);
                }
            }
        }

        public void SetGrowthRadius()
        {
            int num = GenRadial.NumCellsInRadius(this.def.growRadius);
            for (int i = 0; i < num; i++)
            {           
                IntVec3 positionToCheck = this.Position + GenRadial.RadialPattern[i];
                if (!mapComp.ForcedAllowedCells.Contains(positionToCheck))
                {
                    mapComp.ForcedAllowedCells.Add(positionToCheck);
                }
            }
        }

        public void ResetGrowthRadius()
        {
            int num = GenRadial.NumCellsInRadius(this.def.growRadius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 positionToCheck = this.Position + GenRadial.RadialPattern[i];
                if (mapComp.ForcedAllowedCells.Contains(positionToCheck))
                {
                    mapComp.ForcedAllowedCells.Remove(positionToCheck);
                }
            }
        }

        public void DestroyWalls()
        {
            var c = this.CellsAdjacent8WayAndInside();
            foreach (IntVec3 d in c)
            {
                if (d.InBounds(this.Map))
                {
                    var p = d.GetFirstBuilding(this.Map);

                    if (p != null)
                    {
                        int amt = 150;

                        DamageInfo damage = new DamageInfo(DamageDefOf.Mining, amt);

                        if (!p.def.defName.Contains("TBNS"))
                        {
                            p.TakeDamage(damage);
                        }
                    }
                }
            }
        }

        //TODO: Bigger radius needs more random terrain
        public virtual void SpawnTerrain(TiberiumCrystalDef crystalDef)
        {            
            if (!this.def.spawnsTerrain)
            {
                foreach (IntVec3 inside in this.CellsAdjacent8WayAndInside())
                {
                    if (inside.InBounds(this.Map))
                    {
                        GenTiberiumReproduction.SetTiberiumTerrainAndType(crystalDef, inside.GetTerrain(Map), out TiberiumCrystalDef def, out TerrainDef NewTerrain);
                        if (NewTerrain != null)
                        {
                            this.Map.terrainGrid.SetTerrain(inside, NewTerrain);
                        }
                    }
                }
                return;
            }
            int num = GenRadial.NumCellsInRadius(this.def.terrainRadius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = this.Position + GenRadial.RadialPattern[i];
                if (c.InBounds(Map))
                {
                    GenTiberiumReproduction.SetTiberiumTerrainAndType(crystalDef, c.GetTerrain(Map), out TiberiumCrystalDef def, out TerrainDef NewTerrain);
                    if (NewTerrain != null)
                    {
                        Plant plant = c.GetPlant(Map);
                        if(plant != null)
                        {
                            string name = plant.def.defName;
                            ThingDef flora = GenTiberiumReproduction.GetAnyPlant(name);
                            if (!plant.Destroyed)
                            {
                                plant.Destroy(DestroyMode.Vanish);
                            }
                            GenSpawn.Spawn(flora, c, Map);
                        }                    
                        this.Map.terrainGrid.SetTerrain(c, NewTerrain);
                    }
                }
            }
        }

        public void RemoveGrass()
        {
            foreach (IntVec3 inside in this.CellsAdjacent8WayAndInside())
            {
                if (inside.InBounds(this.Map))
                {
                    Plant plant = inside.GetPlant(this.Map);
                    if (plant != null)
                    {
                        plant.Destroy();
                    }
                }
            }
        }

        private void CheckShouldSpawn()
        {
            if (this.ticksUntilSpawn <= 0)
            {
                this.TryDoSpawn();
                this.ResetCountdown();
            }
        }

        public void TryDoSpawn()
        {
            foreach(IntVec3 c in GenAdj.CellsAdjacent8Way(this).InRandomOrder(null))
            {
                if(c.GetEdifice(this.Map) == null && c.GetTiberium(this.Map) == null)
                {
                    GenTiberiumReproduction.SetTiberiumTerrainAndType(CrystalDef, c.GetTerrain(Map), out TiberiumCrystalDef crystalDef, out TerrainDef tdef);
                    if (crystalDef != null)
                    {
                        TiberiumCrystal crystal = (TiberiumCrystal)ThingMaker.MakeThing(crystalDef, null);
                        if (crystal != null)
                        {
                            if (this.def.bindsToCrystals)
                            {
                                crystal.boundProducer = this;
                                this.boundCrystals.Add(crystal);
                            }
                            GenPlace.TryPlaceThing(crystal, c, this.Map, ThingPlaceMode.Direct);
                        }
                    }
                    return;
                }
            }
        }

        private void ResetCountdown()
        {
            this.ticksUntilSpawn = this.def.spawnIntervalRange.RandomInRange;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn " + CrystalDef.label,
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        this.TryDoSpawn();
                        this.ResetCountdown();
                    }
                };
            }
            yield break;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(base.GetInspectString());           
            stringBuilder.AppendLine(IsWoken ? "ProducerWoken".Translate() : "ProducerWokenIn".Translate() + ": " + this.ticksUntilWoken.ToStringTicksToDays());
            if (IsResearchBound)
            {
                stringBuilder.AppendLine("ResearchBound".Translate());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
