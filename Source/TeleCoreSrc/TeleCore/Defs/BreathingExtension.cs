using System.Collections.Generic;
using Verse;

namespace TeleCore.Defs;

public class BreathingExtension : DefModExtension
{
    public List<BreathingRequirements> requiredAtmospheres;

    public void PostLoad()
    {
    }
}

public class BreathingRequirements
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