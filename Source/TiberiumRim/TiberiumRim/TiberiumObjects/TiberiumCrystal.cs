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
        Spreading,
        Evolving
    }

    public class TiberiumCrystal : DiscoverableThing
    {
        public new TiberiumCrystalDef def => (TiberiumCrystalDef)base.def;
        private TiberiumProducer parent;

        protected float growthInt = 0.05f;
        protected int ageInt;
        protected int generationInt;
        protected bool dormantInt = false;

        protected float distanceToParent = -1;

        //Tweak Crap
        [TweakValue("CrystalGrowth", 0f, 100f)]
        public static int CrystalGrowthVal = 1;

        public TiberiumProducer Parent => parent;

        public bool HasParent => !parent.DestroyedOrNull();
        public bool OutOfParentRange => HasParent && distanceToParent > parent.def.SpreadRange;
        public bool IsSelfSufficient => !def.props.needsParent || HasParent;
        public bool HarvestableNow => LifeStage != TiberiumLifeStage.Growing && HarvestValue > 1f;
        private bool NeedsTerrain => !Position.GetTerrain(Map).IsTiberiumTerrain();
        public bool ShouldSpread => Position.CanGrowFrom(Map);

        //TODO?: Suppression with radial dropoff?
        private bool Suppressed => def.props.canBeInhibited &&  Position.IsSuppressed(Map);

        public bool Dormant
        {
            get => dormantInt || !IsSelfSufficient;
            set => dormantInt = value;
        }

        public float HarvestValue => (Growth * def.props.harvestValue);

        //public float ParentSpreadRange => parent


        public float Growth
        {
            get => growthInt;
            set => growthInt = Mathf.Clamp01(value);
        }

        public float GrowthPerTick => def.GrowthPerTick * GrowthRate;

        public virtual float GrowthRate
        {
            get
            {
                float rate = 1f;
                rate *= TiberiumRimSettings.settings.GrowthRate;
                rate *= CrystalGrowthVal;
                //TODO: Suppression dropoff affecting growthrate, rather than fully suppressing
                //rate *= Mathf.InverseLerp(def.props.minTemperature, 2000, Map.mapTemperature.OutdoorTemp);
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
                if (Growth > 0.8f)
                {
                    return TiberiumLifeStage.Spreading;
                }
                return TiberiumLifeStage.Growing;
            }
        }

        public void PreSpawnSetup(TiberiumProducer parent, int gen = 0)
        {
            this.parent = parent;
            this.generationInt = gen;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TiberiumMapComp.RegisterTiberiumCrystal(this);

            Position.GetPlant(map)?.DeSpawn();

            if (HasParent)
            {
                parent.AddBoundCrystal(this);
                distanceToParent = parent.Position.DistanceTo(Position);
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
            Scribe_Values.Look(ref growthInt, "growth");
            Scribe_Values.Look(ref generationInt, "generation");
            Scribe_Values.Look(ref dormantInt, "dormantInt");
            Scribe_References.Look(ref parent, "parent");
        }

        private Effecter_MoteMaker intmaker;
        private Effecter_MoteMaker effecter
        {
            get
            {
                return intmaker ??= new Effecter_MoteMaker(DefDatabase<EffecterDefTR>.GetNamed("TiberiumClouds_Green2"));
            }
        }

        //Main Tick Call for Tiberium
        public override void TickLong()
        {
            TiberiumTick(GenTicks.TickLongInterval);
            /*
            if (!parent.TiberiumField.MarkedForGrowth)
            {
                var targetInfo = new TargetInfo(Position, Map, false);
                effecter.EffectTick(targetInfo, targetInfo);
            }
            */
        }

        public void TiberiumTick(int interval)
        {
            StateTick();
            if (Dormant && Suppressed) return;
            GrowthTick();
            if (!ShouldSpread) return;
            SpreadTick(interval);
        }

        private void StateTick()
        {
            if (!HasParent && def.props.dependsOnProducer)
                TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 3));
        }

        private void GrowthTick()
        {
            if (Growth >= 1f && !NeedsTerrain) return;
            Growth += GrowthPerTick;

            //Set Terrain
            if(NeedsTerrain)
                DoCorruption();
        }

        private void SpreadTick(int checkDuration)
        {
            if (LifeStage < TiberiumLifeStage.Spreading) return;
            if (Rand.MTBEventOccurs(def.props.reproduceDays, GenDate.TicksPerDay, checkDuration))
            {
                GenTiberium.TrySpreadTiberium(this);
            }
        }

        private void DoCorruption()
        {
            GenTiberium.SetTerrain(Position, Map, def);
            foreach (var pos in GenAdjFast.AdjacentCells8Way(Position))
            {
                if (!pos.InBounds(Map) || (pos.GetFirstBuilding(Map) != null)) continue;

                //Try Mutate Plant
                if (!TRUtils.Chance(def.props.plantMutationChance)) continue;
                if (GenTiberium.TryMutatePlant(pos.GetPlant(Map), def)) continue;

                //TryCreateTerrain
                if (!TRUtils.Chance(Mathf.Pow(def.props.plantMutationChance, 2))) continue;
                GenTiberium.SetTerrain(pos, Map, def);
            }
        }

        public void Harvest(Harvester harvester, float growth)
        {
            if (harvester.Container.TryAddValue(def.TiberiumValueType, 1, out float actualValue))
            {
                float adj = growth * (actualValue / 1);
                Growth -= adj;
            }
        }

        //
        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            if (Dormant)
                sb.AppendLine("TR_TibDormant".Translate());
            if (Suppressed)
                sb.AppendLine("TR_TibSuppressed");

            sb.AppendLine("PercentGrowth".Translate(Growth.ToStringPercent()));
            if (Growth < 1f)
                sb.AppendLine("Growth".Translate() + ": " + GrowthRate.ToStringPercent());
            if (HarvestableNow)
                sb.AppendLine("TR_TibHarvestValue".Translate() + ": " + Mathf.RoundToInt(HarvestValue));

            if (DebugSettings.godMode)
            {
                sb.AppendLine("Is Root Node: " );
                sb.AppendLine("ShouldSpread: " + ShouldSpread);
                sb.AppendLine("Exact Value: " + HarvestValue);
                sb.AppendLine("Tiberium Type: " + def.HarvestType);
                sb.AppendLine("Generation: " + generationInt);
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Try Spread",
                action = delegate { GenTiberium.TrySpreadTiberium(this); }
            };

            yield return new Command_Action()
            {
                defaultLabel = "Grow",
                action = delegate { Growth += 1; }
            };

        }

        public override void Print(SectionLayer layer)
        {
            Vector3 a = this.TrueCenter();
            Rand.PushState();
            Rand.Seed = base.Position.GetHashCode();
            int num = Mathf.CeilToInt(this.Growth * (float)this.def.props.MeshCount);
            if (num < 1)
            {
                num = 1;
            }
            float num2 = this.def.props.sizeRange.LerpThroughRange(this.Growth);
            float num3 = this.def.graphicData.drawSize.x * num2;
            Vector3 vector = Vector3.zero;
            int num4 = 0;
            int[] positionIndices = TiberiumPosIndices.GetPositionIndices(this);
            foreach (int num5 in positionIndices)
            {
                if (this.def.props.MeshCount == 1)
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
                    int maxMeshCount = this.def.props.MeshCount;
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
