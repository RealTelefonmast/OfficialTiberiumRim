using Verse;

namespace TeleCore.RWLib;

public class BuildableDefSerializable : IExposable
{
    [Unsaved] private BuildableDef buildableDef;

    private TerrainDef terrainDef;
    private ThingDef thingDef;

    public BuildableDefSerializable()
    {
    }

    public BuildableDefSerializable(BuildableDef bDef)
    {
        buildableDef = bDef;
    }

    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            if (buildableDef is TerrainDef tDef) terrainDef = tDef;

            if (buildableDef is ThingDef thingDef) this.thingDef = thingDef;
        }

        Scribe_Defs.Look(ref terrainDef, "terrainDef");
        Scribe_Defs.Look(ref thingDef, "thingDef");

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            if (terrainDef != null)
                buildableDef = terrainDef;

            if (thingDef != null)
                buildableDef = thingDef;
        }
    }

    public static implicit operator BuildableDef(BuildableDefSerializable bScribed)
    {
        return bScribed.buildableDef;
    }

    public static implicit operator BuildableDefSerializable(BuildableDef bDef)
    {
        return new BuildableDefSerializable(bDef);
    }

    public override int GetHashCode()
    {
        return buildableDef.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is BuildableDefSerializable bScribed) return bScribed.buildableDef.Equals(buildableDef);
        return false;
    }
}