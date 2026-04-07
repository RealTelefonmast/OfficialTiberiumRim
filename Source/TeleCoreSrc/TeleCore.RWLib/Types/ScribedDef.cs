using Verse;

namespace TeleCore.RWLib;

public class ScribedDef<T> : IExposable where T : Def, new()
{
    public T def;

    public ScribedDef(T def)
    {
        this.def = def;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "def");
    }

    public static implicit operator T(ScribedDef<T> tDef)
    {
        return tDef.def;
    }

    public static implicit operator ScribedDef<T>(T def)
    {
        return new ScribedDef<T>(def);
    }
}

public class BuildableDefScribed : IExposable
{
    [Unsaved] private BuildableDef buildableDef;

    private TerrainDef terrainDef;
    private ThingDef thingDef;

    public BuildableDefScribed()
    {
    }

    public BuildableDefScribed(BuildableDef bDef)
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

    public static implicit operator BuildableDef(BuildableDefScribed bScribed)
    {
        return bScribed.buildableDef;
    }

    public static implicit operator BuildableDefScribed(BuildableDef bDef)
    {
        return new BuildableDefScribed(bDef);
    }

    public override int GetHashCode()
    {
        return buildableDef.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is BuildableDefScribed bScribed) return bScribed.buildableDef.Equals(buildableDef);
        return false;
    }
}