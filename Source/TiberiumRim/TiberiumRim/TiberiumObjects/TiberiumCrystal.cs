using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum TiberiumLifeStage
    {
        Growing,
        ProducingTerrain,
        Mature,
        Evolving
    }

    public class TiberiumCrystal : FXThing
    {
        public new TiberiumCrystalDef def;
        public int generation = 0;

        private TiberiumProducer parent;
        private float growthInt = 0.001f;
        private float growthPerTick = 0;
        private float inRange = -1;
        private bool forcedDormant = true;
        private bool updatedTerrain = false;

        private int meshDirtyTicks = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref parent, "parent");
            Scribe_Values.Look(ref growthInt, "growth");
            Scribe_Values.Look(ref growthPerTick, "growthTick");
            Scribe_Values.Look(ref forcedDormant, "dormant");
        }

        public void PreSpawnSetup(TiberiumProducer parent, bool dormant = true, int gen = 0)
        {
            this.forcedDormant = dormant;
            this.parent = parent;
            this.generation = gen;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.def = base.def as TiberiumCrystalDef;
            Manager.AddTiberium(this, respawningAfterLoad);
            if (HasParent)
            {
                parent.AddBoundCrystal(this);
                if (!respawningAfterLoad)
                {
                    growthPerTick = 1f / ((GenDate.TicksPerDay * def.tiberium.growDays) / GenTicks.TickLongInterval);
                    inRange = TRUtils.Range(parent.def.spawner.spreadRange);
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Manager.RemoveTiberium(this);
            if (def.dead != null)
                Map.terrainGrid.SetTerrain(Position, def.dead);
            if (HasParent)
                parent.RemoveBoundCrystal(this);
            base.DeSpawn(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void TickLong()
        {
            if (CanTick)
            {
                TiberiumTick();
            }
            else if(def.tiberium.dependsOnProducer)
            {
                var info = new DamageInfo(DamageDefOf.Rotting, TRUtils.Range(1, 15));
                this.TakeDamage(info);
            }
        }

        public void TiberiumTick()
        {
            if ((!parent?.NoSpread ?? true) && !Dormant)
            {
                if (Growth > 0.75f)
                {
                    if (Rand.MTBEventOccurs(def.tiberium.reproduceDays, GenDate.TicksPerDay, GenTicks.TickLongInterval))
                    {
                        GenTiberium.SpreadTiberium(this);
                    }
                }
            }
            if ((!parent?.NoTerrain ?? true) && !updatedTerrain && LifeStage == TiberiumLifeStage.ProducingTerrain)
            {
                TerrainDef terrain = Position.GetTerrain(Map);
                if (!(terrain is TiberiumTerrainDef))
                {
                    if (GenTiberium.AnyCorruptedOutcomes(def, terrain, out TerrainSupport support))
                    {
                        Map.terrainGrid.SetTerrain(this.Position, support.TerrainOutcome);
                    }
                }
                updatedTerrain = true;
            }
            if ((!parent?.NoGrowth ?? true) && Growth < 1f)
            {
                if (!Suppressed)
                {
                    Growth += GrowthPerTick;
                    if ((!parent?.NoReprint ?? true) && meshDirtyTicks == 0)
                    {
                        Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                        meshDirtyTicks = 50;
                    }
                    meshDirtyTicks--;
                }
            }
        }

        public bool CanBeHarvestedBy(Harvester harvester)
        {
            if (def.HarvestType == HarvestType.Unharvestable)
                return false;

            switch (harvester.harvestMode)
            {
                case HarvestMode.Nearest:
                    return !def.IsMoss;
                case HarvestMode.Value:
                    return def == Manager.TiberiumInfo.MostValuableType;
                case HarvestMode.Moss:
                    return def.IsMoss;
            }
            return false;
        }

        public MapComponent_Tiberium Manager
        {
            get
            {
                return Map.GetComponent<MapComponent_Tiberium>();
            }
        }

        public bool CanTick => (HasParent && !Parent.stopGrowth);
        public bool ShouldGrow => LifeStage < TiberiumLifeStage.Mature;
        public bool ShouldSpread => Manager.TiberiumInfo.TiberiumGrid.growBools[Position];
        public bool HasParent => !parent.DestroyedOrNull();
        public bool Suppressed =>  Manager.Suppression.IsInSuppressorField(Position);
        public bool HarvestableNow => LifeStage != TiberiumLifeStage.Growing && def.tiberium.harvestValue > 0f;
        public TiberiumProducer Parent => parent;

        public bool Dormant
        {
            get => forcedDormant || parent.DestroyedOrNull() || !ShouldSpread || Suppressed || !InGrowRange;
            set => forcedDormant = value;
        }

        public bool InGrowRange
        {
            get
            {
                if (HasParent && inRange > 0)
                {
                    return Position.DistanceTo(parent.Position) <= inRange;
                }
                return true;
            }
        }

        public TiberiumLifeStage LifeStage
        {
            get
            {
                if (Growth > 0.99f)
                {
                    return TiberiumLifeStage.Evolving;
                }
                if (Growth > 0.66f)
                {
                    return TiberiumLifeStage.Mature;
                }
                if (Growth > 0.26f)
                {
                    return TiberiumLifeStage.ProducingTerrain;
                }
                return TiberiumLifeStage.Growing;
            }
        }

        public float HarvestValue => (Growth * def.tiberium.harvestValue);

        public int HarvestTime
        {
            get
            {
                float val = ((float)GenTicks.SecondsToTicks(def.tiberium.harvestTime)) * Growth;
                return (int)Math.Round(val, 0, MidpointRounding.AwayFromZero);
            }
        }

        public float Growth
        {
            get
            {
                return growthInt;
            }
            set
            {
                growthInt = Mathf.Clamp01(value);
            }
        }

        protected float GrowthPerTick => growthPerTick * this.GrowthRate;

        public virtual float GrowthRate
        {
            get
            {
                float rate = 1f;
                rate *= TiberiumRimSettings.settings.GrowthRate;
                //rate *= Mathf.InverseLerp(def.tiberium.minTemperature, 2000, Map.mapTemperature.OutdoorTemp);
                return rate;
            }
        }

        public override void Draw()
        {
            return;
        }

        public override void Print(SectionLayer layer)
        {
            Vector3 a = this.TrueCenter();
            Rand.PushState();
            Rand.Seed = base.Position.GetHashCode();
            int num = Mathf.CeilToInt(this.Growth * (float)this.def.tiberium.MeshCount);
            if (num < 1)
            {
                num = 1;
            }
            float num2 = this.def.tiberium.sizeRange.LerpThroughRange(this.Growth);
            float num3 = this.def.graphicData.drawSize.x * num2;
            Vector3 vector = Vector3.zero;
            int num4 = 0;
            int[] positionIndices = TiberiumPosIndices.GetPositionIndices(this);
            bool flag = false;
            foreach (int num5 in positionIndices)
            {
                if (this.def.tiberium.MeshCount == 1)
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
                    int maxMeshCount = this.def.tiberium.MeshCount;
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
                                        Log.Error(this.def + " must have MaxMeshCount that is a perfect square.", false);
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
                Vector2 vector2 = new Vector2(num3, num3);
                Vector3 center = vector;
                Vector2 size = vector2;
                Material mat = matSingle;
                bool flipUv = @bool;
                Printer_Plane.PrintPlane(layer, center, size, mat, 0f, flipUv, null, new Color32[4], 0.1f, (float)(this.HashOffset() % 1024));
                num4++;
                if (num4 >= num)
                {
                    break;
                }
            }
            Rand.PopState();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            if (Dormant)
            {
                sb.AppendLine("DormantTiberium".Translate());
            }
            sb.AppendLine("PercentGrowth".Translate(Growth.ToStringPercent()));
            if (Growth < 1f)
            {
                sb.AppendLine("Growth".Translate() + ": " + GrowthRate.ToStringPercent());
            }
            if (HarvestableNow)
            {
                sb.AppendLine("HarvestValue".Translate() + ": " + Mathf.RoundToInt(HarvestValue));
            }
            if (DebugSettings.godMode)
            {
                sb.AppendLine("Exact Value: " + HarvestValue);
                sb.AppendLine("Tiberium Type: " + def.HarvestType);
                sb.AppendLine("Generation: " + generation);
            }
            /*
            if (DyingInhibtion)
            {
                //stringBuilder.AppendLine("DyingToInhibition".Translate());
            }
            */
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos();
        }
    }

    /*
    public class TiberiumCrystal2 : ThingWithEffects
    {
        public new TiberiumCrystalDef def;
        private TiberiumProducer parent;

        public int generation = 0;
        public float growth = 0.001f;
        private float evolution = 0f;
        private float maxGrowDistance = -1;
        private int maxFriends = -1;
        private bool updatedTerrain = false;

        private bool parentLost = false;
        public bool dormant = true;
        private bool printedOnce = false;
        private float lastGrowth = 0;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref parent, "parent");
            Scribe_Values.Look(ref dormant, "dormant");
            Scribe_Values.Look(ref growth, "growth");
            Scribe_Values.Look(ref evolution, "evolution");
            Scribe_Values.Look(ref parentLost, "parentLost");
            Scribe_Values.Look(ref updatedTerrain, "updatedTerrain");
            Scribe_Values.Look(ref generation, "generation");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = base.def as TiberiumCrystalDef;
            Manager.AddTiberium(this, respawningAfterLoad);
            
            if (!parent.DestroyedOrNull())
            {
                parent.AddBoundCrystal(this);
                if (!respawningAfterLoad)
                {
                    maxGrowDistance = TRUtils.Range(parent.def.spreadDistance);
                    maxFriends = TRUtils.Range(parent.def.spreadAmount);
                }
            }
        }

        public void Activate(TiberiumProducer producer, int generation = 0)
        {
            this.parent = producer;
            this.dormant = false;
            this.generation = generation + 1;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < 3; i++)
            {
                if (TRUtils.Chance(0.234f))
                {
                    ThingDef filth = DefDatabase<ThingDef>.GetNamed("FilthTiberium" + def.TiberiumValueType.ToString(), false);
                    if (filth != null)
                    {
                        FilthMaker.MakeFilth(Position, Map, filth);
                    }
                }
            }
            base.Destroy(mode);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Manager.RemoveTiberium(this);
            if (def.dead != null)
            {
                Map.terrainGrid.SetTerrain(Position, def.dead);
            }
            if (!parent.DestroyedOrNull())
            {
                parent.RemoveBoundCrystal(this);
            }
            base.DeSpawn(mode);
        }

        public override void TickLong()
        {
            if (!Dormant && (!Parent?.stopGrowth ?? true))
            {
                if (Growth > 0.75f && !GrowthLocked)
                {                   
                    if (Rand.MTBEventOccurs(def.tiberium.reproduceDays, GenDate.TicksPerDay, GenTicks.TickLongInterval))
                    {
                        TryToSpread();
                    }                 
                }
            }
            if (!updatedTerrain && LifeStage == TiberiumLifeStage.ProducingTerrain)
            {
                TerrainDef terrain = Position.GetTerrain(Map);
                if (!terrain.HasTag("Tiberium"))
                {
                    TerrainDef def = GenTiberium.CorruptedOutcomesFor(this.def, terrain, out TiberiumCrystalDef crystaldef);
                    if (def != null)
                    {
                        Map.terrainGrid.SetTerrain(this.Position, def);
                    }
                    updatedTerrain = true;
                }
            }
            if (!Suppressed)
            {
                if (LifeStage == TiberiumLifeStage.Evolving && Evolution < 1f)
                {
                    evolution += EvolutionPerTick;
                }
                if (Growth < 1f)
                {
                    growth += GrowthPerTick;
                    base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                }
            }
        }

        public bool CanBeHarvestedBy(Harvester harvester)
        {
            if(def.IsMoss)
            {
                return harvester.harvestMode == HarvestMode.Moss;
            }
            if(harvester.harvestMode == HarvestMode.Value)
            {
                return def == Manager.TiberiumInfo.MostValuableType;
            }
            return true;
        }

        public void TryToSpread()
        {
            GenTiberium.SpreadTiberium(this);
        }

        public MapComponent_Tiberium Manager => Map.GetComponent<MapComponent_Tiberium>();
        public TiberiumProducer Parent => parent;

        public TiberiumLifeStage LifeStage
        {
            get
            {
                if (Growth > 0.99f)
                {
                    return TiberiumLifeStage.Evolving;
                }
                if (Growth > 0.66f)
                {
                    return TiberiumLifeStage.Mature;
                }
                if (Growth > 0.26f)
                {
                    return TiberiumLifeStage.ProducingTerrain;
                }
                return TiberiumLifeStage.Growing;
            }
        }

        public float HarvestValue => (Growth * def.tiberium.harvestValue);

        public int HarvestTime
        {
            get
            {
                float val = ((float)GenTicks.SecondsToTicks(def.tiberium.harvestTime)) * growth;
                return (int)Math.Round(val, 0, MidpointRounding.AwayFromZero);
            }
        }

        public float Evolution
        {
            get
            {
                return this.evolution;
            }
            set
            {
                evolution = Mathf.Clamp01(value);
            }
        }

        public float EvolutionPerTick
        {
            get
            {
                float value =  1f / ((GenDate.TicksPerDay * def.tiberium.evolutionDays) / GenTicks.TickLongInterval);
                value *= (float)GenAdjFast.AdjacentCells8Way(Position).Where(c => c.GetTiberium(Map) != null).Count() / 8f;
                return value;
            }
        }

        public float Growth
        {
            get
            {
                return growth;
            }
            set
            {
                growth = Mathf.Clamp01(value);
            }
        }

        protected float GrowthPerTick
        {
            get
            {
                float times = (GenDate.TicksPerDay * def.tiberium.growDays) / GenTicks.TickLongInterval;
                float num = 1f / times;
                float pt = growth + num;
                if (pt > 1f)
                {
                    num = num - (pt - 1f);
                }
                return num * this.GrowthRate;
            }
        }

        public virtual float GrowthRate
        {
            get
            {
                float rate = 1f;
                rate *= TiberiumRimSettings.settings.GrowthRate;
                rate *= Mathf.Lerp(1f, 0f, Map.mapTemperature.OutdoorTemp / def.tiberium.minTemperature);
                return rate;
            }
        }

        public bool GrowthLocked
        {
            get
            {
                if (maxGrowDistance > 0f)
                {
                    if (Position.DistanceTo(parent.Position) >= maxGrowDistance)
                    {
                        return true;
                    }
                }
                if(maxFriends > 0)
                {
                    if(HasParent && parent.boundCrystals.Count >= maxFriends)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HasParent => !parent.DestroyedOrNull();
        public bool Suppressed => Manager.SuppressedCells.Contains(Position);
        public bool Dormant => dormant || Suppressed;

        public bool HarvestableNow
        {
            get
            {
                return LifeStage != TiberiumLifeStage.Growing && def.tiberium.harvestValue > 0f;
            }
        }

        public override void Draw()
        {
            return;
            //base.Draw();
        }

        public bool Reprint
        {
            get
            {
                if(!printedOnce)
                {
                    return true;
                }
                return false;
            }
        }

        public override void Print(SectionLayer layer)
        {
            if (Reprint)
            {
                Log.Message("Printing.");
                Find.CameraDriver.StartCoroutine(PrintProcessor(layer));
                printedOnce = true;
            }
        }

        private IEnumerator PrintProcessor(SectionLayer layer)
        {
            Vector3 a = this.TrueCenter();
            Rand.PushState();
            Rand.Seed = base.Position.GetHashCode();
            int num = Mathf.CeilToInt(this.growth * (float)this.def.tiberium.MeshCount);
            if (num < 1)
            {
                num = 1;
            }
            float num2 = this.def.tiberium.sizeRange.LerpThroughRange(this.growth);
            float num3 = this.def.graphicData.drawSize.x * num2;
            Vector3 vector = Vector3.zero;
            int num4 = 0;
            int[] positionIndices = TiberiumPosIndices.GetPositionIndices(this);
            bool flag = false;
            foreach (int num5 in positionIndices)
            {
                if (this.def.tiberium.MeshCount == 1)
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
                    int maxMeshCount = this.def.tiberium.MeshCount;
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
                                        Log.Error(this.def + " must have thing.MaxMeshCount that is a perfect square.", false);
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
                Vector2 vector2 = new Vector2(num3, num3);
                Vector3 center = vector;
                Vector2 size = vector2;
                Material mat = matSingle;
                bool flipUv = @bool;
                Printer_Plane.PrintPlane(layer, center, size, mat, 0f, flipUv, null, new Color32[4], 0.1f, (float)(this.HashOffset() % 1024));
                num4++;
                if (num4 >= num)
                {
                    break;
                }
            }
            Rand.PopState();
            yield return null;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            if (Dormant)
            {
                sb.AppendLine("DormantTiberium".Translate());
            }
            sb.AppendLine("PercentGrowth".Translate(growth.ToStringPercent()));
            if(Growth < 1f)
            {
                sb.AppendLine("Growth".Translate() + ": " + GrowthRate.ToStringPercent());
            }
            if (HarvestableNow)
            {
                sb.AppendLine("HarvestValue".Translate() + ": " + Mathf.RoundToInt(HarvestValue));
            }
            if (DebugSettings.godMode)
            {
                sb.AppendLine("Generation: " + generation);
            }
            /*
            if (DyingInhibtion)
            {
                //stringBuilder.AppendLine("DyingToInhibition".Translate());
            }
            
            return sb.ToString().TrimEndNewlines();
        }
    }
    */
}
