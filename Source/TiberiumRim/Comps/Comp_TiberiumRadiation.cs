using System.Collections.Generic;
using System.Linq;
using TR.Effects;
using TR.Interfaces;
using Verse;

namespace TR;

public class ThingComp_TiberiumRadiation : ThingComp, IRadiationSource
{
    public CompProperties_TiberiumRadiation Props => props as CompProperties_TiberiumRadiation;
    public IntVec3 ParentPos { get; set; }

    protected bool IsRadiating { get; private set; }

    protected virtual bool ShouldRadiate => true;
    protected virtual bool ShouldGlow => true;
    public List<IntVec3> AffectedCells { get; set; }
    public Thing SourceThing => parent;

    //On Spawn:     First Reset | Spawn Thing           | Set New
    //OR:           Spawn Thing | Reset(ignore thing)   | Set New (with thing)
    //On Despawn:   First Reset | Despawn               | Set New

    public void Notify_BuildingSpawned(Building building)
    {
        Reset(parent.Map, building);
        SetRadiation();
    }

    public void Notify_BuildingDespawning(Building building)
    {
        Reset(parent.Map);
    }

    public void Notify_UpdateRadiation()
    {
        SetRadiation();
    }

    public bool AffectsCell(IntVec3 pos)
    {
        return AffectedCells.Contains(pos);
    }

    //On Spawn:     First Reset | Spawn Thing           | Set New
    //OR:           Spawn Thing | Reset(ignore thing)   | Set New (with thing)
    //On Despawn:   First Reset | Despawn               | Set New

    public void Notify_BuildingSpawned(Building building)
    {
        Reset(parent.Map, building);
        SetRadiation();
    }

    public void Notify_BuildingDespawning(Building building)
    {
        Reset(parent.Map);
    }

    public void Notify_UpdateRadiation()
    {
        SetRadiation();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        ParentPos = parent.Position;
        AffectedCells = GenRadial.RadialCellsAround(ParentPos, Props.radius, true).Where(c => c.InBounds(parent.Map))
            .ToList();
        TryStartRadiating();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map);
        if (IsRadiating)
        {
            map.Tiberium().TiberiumAffecter.HediffGrid.Notify_SourceDespawned(this);
            Reset(map);
        }
    }

    public void TryStartRadiating()
    {
        if (ShouldRadiate && !IsRadiating)
        {
            parent.Map.Tiberium().TiberiumAffecter.HediffGrid.Notify_SourceSpawned(this);
            SetRadiation();
        }
    }

    private void Reset(Map map, Thing thingToIgnore = null)
    {
        if (!IsRadiating)
        {
            Log.Error(SourceThing + " trying to reset radiation before setting radiation!");
            return;
        }

        var affecter = map.Tiberium().TiberiumAffecter;
        foreach (var cell in AffectedCells)
        {
            if (!cell.InBounds(map)) continue;
            var intensity = RadiationAt(cell, map, thingToIgnore);
            if (intensity < 0) continue;
            affecter.AddRadiation(cell, -intensity);
        }

        radiating = false;
    }

    private void SetRadiation()
    {
        if (IsRadiating)
        {
            Log.Error(SourceThing + " trying to set radiation after already setting radiation!");
            return;
        }

        var affecter = parent.Map.Tiberium().TiberiumAffecter;
        foreach (var cell in AffectedCells)
        {
            if (!cell.InBounds(parent.Map)) continue;
            var intensity = RadiationAt(cell, parent.Map);
            if (intensity < 0) continue;
            affecter.AddRadiation(cell, intensity);
        }

        IsRadiating = true;
    }

    private float RadiationAt(IntVec3 pos, Map map, Thing thingToIgnore = null)
    {
        var line = new ShootLine(ParentPos, pos);
        float intensity = 1;
        var fraction = ParentPos.DistanceTo(pos) / Props.radius / line.Points().Count();
        foreach (var point in line.Points())
        {
            intensity -= fraction;
            var building = point.GetFirstBuilding(map);
            var penis = 1;
            if (thingToIgnore != null && thingToIgnore == building) continue;
            if (building != null && building != parent)
                /*float fallOff = FillPercentFactor(building) * (1f - (building.def.MadeFromStuff
                    ? TiberiumDefOf.RadiationResistances.FactorFor(building.Stuff)
                    : 0.2f));
                float fallOff = FillPercentFactor(building);*/
                intensity -= intensity * (FillPercentFactor(building) * StuffFactor(building));
        }

        return intensity * Props.intensity;
    }

    private float FillPercentFactor(Building building)
    {
        return (building.def.fillPercent > 0 ? building.def.fillPercent : building.def.IsEdifice() ? 0.2f : 0) /
               building.def.size.Area;
    }

    private float StuffFactor(Building building)
    {
        return TiberiumDefOf.RadiationResistances.FactorFor(building.Stuff);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!parent.Spawned) return;
        if (parent.IsHashIntervalTick(250) && ShouldGlow)
            TiberiumFX.ThrowTiberiumGlow(AffectedCells.RandomElement(), parent.Map, Rand.Range(1.25f, 1.85f));

        if (!IsRadiating && ShouldRadiate) TryStartRadiating();
    }

    public override void CompTickRare()
    {
        base.CompTickRare();
        if (ShouldGlow)
            TiberiumFX.ThrowTiberiumGlow(AffectedCells.RandomElement(), parent.Map, Rand.Range(1.25f, 1.85f));
        if (!IsRadiating && ShouldRadiate) TryStartRadiating();
    }
}

public class CompProperties_TiberiumRadiation : CompProperties
{
    public float intensity = 1f;
    public float leakDamageThreshold = -1;
    public float radius = 1f;

    public CompProperties_TiberiumRadiation()
    {
        compClass = typeof(ThingComp_TiberiumRadiation);
    }
}