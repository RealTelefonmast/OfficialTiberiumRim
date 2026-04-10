using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TR;

public enum TiberiumLifeStage
{
    Growing,
    Spreading,
    Evolving
}

public class TiberiumCrystal : TRThing, IPathFindCostProvider
{
    private const ushort Cost_TiberiumAvoid = 800;

    private static Color32[] workingColors = new Color32[4];

    //Tweak Crap
    [TweakValue("[TR]CrystalGrowth")] public static int CrystalGrowthVal = 1;

    protected int ageInt;

    private Color32? colorInt;

    private Color32[] colorsInt = new Color32[4];

    protected float distanceToParent = -1;
    protected bool dormantInt;
    protected int generationInt;

    protected float growthInt = 0.05f;

    private Effecter_MoteMaker makerInt;
    private TiberiumProducer parent;

    private TiberiumField parentField;

    public new TiberiumCrystalDef def => (TiberiumCrystalDef)base.def;

    public TiberiumProducer Parent => parent;

    public bool HasParent => !Parent.DestroyedOrNull();

    public bool OutOfParentRange =>
        HasParent && Parent.def.SpreadRange > 0 && distanceToParent > Parent.def.SpreadRange;

    public bool IsSelfSufficient => !def.tiberium.needsParent || HasParent;
    public bool HarvestableNow => LifeStage != TiberiumLifeStage.Growing && HarvestValue >= 1f;
    private bool NeedsTerrain => !Position.GetTerrain(Map).IsTiberiumTerrain();
    public bool ShouldSpread => Position.CanGrowFrom(Map) && !Dormant && !OutOfParentRange;

    private bool SpreadLocked =>
        Parent != null && !(Parent.ShouldSpawnTiberium || Parent.TiberiumField.MarkedForFastGrowth);

    //TODO?: Suppression with radial dropoff?
    private bool Suppressed => def.tiberium.canBeInhibited && Position.IsSuppressed(Map);

    public bool Dormant
    {
        get => dormantInt || !IsSelfSufficient || SpreadLocked;
        set => dormantInt = value;
    }

    public float HarvestValue => Growth * def.tiberium.harvestValue;

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
            var rate = 1f;
            rate *= TiberiumSettings.Settings.GrowthRate;
            rate *= CrystalGrowthVal;
            //TODO: Suppression dropoff affecting growthrate, rather than fully suppressing
            //rate *= Mathf.InverseLerp(def.tiberium.minTemperature, 2000, Map.mapTemperature.OutdoorTemp);
            return rate;
        }
    }

    public TiberiumLifeStage LifeStage
    {
        get
        {
            if (Growth > 0.99f) return TiberiumLifeStage.Evolving;

            if (Growth > 0.8f) return TiberiumLifeStage.Spreading;

            return TiberiumLifeStage.Growing;
        }
    }

    private Effecter_MoteMaker TibFogEffecter
    {
        get
        {
            return makerInt ??= new Effecter_MoteMaker(DefDatabase<EffecterDefTR>.GetNamed("TiberiumClouds_Green2"));
        }
    }

    private Color32 MainColor
    {
        get
        {
            if (colorInt == null)
            {
                var color = Color.white; //* Mathf. Map.fertilityGrid.FertilityAt(Position);
                color.a = 1;
                colorInt = color;
            }

            return colorInt.Value;
        }
    }

    public ushort PathFindCostFor(Pawn pawn)
    {
        if (!pawn.CanBeAffectedByTib())
            return 0;
        return Cost_TiberiumAvoid;
    }

    public CellRect GetOccupiedRect()
    {
        return this.OccupiedRect();
    }

    public void PreSpawnSetup(TiberiumProducer parent, int gen = 0, bool dormant = false)
    {
        this.parent = parent;
        generationInt = gen;
        Dormant = dormant;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        TiberiumMapComp.RegisterTiberiumCrystal(this);
        Position.GetPlant(map)?.DeSpawn();

        SetHealthEffects();

        if (HasParent)
        {
            Parent.AddBoundCrystal(this);
            distanceToParent = Parent.Position.DistanceTo(Position);
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        SetHealthEffects(true);
        TiberiumMapComp.DeregisterTiberiumCrystal(this);
        if (def.dead != null)
            Map.terrainGrid.SetTerrain(Position, def.dead);
        if (HasParent)
            Parent.RemoveBoundCrystal(this);
        base.DeSpawn(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref growthInt, "amount");
        Scribe_Values.Look(ref generationInt, "generation");
        Scribe_Values.Look(ref dormantInt, "dormantInt");
        Scribe_References.Look(ref parent, "parent");
    }

    //Main Tick Call for Tiberium
    public override void TickLong()
    {
        TiberiumTick(GenTicks.TickLongInterval);

        if (!(parent?.TiberiumField?.MarkedForFastGrowth ?? false))
        {
            //var targetInfo = new TargetInfo(Position, Map, false);
            //TibFogEffecter.Tick(this, this);
        }
    }

    public void TiberiumTick(int interval)
    {
        //Check for current state
        StateTick();
        if (Suppressed) return;
        //Grow the tib
        GrowthTick();
        //During growth, this may despawn
        if (!Spawned) return;
        //Try to spread
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
        if (Growth >= 1f && !NeedsTerrain) return;
        Growth += GrowthPerTick;

        //Set Terrain
        if (NeedsTerrain)
            DoCorruption();
    }

    private void SpreadTick(int interval)
    {
        if (LifeStage < TiberiumLifeStage.Spreading) return;
        if (Rand.MTBEventOccurs(def.tiberium.reproduceDays, GenDate.TicksPerDay, interval))
            if (CanSpreadNow())
                GenTiberium.TrySpreadTiberium(this);
    }

    protected virtual bool CanSpreadNow()
    {
        return true;
    }

    private void DoCorruption()
    {
        GenTiberium.SetTerrain(Position, Map, def);
        foreach (var pos in GenAdjFast.AdjacentCells8Way(Position))
        {
            if (!pos.InBounds(Map) || pos.GetFirstBuilding(Map) != null) continue;

            //Try Mutate Plant
            if (!TRandom.Chance(def.tiberium.plantMutationChance)) continue;
            if (GenTiberium.TryMutatePlant(pos.GetPlant(Map), def))
            {
                if (Rand.Chance(0.01f))
                {
                    var mapComp = TiberiumMapComp;
                    var mapRef = mapComp.map;

                    //Once we mutate a plant, lets create garden
                    Predicate<IntVec3>
                        pred = c => Parent.TiberiumField.Contains(c) &&
                                    Rand.Chance(
                                        0.5f); //c.InBounds(mapRef) && c.GetEdifice(mapRef) == null && (c.GetTerrain(mapRef).IsSoil() || c.GetTerrain(mapRef).IsWater);
                    var action = delegate(IntVec3 c)
                    {
                        if (c.GetTerrain(mapRef).IsWater)
                        {
                            mapRef.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.BlueTiberiumWater);
                            return;
                        }

                        c.GetTiberium(mapRef)?.DeSpawn();
                        var flora = parent?.Ruleset.PlantAt(Rand.Value, 1);
                        GenSpawn.Spawn(flora ?? TiberiumDefOf.TiberiumGrass, c, mapRef);
                        //c.GetPlant(mapRef)?.DeSpawn();
                        /*if (tib != null)
                        {
                            tib.DeSpawn();
                            var hasOut = def.GetFloraOutcomes(ThingDefOf.Plant_Grass, out ThingDef toPlant, out TerrainDef _);
                            if (hasOut)
                            {
                                var newPlant = (TiberiumPlant) ThingMaker.MakeThing(toPlant);
                                newPlant.Growth += Rand.Range(0.25f, Growth);
                                GenSpawn.Spawn(newPlant, c, mapRef);
                            }
                        }
                        GenTiberium.TryMutatePlant(c.GetPlant(mapRef), def);
                        */
                        mapRef.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.TiberiumPodSoil);
                    };
                    mapComp.FloraInfo.MakeGarden(TeleFlooder.Flood(Map, Position, action, pred, Rand.Range(100, 200))
                        .ToList());
                    return;
                }

                continue;
            }

            //TryCreateTerrain
            if (!TRandom.Chance(Mathf.Pow(def.tiberium.plantMutationChance, 2))) continue;
            GenTiberium.SetTerrain(pos, Map, def);
        }
    }

    public void Harvest(Harvester harvester, float amount)
    {
        if (harvester.Container.TryAdd(def.TiberiumValueTypeForNetwork, 1, out var result))
        {
            var adj = amount * (result.Actual / 1);
            Growth -= adj;
            if (Growth <= 0.01f)
                Destroy();
        }
    }

    private void SetHealthEffects(bool despawning = false)
    {
        var value = despawning ? -1 : 1;
        if (def.IsInfective)
            Map.Tiberium().TiberiumAffecter.AddInfection(Position, value);
        if (def.tiberium.radiates)
            Map.Tiberium().TiberiumAffecter.AddRadiation(Position, value);
    }

    //
    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.Append(base.GetInspectString());
        if (Dormant)
            sb.AppendLine("TR_TibDormant".Translate());
        if (Suppressed)
            sb.AppendLine("TR_TibSuppressed".Translate());

        sb.AppendLine("PercentGrowth".Translate(Growth.ToStringPercent()));
        if (Growth < 1f)
            sb.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
        if (HarvestableNow)
            sb.AppendLine("TR_TibHarvestValue".Translate(Mathf.RoundToInt(HarvestValue)));

        if (DebugSettings.godMode)
        {
            sb.AppendLine("Parent: " + Parent);
            sb.AppendLine("Parent ShouldSpawnTib: " + Parent?.ShouldSpawnTiberium);
            sb.AppendLine("Fast Growth: " + Parent?.TiberiumField?.MarkedForFastGrowth);
            sb.AppendLine("SpreadLocked: " + SpreadLocked);
            sb.AppendLine("ShouldSpread: " + ShouldSpread);
            sb.AppendLine("IsDormant: " + Dormant);
            sb.AppendLine("Exact Value: " + HarvestValue);
            sb.AppendLine("Tiberium Type: " + def.HarvestType);
            sb.AppendLine("Generation: " + generationInt);
        }

        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos()) yield return gizmo;

        if (!DebugSettings.godMode) yield break;

        yield return new Command_Action
        {
            defaultLabel = "Try Spread",
            action = delegate { GenTiberium.TrySpreadTiberium(this); }
        };

        yield return new Command_Action
        {
            defaultLabel = "Grow",
            action = delegate { Growth += 1; }
        };
    }

    public override void Print(SectionLayer layer)
    {
        var a = this.TrueCenter();
        Rand.PushState();
        Rand.Seed = Position.GetHashCode();

        //Log.Message($"Printing tib {Position.GetHashCode()} Color: {graphicRand.MatSingle.color}");
        var num = Mathf.CeilToInt(Growth * def.tiberium.MeshCount);
        if (num < 1) num = 1;
        var num2 = def.tiberium.sizeRange.LerpThroughRange(Growth);
        var num3 = def.graphicData.drawSize.x * num2;
        var finalPos = Vector3.zero;
        var num4 = 0;
        var positionIndices = TiberiumPosIndices.GetPositionIndices(this);
        foreach (var num5 in positionIndices)
        {
            if (def.tiberium.MeshCount == 1)
            {
                finalPos = a + Gen.RandomHorizontalVector(0.05f);
                var num6 = (float)Position.z;
                if (finalPos.z - num2 / 2f < num6) finalPos.z = num6 + num2 / 2f;
            }
            else
            {
                var num7 = 1;
                var maxMeshCount = def.tiberium.MeshCount;
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
                                    Log.Error(def + " must have MaxMeshCount that is a perfect square.", false);
                                else
                                    num7 = 5;
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

                var num8 = 1f / num7;
                finalPos = Position.ToVector3();
                finalPos.y = def.Altitude;
                finalPos.x += 0.5f * num8;
                finalPos.z += 0.5f * num8;
                var num9 = num5 / num7;
                var num10 = num5 % num7;
                finalPos.x += num9 * num8;
                finalPos.z += num10 * num8;
                var max = num8 * 0.3f;
                finalPos += Gen.RandomHorizontalVector(max);
            }

            var randBool = Rand.Bool;
            var matSingle = Graphic.MatSingle;
            var size = new Vector2(num3, num3);

            Vector2[] uvs;
            //Color32 color;
            Graphic.TryGetTextureAtlasReplacementInfo(matSingle, def.category.ToAtlasGroup(), randBool, true,
                out matSingle, out uvs, out _);

            //matSingle.SetColor(ShaderPropertyIDs.Color, Color.red);
            Printer_Plane.PrintPlane(layer, finalPos, size, matSingle, 0, randBool, uvs, new[]
            {
                MainColor,
                MainColor,
                MainColor,
                MainColor
            }, 0.1f, this.HashOffset() % 1024);

            num4++;
            if (num4 >= num) break;
        }

        Rand.PopState();
    }
}