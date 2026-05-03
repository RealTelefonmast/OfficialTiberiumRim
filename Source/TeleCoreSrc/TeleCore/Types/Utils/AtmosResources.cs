using System.Collections.Generic;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public static class AtmosResources
{
    // 1000L of Air = 1m^3 of Air
    //We assume a rimworld tile has a volume of 2m^3
    public const float CELL_FLOOR = 2.25f; //1.5*1.5
    public const float CELL_HEIGHT = 2.5f;
    public const int CELL_CAPACITY = 5625; // 2.25 * 2.5 * 1000
    public const float MIN_EQ_VAL = 2;

    //[TweakValue("Atmos.Friction",0f, 1)]
    public static float Friction = 0.15f;

    //[TweakValue("Atmos.CSquared", 0, 100)]
    public static float CSquared = 10;

    public static List<AtmosphericValueDef> AllAtmosphericDefs =>
        DefDatabase<AtmosphericValueDef>.AllDefsListForReading;

    public static FlowVolumeConfig<AtmosphericValueDef> DefaultAtmosConfig(int roomSize)
    {
        return new FlowVolumeConfig<AtmosphericValueDef>
        {
            values = new FlowVolumeConfig<AtmosphericValueDef>.Values
            {
                allowedValues = AllAtmosphericDefs
            },
            capacity = roomSize * CELL_CAPACITY,
            area = 0,
            elevation = 0,
            height = 0
        };
    }

    public static FlowVolumeConfig<AtmosphericValueDef> DefaultAtmosConfigMap(int mapCellSize)
    {
        var config = DefaultAtmosConfig(mapCellSize);
        config.infiniteSource = true;
        return config;
    }
}