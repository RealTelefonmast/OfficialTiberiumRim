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
        Spreading,
        Evolving
    }

    public class TiberiumCrystal : DiscoverableThing
    {
        public new TiberiumCrystalDef def;
        private TiberiumProducer parent;

        private float growth = 0.001f;
        private float growthPerTick;

        private bool hasSpread;

        private bool isRootNode;
        private bool inParentRange;
        private bool inSpreadRange = true;
        private bool dormantInt = false;
        private int generation;

        private float parentSpreadRange = -1;

        public void PreSpawnSetup(TiberiumProducer parent, int gen = 0)
        {
            this.parent = parent;
            this.generation = gen;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            def = (TiberiumCrystalDef)base.def;
            growthPerTick = 1f / ((GenDate.TicksPerDay * def.tiberium.growDays) / GenTicks.TickLongInterval);

            TiberiumMapComp.RegisterTiberiumCrystal(this);
            if (!respawningAfterLoad)
            {
                isRootNode = def.tiberium.smoothSpread || TRUtils.Chance(0.02f);
            }
            if (!HasParent) return;
            if (!respawningAfterLoad)
            {
                parentSpreadRange = TRUtils.Range(parent.def.spawner.spreadRange);
            }
            parent.AddBoundCrystal(this);

            var distance = Position.DistanceTo(parent.Position);
            inParentRange = distance <= parent.GrowthRadius;
            inSpreadRange = parentSpreadRange < 0 || distance <= parentSpreadRange;
            isRootNode |= inParentRange;

            if (inParentRange)
            {
                foreach (var cell in Position.CellsAdjacent8Way())
                {
                    if (cell.InBounds(Map) && GenTiberium.ForceGrowAt(cell, map))
                        cell.GetPlant(Map)?.DeSpawn();
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            TiberiumMapComp.DeregisterTiberiumCrystal(this);
            if (def.dead != null)
                Map.terrainGrid.SetTerrain(Position, def.dead);
            if (HasParent)
                parent.RemoveBoundCrystal(this);
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref growth, "growth");
            Scribe_Values.Look(ref parentSpreadRange, "spreadRange");
            Scribe_Values.Look(ref generation, "generation");
            Scribe_Values.Look(ref isRootNode, "isRootNode");
            Scribe_Values.Look(ref hasSpread, "hasSpread");
            Scribe_Values.Look(ref dormantInt, "dormantInt");
            Scribe_References.Look(ref parent, "parent");
        }

        public TiberiumProducer Parent => parent;

        //We track if the crystal already spread once
        public bool HasSpread => !isRootNode && hasSpread;

        public bool IsSelfSufficient => !def.tiberium.needsParent || HasParent;
        public bool HasParent => !parent.DestroyedOrNull();
        public bool HarvestableNow => LifeStage != TiberiumLifeStage.Growing && def.tiberium.harvestValue > 0f;
        private bool NeedsTerrain => !Position.GetTerrain(Map).IsTiberiumTerrain();
        private bool ShouldSpread => !HasSpread && TiberiumMapComp.TiberiumInfo.CanGrowFrom(Position);
        private bool Suppressed => TiberiumMapComp.Suppression.IsInSuppressorField(Position);

        public bool Dormant
        {
            get => dormantInt || !IsSelfSufficient || Suppressed || !inSpreadRange;
            set => dormantInt = value;
        }

        public float HarvestValue => (Growth * def.tiberium.harvestValue);

        public float Growth
        {
            get => Mathf.Clamp01(growth);
            set => growth = Mathf.Clamp01(value);
        }

        public float GrowthPerTick => growthPerTick * GrowthRate;

        [TweakValue("CrystalGrowth", 0f, 100f)]
        public static int CrystalGrowthVal = 1;

        public virtual float GrowthRate
        {
            get
            {
                float rate = 1f;
                rate *= TiberiumRimSettings.settings.GrowthRate;
                rate *= CrystalGrowthVal;
                //rate *= Mathf.InverseLerp(def.tiberium.minTemperature, 2000, Map.mapTemperature.OutdoorTemp);
                return rate;
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
                if (Growth > 0.85f)
                {
                    return TiberiumLifeStage.Spreading;
                }
                if (Growth > 0.26f)
                {
                    return TiberiumLifeStage.ProducingTerrain;
                }
                return TiberiumLifeStage.Growing;
            }
        }

        public void Harvest(Harvester harvester, float growth)
        {
            if (harvester.Container.TryAddValue(def.TiberiumValueType, 1, out float actualValue))
            {
                float adj = growth * (actualValue / 1);
                Growth -= adj;

                if(HarvestValue < 1)
                    TiberiumMapComp.HarvesterInfo.SetHarvestableBool(this, false);
            }
        }

        public void Harvested()
        {
            this.Destroy(DestroyMode.Vanish);
        }

        private void IncrementSpread()
        {
            hasSpread = true;
        }

        public override void TickLong()
        {
            TiberiumTick(GenTicks.TickLongInterval);
        }

        public void TiberiumTick(int interval)
        {
            StateTick();
            if (Dormant) return;
            GrowthTick();
            if (!ShouldSpread) return;
            SpreadTick(interval);
        }

        private void StateTick()
        {
            if (!HasParent && def.tiberium.dependsOnProducer)
                TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 3));
        }

        private void GrowthTick()
        {
            if (Growth >= 1f) return;
            Growth += GrowthPerTick;

            //Set Harvestable
            if(!TiberiumMapComp.HarvesterInfo.HarvestableAt(Position) && def.HarvestType != HarvestType.Unharvestable && HarvestValue >= 1)
                TiberiumMapComp.HarvesterInfo.SetHarvestableBool(this, true);

            //Set Terrain
            if (!NeedsTerrain || LifeStage != TiberiumLifeStage.ProducingTerrain) return;
            def.SpreadOutcomesAt(Position, Map, out TerrainDef top, out _, out _);
            if (top != null)
                Map.terrainGrid.SetTerrain(Position, top);
        }

        private void SpreadTick(int checkDuration)
        {
            if (LifeStage < TiberiumLifeStage.Spreading) return;
            if (Rand.MTBEventOccurs(def.tiberium.reproduceDays, GenDate.TicksPerDay, checkDuration))
            {
                GenTiberium.TrySpreadTiberium(this);
                IncrementSpread();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            if (Dormant)
                sb.AppendLine("TR_TibDormant".Translate());

            sb.AppendLine("PercentGrowth".Translate(Growth.ToStringPercent()));
            if (Growth < 1f)
                sb.AppendLine("Growth".Translate() + ": " + GrowthRate.ToStringPercent());
            if (HarvestableNow)
                sb.AppendLine("TR_TibHarvestValue".Translate() + ": " + Mathf.RoundToInt(HarvestValue));
            
            if (DebugSettings.godMode)
            {
                sb.AppendLine("Is Root Node: " + isRootNode);
                sb.AppendLine("ShouldSpread: " + ShouldSpread);
                sb.AppendLine("Exact Value: " + HarvestValue);
                sb.AppendLine("Tiberium Type: " + def.HarvestType);
                sb.AppendLine("Generation: " + generation);
            }
            return sb.ToString().TrimEndNewlines();
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
            foreach (int num5 in positionIndices)
            {
                if (this.def.tiberium.MeshCount == 1)
                {
                    vector = a + Gen.RandomHorizontalVector(0.05f);
                    float num6 = (float)base.Position.z;
                    if (vector.z - num2 / 2f < num6)
                    {
                        vector.z = num6 + num2 / 2f;
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
    }
}
