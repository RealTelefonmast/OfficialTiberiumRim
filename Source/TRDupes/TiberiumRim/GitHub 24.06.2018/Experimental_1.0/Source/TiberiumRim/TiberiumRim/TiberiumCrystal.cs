using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TiberiumCrystal : ThingWithComps
    {
        public new TiberiumCrystalDef def;

        private MapComponent_TiberiumHandler mapComp;

        public Building_TiberiumProducer boundProducer = null;

        //Properties
        public float MinGrowthTemperature = MainTCD.MainTiberiumControlDef.TiberiumMinTemp;

        private const float GridPosRandomnessFactor = 0.4f;

        private static readonly FloatRange DyingDamagePerTickBecauseInhibited = new FloatRange(0.0001f, 0.001f);

        public const float BaseGrowthPercent = 0.05f;

        private static Color32[] workingColors = new Color32[4];

        protected int ageInt;

        public float growthInt = 0.001f;

        public int harvestTicks;

        //Crystal Properties

        public TerrainDef corruptsTo = null;

        public List<IntVec3> affectedTiles = new List<IntVec3>();

        private int bledTimes = 0;

        // SpawnSetup --
        // Setting up the MapComp and adding Tiberium to its list.
        // Also adding the tiles which get affected by the mapComp - tiberium itself is just the messenger inbetween.
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.mapComp = this.Map.GetComponent<MapComponent_TiberiumHandler>();                      
            if (!respawningAfterLoad)
            {
                this.def = (TiberiumCrystalDef)base.def;               
                if (!mapComp.AllTiberiumCrystals.Contains(this))
                {
                    mapComp.AllTiberiumCrystals.Add(this);
                }
                SetTiles();
                this.harvestTicks = GenTicks.SecondsToTicks(def.tiberium.harvestTimeSec);
            }            
        }

        // On Despawn() this crystal needs to be removed from the mapcomp, aswell as the affected tiles.
        public override void DeSpawn()
        {
            RemoveTiles();
            this.Map.GetComponent<MapComponent_TiberiumHandler>().AllTiberiumCrystals.Remove(this);
            base.DeSpawn();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.growthInt, "growth", 0f, false);
            Scribe_Values.Look<int>(ref this.ageInt, "age", 0, false);
            Scribe_Values.Look<int>(ref harvestTicks, "harvestTicks");
            Scribe_Defs.Look<TiberiumCrystalDef>(ref this.def, "def");
        }

        public override void TickLong()
        {
            if (base.Destroyed)
            {
                return;
            }
            if (def.tiberium.isFlesh)
            {
                if (this.HitPoints < this.MaxHitPoints)
                {
                    if (this.bledTimes < 6)
                    {
                        bledTimes++;
                    }
                }
            }

            Plant plant = this.Position.GetPlant(Map);
            if(plant != null)
            {
                if (!def.friendlyTo.Contains(plant.def))
                {
                    if (Rand.Chance(0.06f))
                    {
                        ThingDef plantdef = GenTiberiumReproduction.GetAnyPlant(plant.def.defName);
                        GenSpawn.Spawn(plantdef, plant.Position, Map);
                        plant.DeSpawn();
                    }
                    plant.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, Rand.Range(8, 26)));
                }
            }

            if(this.LifeStage == TiberiumLifeStage.ProducesTerrain)
            {
                TerrainDef terrain = this.Position.GetTerrain(Map);
                if(!(terrain is TiberiumTerrainDef))
                {
                    GenTiberiumReproduction.SetTiberiumTerrainAndType(this.def, terrain, out TiberiumCrystalDef crystalDef, out TerrainDef tDef);
                    if (tDef != null)
                    {
                        Map.terrainGrid.SetTerrain(this.Position, tDef);
                    }
                }
            }

            float growthInt = this.growthInt;
            bool mature = this.LifeStage == TiberiumLifeStage.Mature;
            this.growthInt += this.GrowthPerTick * 2000f;
            if (this.growthInt > 1f)
            {
                this.growthInt = 1f;
            }
            if (((!mature && this.LifeStage == TiberiumLifeStage.Mature) || (int)(growthInt * 10f) != (int)(this.growthInt * 10f)))
            {
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
            }
            if (this.CanReproduceNow && Rand.MTBEventOccurs(this.def.tiberium.reproduceMtbDays, 60000f, 2000f))
            {
                GenTiberiumReproduction.TryReproduceFrom(base.Position, this, SeedTargFindMode.Reproduce, base.Map, mapComp, this.def.friendlyTo);
            }
            this.ageInt += 2000;
            if (this.Dying)
            {
                Map map = base.Map;
                int amount = Mathf.CeilToInt(this.CurrentDyingDamagePerTick * 2000f);
                base.TakeDamage(new DamageInfo(DamageDefOf.Rotting, amount, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            }
        }

        //
        public TiberiumLifeStage LifeStage
        {
            get
            {
                if(this.growthInt > 0.33f)
                {
                    return TiberiumLifeStage.ProducesTerrain;
                }
                if (this.growthInt > 0.77f)
                {
                    return TiberiumLifeStage.Mature;
                }
                if (this.growthInt > 0.999f)
                {
                    return TiberiumLifeStage.Evolving;
                }
                return TiberiumLifeStage.Growing;
            }
        }      

        public void SetTiles()
        {
            for (int i = 0; i < GenAdjFast.AdjacentCells8Way(this.Position).Count; i++)
            {
                IntVec3 v = GenAdjFast.AdjacentCells8Way(this.Position)[i];
                if (v.InBounds(this.Map))
                {
                    if (!mapComp.AffectedTiles.Contains(v))
                    {
                        mapComp.AffectedTiles.Add(v);
                    }
                }
                this.affectedTiles.Add(v);
            }
        }

        public void RemoveTiles()
        {
            HashSet<IntVec3> sourceAffectedTiles = new HashSet<IntVec3>();
            for (int i = 0; i < this.affectedTiles.Count; i++)
            {
                IntVec3 v = this.affectedTiles[i];
                if (v.InBounds(this.Map))
                {
                    List<TiberiumCrystal> sources = MapComponent_TiberiumHandler.Sources(v, this.Map);
                    for (int ii = 0; ii < sources.Count; ii++)
                    {
                        TiberiumCrystal source2 = sources[ii];
                        if (source2 != null)
                        {
                            if (source2.affectedTiles.Contains(v))
                            {
                                sourceAffectedTiles.Add(v);
                            }
                        }
                    }
                    if (mapComp.AffectedTiles.Contains(v) && !sourceAffectedTiles.Contains(v))
                    {
                        mapComp.AffectedTiles.Remove(v);
                    }
                }
            }
        }

        // Functions

        public override void Print(SectionLayer layer)
        {
            Vector3 a = this.TrueCenter();
            Rand.PushState();
            Rand.Seed = base.Position.GetHashCode();
            int num = Mathf.CeilToInt(this.growthInt * (float)this.def.tiberium.maxMeshCount);
            if (num < 1)
            {
                num = 1;
            }
            float num2 = this.def.tiberium.visualSizeRange.LerpThroughRange(this.growthInt);
            float num3 = this.def.graphicData.drawSize.x * num2;
            Vector3 vector = Vector3.zero;
            int num4 = 0;
            int[] positionIndices = TiberiumPosIndices.GetPositionIndices(this);
            bool flag = false;
            foreach (int num5 in positionIndices)
            {
                if (this.def.tiberium.maxMeshCount == 1)
                {
                    vector = a + Gen.RandomHorizontalVector(0.05f);
                    float num6 = (float)base.Position.z;
                    if (vector.z - num2 / 2f < num6)
                    {
                        vector.z = num6 + num2 / 2f;
                        flag = true;
                    }
                }
                else
                {
                    int num7 = 1;
                    int maxMeshCount = this.def.tiberium.maxMeshCount;
                    switch (maxMeshCount)
                    {
                        case 1:
                            num7 = 1;
                            break;
                        default:
                            if (maxMeshCount != 9)
                            {
                                if (maxMeshCount != 16)
                                {
                                    if (maxMeshCount != 25)
                                    {
                                        Log.Error(this.def + " must have tiberium.MaxMeshCount that is a perfect square.");
                                    }
                                    else
                                    {
                                        num7 = 5;
                                    }
                                }
                                else
                                {
                                    num7 = 4;
                                }
                            }
                            else
                            {
                                num7 = 3;
                            }
                            break;
                        case 4:
                            num7 = 2;
                            break;
                    }
                    float num8 = 1f / (float)num7;
                    vector = base.Position.ToVector3();
                    vector.y = this.def.Altitude;
                    vector.x += 0.5f * num8;
                    vector.z += 0.5f * num8;
                    int num9 = num5 / num7;
                    int num10 = num5 % num7;
                    vector.x += (float)num9 * num8;
                    vector.z += (float)num10 * num8;
                    float max = num8 * 0.3f;
                    vector += Gen.RandomHorizontalVector(max);
                }
                bool @bool = Rand.Bool;
                Material matSingle = this.Graphic.MatSingle;
                GenTiberium.SetWindExposureColors(TiberiumCrystal.workingColors, this);
                Vector2 vector2 = new Vector2(num3, num3);
                Vector3 center = vector;
                Vector2 size = vector2;
                Material mat = matSingle;
                bool flipUv = @bool;
                Printer_Plane.PrintPlane(layer, center, size, mat, 0f, flipUv, null, TiberiumCrystal.workingColors, 0.1f);
                num4++;
                if (num4 >= num)
                {
                    break;
                }
            }
            if (this.def.graphicData.shadowData != null)
            {
                Vector3 center2 = a + this.def.graphicData.shadowData.offset * num2;
                if (flag)
                {
                    center2.z = base.Position.ToVector3Shifted().z + this.def.graphicData.shadowData.offset.z;
                }
                center2.y -= 0.046875f;
                Vector3 volume = def.graphicData.shadowData.volume * num2;
                Printer_Shadow.PrintShadow(layer, center2, volume, Rot4.North);
            }
            Rand.PopState();
        }

        // Bools
        public bool Harvestable
        {
            get
            {
                return def.tiberium.maxHarvestValue > 0 && def.HarvestableType;
            }
        }

        public virtual bool CanReproduceNow
        {
            get
            {
                return base.Spawned && growthInt >= 0.6f;
            }
        }

        public bool Dying
        {
            get
            {
                return this.CurrentDyingDamagePerTick > 0f;
            }
        }

        public bool DyingBecauseExposedToInhibition
        {
            get
            {
                return def.tiberium.canBeInhibited && Spawned && mapComp.InhibitedCells.Contains(Position);
            }
        }

        public bool Resting
        {
            get
            {
                return GenLocalDate.DayPercent(this) < 0.2f;
            }
        }

        public bool CanGrowToMonolith
        {
            get
            {
                return this.HarvestValue >= this.def.tiberium.maxHarvestValue;
            }
        }

        // Values

        public virtual float CurrentDyingDamagePerTick
        {
            get
            {
                float num = 0f;
                if (this.DyingBecauseExposedToInhibition)
                {
                    num = Mathf.Max(num, DyingDamagePerTickBecauseInhibited.LerpThroughRange(0.1f));
                }
                return num;
            }
        }

        public float Growth
        {
            get
            {
                return this.growthInt;
            }
            set
            {
                this.growthInt = Mathf.Clamp01(value);
            }
        }

        public float HarvestValue
        {
            get
            {
                return growthInt * this.def.tiberium.maxHarvestValue;
            }
        }

        protected float GrowthPerTick
        {
            get
            {
                if (this.growthInt >= 1f || this.Resting)
                {
                    return 0f;
                }
                float num = 1f / (60000f * this.def.tiberium.growDays);
                return num * this.GrowthRate;
            }
        }

        public virtual float GrowthRate
        {
            get
            {
                return TiberiumRimSettings.settings.GrowthRate;
            }
        }

        /*
        public float GrowthRateFactor_Fertility
        {
            get
            {
                return base.Map.fertilityGrid.FertilityAt(base.Position) * thisDef.tiberium.fertilitySensitivity + (1f - thisDef.tiberium.fertilitySensitivity);
            }
        }
        */

        public float GrowthRateFactor_Temperature
        {
            get
            {
                float num;
                if (!GenTemperature.TryGetTemperatureForCell(base.Position, base.Map, out num))
                {
                    return 1f;
                }
                if (num < 10f)
                {
                    return Mathf.InverseLerp(0f, 10f, num);
                }
                if (num > 42f)
                {
                    return Mathf.InverseLerp(58f, 42f, num);
                }
                return 1f;
            }
        }

        public int Age
        {
            get
            {
                return this.ageInt;
            }
            set
            {
                this.ageInt = value;
            }
        }

        protected string GrowthPercentString
        {
            get
            {
                return (this.growthInt + 0.0001f).ToStringPercent();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("PercentGrowth".Translate(new object[]
            {
                    this.GrowthPercentString
            }));

            if (this.Resting)
            {
                stringBuilder.AppendLine("PlantResting".Translate());
            }
            else if (this.LifeStage == TiberiumLifeStage.Mature)
            {
                if (this.Harvestable)
                {
                    stringBuilder.AppendLine("HarvestReady".Translate());
                }
            }
            if (this.Harvestable)
            {
                stringBuilder.AppendLine("HarvestValue".Translate() + ": " + Mathf.RoundToInt(HarvestValue));
            }
            if (DyingBecauseExposedToInhibition)
            {
                stringBuilder.AppendLine("DyingToInhibition".Translate());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}