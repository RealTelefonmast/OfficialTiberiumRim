using System.Collections.Generic;
using TeleCore.Atmosphere.Defs;
using Verse;

namespace TeleCore.Atmosphere.Health;

public class BreathingExtension : DefModExtension
{
    public RespiratoryProperties settings;
}

public class RespiratoryProperties
{
    public AtmosphericValueDef atmosphere;
    public float baseLevel = 0.21f;
    public List<BreathingStage> stages;
}

public class BreathingStage
{
    public List<HediffGiver> hediffGivers;
    public string label;
    public float level;
}